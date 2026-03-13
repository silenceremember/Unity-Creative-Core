using System.Collections;
using System.Collections.Generic;
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
    private Coroutine _playback;
    private DialogueSequence _currentSequence;
    private CanvasGroup _group;
    private Dictionary<string, GameObject> _sceneObjectMap;

    // Debug skip
    private bool _skipLine;

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
        _group = subtitleRoot?.GetComponent<CanvasGroup>();

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

    // ── Public API ───────────────

    public void Play(DialogueSequence sequence)
    {
        if (sequence == null) return;

        if (_playback != null)
        {
            if (sequence.priority < (_currentSequence?.priority ?? 0))
                return; // приоритет ниже — не прерываем
        }

        Stop();
        _currentSequence = sequence;
        _playback = StartCoroutine(PlaySequence(sequence));
    }

    public void Stop()
    {
        if (_playback != null)
        {
            StopAllCoroutines(); // убиваем и дочерние (EraseText, ShowLine)
            _playback = null;
        }
        _currentSequence = null;

        // Очищаем текст чтобы не было наложений
        if (lineText != null) lineText.text = "";
        if (speakerText != null) speakerText.text = "";
        if (subtitleRoot != null) subtitleRoot.SetActive(false);
    }

    // ── Playback ─────────────────

    private IEnumerator PlaySequence(DialogueSequence sequence)
    {
        foreach (var line in sequence.lines)
        {
            // Стираем предыдущий текст быстро (пропускаем для первой строки)
            if (lineText != null && lineText.text.Length > 0)
                yield return StartCoroutine(EraseText());

            yield return StartCoroutine(ShowLine(line));

            // Пауза между строками — прерывается по P
            float pauseLeft = line.pauseAfter;
            while (pauseLeft > 0f && !_skipLine)
            {
                pauseLeft -= Time.deltaTime;
                yield return null;
            }
            _skipLine = false;
        }

        // Стираем последнюю реплику в конце последовательности
        if (lineText != null && lineText.text.Length > 0)
            yield return StartCoroutine(EraseText());

        subtitleRoot?.SetActive(false);

        // Уведомляем подписчиков о завершении ЭТОГО сегмента
        // (нужно до перехода к nextSequence, чтобы ExplorationManager
        // мог среагировать на промежуточные триггеры: таймер, кликер и т.д.)
        channel?.NotifyCompleted(sequence);

        // Автопереход
        if (sequence.nextSequence != null)
        {
            yield return StartCoroutine(PlaySequence(sequence.nextSequence));
        }
        else
        {
            // Цепочка полностью завершена — сбрасываем состояние
            _playback = null;
            _currentSequence = null;
            yield break;
        }

        _playback = null;
        _currentSequence = null;
    }

    private IEnumerator ShowLine(DialogueLine line)
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
        if (speakerText != null) speakerText.text = line.speaker;
        if (lineText != null)    lineText.text = "";

        // Печатаем по символам (P — мгновенно допечатать)
        if (lineText != null)
        {
            foreach (char c in line.text)
            {
                if (_skipLine)
                {
                    lineText.text = line.text; // допечатать всё сразу
                    break;
                }
                lineText.text += c;
                yield return new WaitForSeconds(1f / charsPerSecond);
            }
        }

        // Ждём остаток времени (P — пропустить паузу)
        if (!_skipLine)
        {
            float elapsed = line.text.Length / charsPerSecond;
            float total   = line.GetDuration();
            if (total > elapsed)
            {
                float remaining = total - elapsed;
                while (remaining > 0f && !_skipLine)
                {
                    remaining -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        _skipLine = false;
    }

    private IEnumerator EraseText()
    {
        if (lineText == null) yield break;
        float delay = 1f / eraseSpeed;
        while (lineText.text.Length > 0)
        {
            lineText.text = lineText.text[..^1]; // убираем последний символ
            yield return new WaitForSeconds(delay);
        }
    }
}
