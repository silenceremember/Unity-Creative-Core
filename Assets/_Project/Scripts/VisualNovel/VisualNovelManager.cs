using System.Collections;
using TMPro;
using UnityEngine;
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
/// </summary>
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

    [Header("Typewriter")]
    [Range(20, 200)]
    public float charsPerSecond = 60f;

    // ─── State ───────────────────────────────────────────────
    private int _lineIndex = -1;
    private bool _waitingForNarrator = false;
    private bool _typewriterRunning = false;
    private Coroutine _typewriter;
    private Coroutine _cameraBlend;
    private int _currentCameraIndex = -1;

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        if (novelCanvasRoot != null) novelCanvasRoot.SetActive(false);
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
    }

    // ─── Public API ───────────────────────────────────────────

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

        _lineIndex = 0;
        _currentCameraIndex = -1;

        // Мгновенная телепортация к первому anchor
        SnapCamera(sequence.lines[0].cameraIndex);

        // Если у первой строки есть пролог-рассказчик — сначала он
        if (sequence.lines[0].narratorSequenceBefore != null)
        {
            HideNovelCanvas();
            TriggerNarrator(sequence.lines[0].narratorSequenceBefore);
            _waitingForNarrator = true;
            // ShowCurrentLine будет вызван из OnNarratorCompleted
        }
        else
        {
            ShowCurrentLine();
        }
    }

    /// <summary>Кнопка «Далее»</summary>
    public void OnNextButton()
    {
        // Если typewriter ещё не дописал — сразу показать весь текст
        if (_typewriterRunning)
        {
            CompleteTypewriter();
            return;
        }

        AdvanceLine();
    }

    // ─── Line Logic ───────────────────────────────────────────

    private void AdvanceLine()
    {
        _lineIndex++;

        if (sequence == null || _lineIndex >= sequence.lines.Length)
        {
            EndNovel();
            return;
        }

        var line = sequence.lines[_lineIndex];

        if (line.narratorSequenceBefore != null)
        {
            // Скрыть NovelCanvas
            HideNovelCanvas();
            _waitingForNarrator = true;

            // Плавно сдвинуть камеру, затем запустить рассказчика
            if (line.cameraIndex != _currentCameraIndex)
            {
                StartCoroutine(BlendCamera(line.cameraIndex, () =>
                {
                    TriggerNarrator(line.narratorSequenceBefore);
                }));
            }
            else
            {
                TriggerNarrator(line.narratorSequenceBefore);
            }
            // Продолжение — в OnNarratorCompleted → ShowCurrentLine (камера уже на месте)
        }
        else
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (sequence == null || _lineIndex < 0 || _lineIndex >= sequence.lines.Length) return;

        var line = sequence.lines[_lineIndex];

        // Переход камеры: мгновенно если индекс тот же (или самый первый snap уже был)
        if (line.cameraIndex != _currentCameraIndex)
        {
            // Если это самый первый вызов — snap уже был в StartNovel, просто обновим индекс
            // Иначе — плавный блендинг
            if (_currentCameraIndex == -1)
            {
                _currentCameraIndex = line.cameraIndex;
            }
            else
            {
                StartCoroutine(BlendCamera(line.cameraIndex, () =>
                {
                    DisplayLine(line);
                }));
                return;
            }
        }

        DisplayLine(line);
    }

    private void DisplayLine(NovelLine line)
    {
        ShowNovelCanvas();

        if (speakerText != null) speakerText.text = line.speaker;
        if (lineText != null) lineText.text = "";

        // Запуск typewriter
        if (_typewriter != null) StopCoroutine(_typewriter);
        _typewriterRunning = true;
        _typewriter = StartCoroutine(TypewriterCoroutine(line.text, () =>
        {
            _typewriterRunning = false;
        }));
    }

    private void CompleteTypewriter()
    {
        if (_typewriter != null)
        {
            StopCoroutine(_typewriter);
            _typewriter = null;
        }
        _typewriterRunning = false;

        // Написать весь текст сразу
        if (_lineIndex >= 0 && _lineIndex < sequence.lines.Length)
        {
            if (lineText != null)
                lineText.text = sequence.lines[_lineIndex].text;
        }
    }

    private void EndNovel()
    {
        HideNovelCanvas();
        novelChannel?.NotifyCompleted();
        gameStateChannel?.Raise(GameState.Gameplay);
        Debug.Log("[VisualNovelManager] Novel complete.");
    }

    // ─── Narrator ─────────────────────────────────────────────

    private void TriggerNarrator(DialogueSequence seq)
    {
        narratorChannel?.Raise(seq);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (!_waitingForNarrator) return;
        _waitingForNarrator = false;

        ShowCurrentLine();
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

    /// <summary>Плавный переход камеры, потом callback</summary>
    private IEnumerator BlendCamera(int anchorIndex, System.Action onDone)
    {
        if (mainCamera == null || anchorIndex < 0 || anchorIndex >= cameraAnchors.Length)
        {
            onDone?.Invoke();
            yield break;
        }

        var anchor = cameraAnchors[anchorIndex];
        if (anchor == null)
        {
            onDone?.Invoke();
            yield break;
        }

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        float elapsed = 0f;

        while (elapsed < blendDuration)
        {
            elapsed += Time.deltaTime;
            float t = blendCurve.Evaluate(Mathf.Clamp01(elapsed / blendDuration));
            mainCamera.transform.position = Vector3.Lerp(startPos, anchor.position, t);
            mainCamera.transform.rotation = Quaternion.Lerp(startRot, anchor.rotation, t);
            yield return null;
        }

        mainCamera.transform.position = anchor.position;
        mainCamera.transform.rotation = anchor.rotation;
        _currentCameraIndex = anchorIndex;

        onDone?.Invoke();
    }

    // ─── Typewriter ───────────────────────────────────────────

    private IEnumerator TypewriterCoroutine(string text, System.Action onDone)
    {
        if (lineText == null) yield break;

        foreach (char c in text)
        {
            lineText.text += c;
            yield return new WaitForSeconds(1f / charsPerSecond);
        }

        onDone?.Invoke();
    }
}
