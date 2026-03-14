using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Управляет фазой исследования:
///   • seqAmbientStart   — линейная цепочка реплик рассказчика
///   • seqTimerTrigger   — сегмент, ПОСЛЕ которого появляется декоративный таймер (Seq_Ambient_19)
///   • seqClickerTrigger — сегмент, ПОСЛЕ которого появляется кликер (Seq_Ambient_22)
///   • seqClickerEnd     — сегмент, ПОСЛЕ которого кликер скрывается (Seq_Ambient_38)
///   • Квест стартует по завершению всей ambient-цепочки (последний сегмент, nextSequence == null)
///   • Триггер A / B     — прерывают ambient, после возобновляют с того же места (одноразовые)
/// </summary>
public class ExplorationManager : MonoBehaviour
{
    public static ExplorationManager Instance { get; private set; }

    [Header("Channels")]
    public GameStateChannel gameStateChannel;
    public NarratorChannel  narratorChannel;

    [Header("Ambient Sequence")]
    [Tooltip("Первый сегмент ambient-цепочки (Seq_Ambient_00)")]
    public DialogueSequence seqAmbientStart;

    [Header("Timer Trigger")]
    [Tooltip("Сегмент, ПОСЛЕ которого появится декоративный таймер (Seq_Ambient_19)")]
    public DialogueSequence seqTimerTrigger;

    [Header("Clicker Trigger")]
    [Tooltip("Сегмент, ПОСЛЕ которого появится кликер (Seq_Ambient_22)")]
    public DialogueSequence seqClickerTrigger;

    [Header("Trigger Sequences (одноразовые)")]
    public DialogueSequence seqTriggerA;
    public DialogueSequence seqTriggerB;

    [Header("Decorative Timer (flavor, optional)")]
    [Tooltip("TMP_Text обратного отсчёта. Оставьте пустым — не покажется.")]
    public TMP_Text timerLabel;
    [Tooltip("Длительность декоративного таймера в секундах (по умолчанию 30 сек)")]
    public float decorativeTimerDuration = 30f;

    [Header("Canvas")]
    [Tooltip("Корневой Canvas таймера и кликера — включается при старте Gameplay")]
    public GameObject explorationCanvas;

    [Header("Clicker")]
    [Tooltip("TMP_Text счётчика кликов. Оставьте пустым — кликер не появится.")]
    public TMP_Text clickerLabel;
    [Tooltip("Компонент сочных эффектов кликера — назначается автоматически")]
    public ClickerJuice clickerJuice;

    // ── State ────────────────────────────────────────────────────────
    private bool             _explorationActive;
    private bool             _triggerAUsed;
    private bool             _triggerBUsed;

    /// <summary>True только пока прямо сейчас играет диалог триггера A или B.</summary>
    public bool TriggerDialoguePlaying => _triggerDialoguePlaying;
    private bool _triggerDialoguePlaying;

    // Ambient: tracking for OnAmbientChainCompleted (no longer used for restoration)
    // Decorative timer
    private Coroutine        _decorativeCo;

    // Clicker
    private bool             _clickerActive;

    // ── Lifecycle ────────────────────────────────────────────────────

    void Awake() => Instance = this;

    void Start()
    {
        // Убеждаемся что таймер и кликер скрыты в начале
        if (timerLabel != null)   timerLabel.gameObject.SetActive(false);
        if (clickerLabel != null) clickerLabel.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged += OnStateChanged;
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
    }

    void OnDisable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged -= OnStateChanged;
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
    }

    void Update()
    {
        // Обрабатываем клик по кликеру (только когда кликер активен)
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (_clickerActive && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (PauseMenuManager.IsPaused) return;
            if (clickerJuice != null)
                clickerJuice.RegisterClick();
        }
    }

    // ── GameState ────────────────────────────────────────────────────

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Gameplay)
            StartExploration();
        else if (state == GameState.Quest)
            _explorationActive = false;  // Квест начался — больше не перехватываем завершения нарратора
    }

    // ── Exploration Start ────────────────────────────────────────────

    private void StartExploration()
    {
        if (_explorationActive) return;
        _explorationActive = true;

        Debug.Log("[ExplorationManager] Starting exploration phase.");

        // Показываем корневой Canvas (таймер + кликер)
        if (explorationCanvas != null)
            explorationCanvas.SetActive(true);

        // Запускаем ambient с первого сегмента
        PlayAmbient(seqAmbientStart);
    }

    // ── Ambient helpers ──────────────────────────────────────────────

    /// <summary>Запускает указанный ambient-сегмент и запоминает его.</summary>
    private void PlayAmbient(DialogueSequence seg)
    {
        if (seg == null) return;
        narratorChannel?.Raise(seg);
    }

    // ── Area Triggers (одноразовые) ──────────────────────────────────

    /// <summary>Вызывается из NarratorTrigger (triggerId: 0 = A, 1 = B).
    /// Срабатывает в любом GameState (всегда активен), но одноразово.
    /// </summary>
    public void OnAreaTrigger(int triggerId)
    {
        if (triggerId == 0 && !_triggerAUsed && seqTriggerA != null)
        {
            _triggerAUsed = true;
            PlayTrigger(seqTriggerA);
        }
        else if (triggerId == 1 && !_triggerBUsed && seqTriggerB != null)
        {
            _triggerBUsed = true;
            PlayTrigger(seqTriggerB);
        }
    }

    private void PlayTrigger(DialogueSequence triggerSeq)
    {
        // Проверяем примет ли нарратор триггер (для корректного выставления флага).
        // Если triggerSeq.restoreInterrupted = true, NarratorManager сам сохранит
        // прерванный диалог и восстановит его после завершения триггера.
        int currentPriority = NarratorManager.Instance?.CurrentSequence?.priority ?? 0;
        bool willPlay = !NarratorManager.Instance.IsPlaying || triggerSeq.priority >= currentPriority;
        if (willPlay)
            _triggerDialoguePlaying = true;

        narratorChannel?.Raise(triggerSeq);
    }


    // ── Narrator Completed ───────────────────────────────────────────

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        bool isTrigger = (completed == seqTriggerA || completed == seqTriggerB);

        if (isTrigger)
        {
            // Восстановление прерванного диалога делает NarratorManager автоматически
            // (если triggerSeq.restoreInterrupted = true в ассете).
            _triggerDialoguePlaying = false;
            return;
        }

        if (!_explorationActive) return;

        // Специальный сегмент: таймер появляется после его завершения
        if (completed == seqTimerTrigger && timerLabel != null)
        {
            if (_decorativeCo != null) StopCoroutine(_decorativeCo);
            _decorativeCo = StartCoroutine(DecorativeCountdown());
        }

        // Специальный сегмент: кликер появляется после его завершения
        if (completed == seqClickerTrigger && clickerLabel != null)
        {
            ShowClicker();
        }

        // Ambient-сегмент завершился.
        // NarratorManager сам запускает следующий через nextSequence.
        // Обновляем логику таймера/кликера и проверяем конец цепочки.
        if (completed.nextSequence == null)
            OnAmbientChainCompleted();
    }

    // ── Ambient Chain End → Quest ────────────────────────────────────

    private void OnAmbientChainCompleted()
    {
        // Защита от двойного вызова: если exploration уже остановлена — ничего не делаем.
        // Может случиться если триггер прервал последний ambient, а restoration его повторила.
        if (!_explorationActive) return;

        Debug.Log("[ExplorationManager] Ambient dialogue finished — starting quest.");
        _explorationActive = false;

        // Переключаем состояние игры → Quest
        gameStateChannel?.Raise(GameState.Quest);

        if (PaintingQuestManager.Instance != null)
            PaintingQuestManager.Instance.StartQuest();
    }

    // ── Decorative Countdown ─────────────────────────────────────────

    private IEnumerator DecorativeCountdown()
    {
        timerLabel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (true)
        {
            float display = decorativeTimerDuration - elapsed;
            bool negative = display < 0f;
            float abs = Mathf.Abs(display);
            int minutes = Mathf.FloorToInt(abs / 60f);
            int seconds = Mathf.FloorToInt(abs % 60f);
            timerLabel.text = negative
                ? $"-{minutes:0}:{seconds:00}"
                : $"{minutes:0}:{seconds:00}";

            yield return new WaitForSeconds(1f);
            elapsed += 1f;
        }
    }

    // ── Clicker ───────────────────────────────────────────────────────

    private void ShowClicker()
    {
        _clickerActive = true;
        clickerLabel.gameObject.SetActive(true);
        // Сбрасываем текст — ClickerJuice сам будет рисовать число
        clickerLabel.text = "0";
        Debug.Log("[ExplorationManager] Clicker shown.");
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>[DEBUG] Сразу показывает таймер и кликер (F3 в DebugSkip).</summary>
    public void DebugShowTimerAndClicker()
    {
        if (timerLabel != null && !timerLabel.gameObject.activeSelf)
        {
            if (_decorativeCo != null) StopCoroutine(_decorativeCo);
            _decorativeCo = StartCoroutine(DecorativeCountdown());
        }

        if (clickerLabel != null && !clickerLabel.gameObject.activeSelf)
            ShowClicker();
    }
#endif
}
