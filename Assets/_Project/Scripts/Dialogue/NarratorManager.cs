using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Воспроизводит DialogueSequence через NarratorChannel.
/// Повесь на GameObject в сцене. Назначь channel и UI-элементы.
/// </summary>
public class NarratorManager : MonoBehaviour
{
    [Header("SO Channel")]
    public NarratorChannel channel;

    [Header("UI")]
    public GameObject subtitleRoot;   // корневой объект с фоном
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI lineText;

    [Header("Объекты сцены (activateObject)")]
    [Tooltip("Список объектов сцены, на которые ссылаются DialogueLine.activateObject. " +
             "Перетащи GameObject сюда — работает даже если объект изначально неактивен.")]
    public List<SceneObjectEntry> sceneObjects = new();

    [Header("Настройки")]
    [Range(20, 200)]
    public float charsPerSecond = 50f;
    [Range(50, 500)]
    public float eraseSpeed = 1000f;  // быстрее чем печать
    public float fadeSpeed = 4f;

    // ──────────────────────────────
    private CancellationTokenSource _cts;
    private DialogueSequence _currentSequence;
    private Dictionary<string, GameObject> _sceneObjectMap;

    // Debug skip
    private bool _skipLine;

    public static NarratorManager Instance { get; private set; }

    /// <summary>True пока воспроизводится любой диалог.</summary>
    public bool IsPlaying => _cts != null && !_cts.IsCancellationRequested;

    /// <summary>Текущая последовательность (null если ничего не играет).</summary>
    public DialogueSequence CurrentSequence => _currentSequence;

    /// <summary>Сохранённая последовательность для восстановления после restoreInterrupted=true.</summary>
    private DialogueSequence _savedSequence;

    void Update()
    {
#if UNITY_EDITOR
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.pKey.wasPressedThisFrame)
            _skipLine = true;
#endif
    }

    void Awake()
    {
        Instance = this;

        _sceneObjectMap = new Dictionary<string, GameObject>(sceneObjects.Count);
        foreach (var entry in sceneObjects)
            if (!string.IsNullOrEmpty(entry.key) && entry.gameObject != null)
                _sceneObjectMap[entry.key] = entry.gameObject;
    }

    void OnEnable()
    {
        if (channel != null)
        {
            channel.OnSequenceRequested += Play;
            channel.OnStopRequested     += Stop;
        }
    }

    void OnDisable()
    {
        if (channel != null)
        {
            channel.OnSequenceRequested -= Play;
            channel.OnStopRequested     -= Stop;
        }
    }

    void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    // ── Public API ───────────────

    public void Play(DialogueSequence sequence)
    {
        if (sequence == null) return;

        if (_currentSequence != null)
        {
            if (sequence.priority < _currentSequence.priority)
                return; // приоритет ниже — не прерываем

            // Если новая последовательность хочет восстановить прерванную — сохраняем
            if (sequence.restoreInterrupted)
                _savedSequence = _currentSequence;
        }

        Stop();
        _currentSequence = sequence;

        _cts = new CancellationTokenSource();
        PlaySequence(sequence, _cts.Token).Forget();
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        _currentSequence = null;

        // Очищаем текст чтобы не было наложений
        if (lineText != null) lineText.text = "";
        if (subtitleRoot != null) subtitleRoot.SetActive(false);
    }

    // ── Playback ─────────────────

    private async UniTask PlaySequence(DialogueSequence sequence, CancellationToken ct)
    {
        try
        {
            foreach (var line in sequence.lines)
            {
                // Стираем предыдущий текст быстро (пропускаем для первой строки)
                if (lineText != null && lineText.text.Length > 0)
                    await EraseText(ct);

                await ShowLine(line, ct);

                // Пауза между строками — прерывается по P
                float pauseLeft = line.pauseAfter;
                while (pauseLeft > 0f && !_skipLine)
                {
                    await UniTask.Yield(ct);
                    pauseLeft -= Time.deltaTime;
                }
                _skipLine = false;
            }

            // Стираем последнюю реплику в конце последовательности
            if (lineText != null && lineText.text.Length > 0)
                await EraseText(ct);

            subtitleRoot?.SetActive(false);

            // Автопереход / завершение цепочки
            if (sequence.nextSequence != null)
            {
                // Промежуточный сегмент: уведомляем пока ещё "играем" (для таймера/кликера),
                // затем сразу переходим к следующему.
                channel?.NotifyCompleted(sequence);
                _currentSequence = sequence.nextSequence;
                await PlaySequence(sequence.nextSequence, ct);
            }
            else
            {
                // Терминальный сегмент: очищаем состояние ДО NotifyCompleted,
                // чтобы подписчики могли сделать Raise нового диалога без блокировки по приоритету.
                _cts?.Dispose();
                _cts = null;
                _currentSequence = null;
                channel?.NotifyCompleted(sequence);

                // Восстанавливаем прерванный диалог (если был сохранён через restoreInterrupted)
                if (_savedSequence != null)
                {
                    var toRestore = _savedSequence;
                    _savedSequence = null;
                    channel?.Raise(toRestore);
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            // Нормальное прерывание через Stop() — UI уже очищен в Stop()
        }
    }

    private async UniTask ShowLine(DialogueLine line, CancellationToken ct)
    {
        _skipLine = false;

        // Если задан, активируем объект сцены перед показом реплики
        if (!string.IsNullOrEmpty(line.activateObject))
        {
            if (_sceneObjectMap.TryGetValue(line.activateObject, out var go))
                go.SetActive(true);
            else
                Debug.LogWarning($"[NarratorManager] activateObject '{line.activateObject}' не назначен в списке sceneObjects");
        }

        if (subtitleRoot != null) subtitleRoot.SetActive(true);
        if (speakerText != null) speakerText.text = "";
        if (lineText != null)    lineText.text = "";

        // Печатаем по символам (P — мгновенно допечатать)
        if (lineText != null)
        {
            int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / charsPerSecond));
            foreach (char c in line.text)
            {
                if (_skipLine)
                {
                    lineText.text = line.text; // допечатать всё сразу
                    break;
                }
                lineText.text += c;
                await UniTask.Delay(delayMs, cancellationToken: ct);
            }
        }

        // Ждём остаток времени (P — пропустить)
        if (!_skipLine)
        {
            float elapsed   = line.text.Length / charsPerSecond;
            float total     = line.GetDuration();
            float remaining = total - elapsed;
            if (remaining > 0f)
            {
                float endTime = Time.time + remaining;
                while (Time.time < endTime && !_skipLine)
                    await UniTask.Yield(ct);
            }
        }

        // Не сбрасываем _skipLine здесь — пусть флаг «протечёт» в pauseAfter-цикл
        // PlaySequence, который сам его сбросит.
    }

    private async UniTask EraseText(CancellationToken ct)
    {
        if (lineText == null) return;
        int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / eraseSpeed));
        while (lineText.text.Length > 0)
        {
            lineText.text = lineText.text[..^1];
            await UniTask.Delay(delayMs, cancellationToken: ct);
        }
    }
}
