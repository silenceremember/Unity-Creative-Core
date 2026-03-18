using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Управляет визуальной новеллой «The Smith Family».
///
/// Поток:
///  1. StartNovel() — камера мгновенно на anchor[line.cameraIndex], показать NovelCanvas, напечатать реплику 0
///  2. Пользователь нажимает «Далее»
///     а) Если у СЛЕДУЮЩЕЙ строки есть narratorSequenceBefore — скрыть NovelCanvas, дать слово рассказчику, ждать OnSequenceCompleted
///     б) После рассказчика (или сразу если его нет) — плавно перейти к новому anchor если cameraIndex изменился, показать NovelCanvas, напечатать реплику
///     в) Повторять до конца списка
///  3. По окончании — скрыть NovelCanvas, вызвать NovelChannel.NotifyCompleted()
///
///  Поддерживает прерывание через CancellationToken (например из меню паузы).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class VisualNovelManager : MonoBehaviour
{
    public static VisualNovelManager Instance { get; private set; }

    // ─── References ──────────────────────────────────────────
    [Header("SO Channels")]
    [Tooltip("Novel Channel — шина событий новеллы")]
    public NovelChannel novelChannel;

    [Tooltip("Game State Channel — для переключения состояний игры")]
    public GameStateChannel gameStateChannel;

    [Tooltip("NarratorChannel — тот же что в NarratorManager")]
    public NarratorChannel narratorChannel;

    [Header("Novel Canvas")]
    [Tooltip("Корневой объект Novel Canvas (деактивируется пока рассказчик говорит)")]
    public GameObject novelCanvasRoot;

    [Tooltip("Имя говорящего")]
    public TextMeshProUGUI speakerText;

    [Tooltip("Текст реплики")]
    public TextMeshProUGUI lineText;

    [Tooltip("Кнопка «Далее»")]
    public Button nextButton;

    [Header("Sequence")]
    public NovelSequence sequence;

    [Header("Camera")]
    [Tooltip("Main Camera")]
    public Camera mainCamera;

    [Tooltip("4 якоря камеры: 0=дверь, 1=балкон, 2=гостиная, 3=крупный план")]
    public Transform[] cameraAnchors = new Transform[4];

    [Tooltip("Длительность плавного перехода камеры (сек)")]
    [Range(0.2f, 2f)]
    public float blendDuration = 0.8f;

    public AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Аудио")]
    [Tooltip("AudioMixerGroup для голосов новеллы — управляет громкостью через микшер")]
    public AudioMixerGroup mixerGroup;

    [Header("Typewriter")]
    [Range(20, 200)]
    public float charsPerSecond = 60f;

    [Tooltip("Звук играет раз в N непробельных символов (Undertale-стиль).\n" +
             "При 60 симв/сек: 2 = 30 блипов/сек, 4 = ~15, 6 = ~10.")]
    [Range(1, 10)]
    public int blipEveryNChars = 4;

    // ─── State ───────────────────────────────────────────────
    private int _lineIndex = -1;
    private bool _waitingForNarrator = false;
    private bool _typewriterRunning = false;

    // UniTask cancellation
    private CancellationTokenSource _cts;

    // Сигнал нажатия «Далее» — UniTask ждёт этого флага
    private bool _nextPressed = false;

    // Ускорение typewriter — нажали «Далее» пока он ещё печатает
    private bool _skipTypewriter = false;

    private int _currentCameraIndex = -1;
    private AudioSource _audioSource;
    private int _blipCounter;  // считает непробельные символы между блипами

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = mixerGroup;
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
        if (novelChannel != null)
            novelChannel.OnNovelAbortRequested += AbortNovel;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (novelChannel != null)
            novelChannel.OnNovelAbortRequested -= AbortNovel;
    }

    // ─── Public API ───────────────────────────────────────────

    /// <summary>Принудительно скрыть UI новеллы (для debug-скипа)</summary>
    public void ForceHideNovelCanvas() => HideNovelCanvas();

    /// <summary>
    /// Вызывается из IntroCrawl по завершении кроула.
    /// Камера мгновенно телепортируется к anchor[0].
    /// </summary>
    public void StartNovel()
    {
        if (sequence == null || sequence.lines == null || sequence.lines.Length == 0)
        {
            Debug.LogWarning("[VisualNovelManager] Sequence is empty.");
            return;
        }

        Debug.Log("[VisualNovelManager] Novel started.");

        // Отменяем предыдущий токен на случай повторного запуска
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _lineIndex = 0;
        _currentCameraIndex = -1;
        _waitingForNarrator = false;
        _nextPressed = false;
        _skipTypewriter = false;

        RunNovelAsync(_cts.Token).Forget();
    }

    /// <summary>Принудительно прерывает новеллу (вызывается из NovelChannel или напрямую).</summary>
    public void AbortNovel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        HideNovelCanvas();
        Debug.Log("[VisualNovelManager] Novel aborted.");
    }

    /// <summary>Кнопка «Далее»</summary>
    public void OnNextButton()
    {
        if (_typewriterRunning)
        {
            _skipTypewriter = true;
            return;
        }

        _nextPressed = true;
    }

    // ─── Main Loop ────────────────────────────────────────────

    private async UniTask RunNovelAsync(CancellationToken ct)
    {
        try
        {
            // Мгновенная телепортация к первому anchor
            SnapCamera(sequence.lines[0].cameraIndex);

            // Если у первой строки есть пролог-рассказчик — сначала он
            if (sequence.lines[0].narratorSequenceBefore != null)
            {
                HideNovelCanvas();
                TriggerNarrator(sequence.lines[0].narratorSequenceBefore);
                _waitingForNarrator = true;
                await WaitForNarratorAsync(ct);
            }

            await ShowLineAndWaitAsync(ct);

            // Основной цикл
            while (true)
            {
                // Ждём нажатия «Далее»
                _nextPressed = false;
                await UniTask.WaitUntil(() => _nextPressed, cancellationToken: ct);
                _nextPressed = false;

                _lineIndex++;

                if (sequence == null || _lineIndex >= sequence.lines.Length)
                {
                    EndNovel();
                    return;
                }

                var line = sequence.lines[_lineIndex];

                if (line.narratorSequenceBefore != null)
                {
                    HideNovelCanvas();
                    _waitingForNarrator = true;

                    // Плавный переход камеры до рассказчика
                    if (line.cameraIndex != _currentCameraIndex)
                        await BlendCameraAsync(line.cameraIndex, ct);

                    TriggerNarrator(line.narratorSequenceBefore);
                    await WaitForNarratorAsync(ct);
                }

                await ShowLineAndWaitAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальная отмена (AbortNovel или выход в меню) — ничего не делаем
            Debug.Log("[VisualNovelManager] RunNovel cancelled.");
        }
    }

    /// <summary>Показывает текущую реплику (с переходом камеры если нужно).</summary>
    private async UniTask ShowLineAndWaitAsync(CancellationToken ct)
    {
        if (sequence == null || _lineIndex < 0 || _lineIndex >= sequence.lines.Length) return;

        var line = sequence.lines[_lineIndex];

        // Переход камеры
        if (line.cameraIndex != _currentCameraIndex)
        {
            if (_currentCameraIndex == -1)
            {
                // Первый вызов — snap уже был в StartNovel
                _currentCameraIndex = line.cameraIndex;
            }
            else
            {
                await BlendCameraAsync(line.cameraIndex, ct);
            }
        }

        await DisplayLineAsync(line, ct);
    }

    // ─── Line Display ─────────────────────────────────────────

    private async UniTask DisplayLineAsync(NovelLine line, CancellationToken ct)
    {
        ShowNovelCanvas();

        if (speakerText != null) speakerText.text = line.speaker;
        if (lineText != null) lineText.text = "";
        _blipCounter = 0;  // сброс на каждую реплику

        _typewriterRunning = true;
        _skipTypewriter = false;
        await TypewriterAsync(line.text, line.speaker, ct);
        _typewriterRunning = false;
    }

    // ─── Narrator ─────────────────────────────────────────────

    private void TriggerNarrator(DialogueSequence seq)
    {
        narratorChannel?.Raise(seq);
    }

    private UniTaskCompletionSource _narratorTcs;

    private UniTask WaitForNarratorAsync(CancellationToken ct)
    {
        _narratorTcs = new UniTaskCompletionSource();
        return _narratorTcs.Task.AttachExternalCancellation(ct);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (!_waitingForNarrator) return;
        _waitingForNarrator = false;
        _narratorTcs?.TrySetResult();
        _narratorTcs = null;
    }

    // ─── End / Cleanup ────────────────────────────────────────

    private void EndNovel()
    {
        HideNovelCanvas();
        novelChannel?.NotifyCompleted();
        gameStateChannel?.Raise(GameState.Gameplay);
        Debug.Log("[VisualNovelManager] Novel complete.");
    }

    // ─── Canvas ───────────────────────────────────────────────

    private void ShowNovelCanvas()
    {
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(true);
    }

    private void HideNovelCanvas()
    {
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(false);
    }

    // ─── Camera ───────────────────────────────────────────────

    /// <summary>Мгновенная телепортация</summary>
    private void SnapCamera(int anchorIndex)
    {
        if (mainCamera == null) return;
        if (anchorIndex < 0 || anchorIndex >= cameraAnchors.Length) return;
        var anchor = cameraAnchors[anchorIndex];
        if (anchor == null) return;

        mainCamera.transform.position = anchor.position;
        mainCamera.transform.rotation = anchor.rotation;
        _currentCameraIndex = anchorIndex;
    }

    /// <summary>Плавный переход камеры (async UniTask)</summary>
    private async UniTask BlendCameraAsync(int anchorIndex, CancellationToken ct)
    {
        if (mainCamera == null || anchorIndex < 0 || anchorIndex >= cameraAnchors.Length) return;

        var anchor = cameraAnchors[anchorIndex];
        if (anchor == null) return;

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < blendDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float t = blendCurve.Evaluate(Mathf.Clamp01(elapsed / blendDuration));
            mainCamera.transform.position = Vector3.Lerp(startPos, anchor.position, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, anchor.rotation, t);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        mainCamera.transform.position = anchor.position;
        mainCamera.transform.rotation = anchor.rotation;
        _currentCameraIndex = anchorIndex;
    }

    // ─── Typewriter ───────────────────────────────────────────

    private async UniTask TypewriterAsync(string text, string speaker, CancellationToken ct)
    {
        if (lineText == null) return;

        int delay = Mathf.RoundToInt(1000f / charsPerSecond);

        foreach (char c in text)
        {
            if (_skipTypewriter)
            {
                // Мгновенно дописать весь текст
                lineText.text = text;
                _skipTypewriter = false;
                _typewriterRunning = false;
                return;
            }

            ct.ThrowIfCancellationRequested();
            lineText.text += c;
            // Undertale-стиль: блип раз в N непробельных символов
            if (!char.IsWhiteSpace(c))
            {
                _blipCounter++;
                if (_blipCounter >= blipEveryNChars)
                {
                    _blipCounter = 0;
                    PlayVoiceBlip(speaker);
                }
            }
            await UniTask.Delay(delay, cancellationToken: ct);
        }
    }

    private void PlayVoiceBlip(string speaker)
    {
        if (_audioSource == null || novelChannel == null) return;
        var clip = novelChannel.GetBlip(speaker);
        if (clip == null) return;
        _audioSource.pitch = novelChannel.GetRandomPitch(speaker);
        _audioSource.PlayOneShot(clip);
    }
}
