using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Управляет фазой исследования:
///   • seqAmbientStart   — линейная цепочка реплик рассказчика
///   • seqTimerTrigger   — конкретный сегмент, после которого появляется декоративный таймер
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

    [Header("Trigger Sequences (одноразовые)")]
    public DialogueSequence seqTriggerA;
    public DialogueSequence seqTriggerB;

    [Header("Decorative Timer (flavor, optional)")]
    [Tooltip("TMP_Text обратного отсчёта. Оставьте пустым — не покажется.")]
    public TMP_Text timerLabel;
    [Tooltip("Длительность декоративного таймера в секундах (по умолчанию 30 сек)")]
    public float decorativeTimerDuration = 30f;

    // ── State ────────────────────────────────────────────────────────
    private bool             _explorationActive;
    private bool             _triggerAUsed;
    private bool             _triggerBUsed;

    // Ambient: последний «живой» сегмент (сохраняем при прерывании триггером)
    private DialogueSequence _currentAmbientSeg;

    // Decorative timer
    private Coroutine        _decorativeCo;

    // ── Lifecycle ────────────────────────────────────────────────────

    void Awake() => Instance = this;

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

    // ── GameState ────────────────────────────────────────────────────

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Gameplay)
            StartExploration();
    }

    // ── Exploration Start ────────────────────────────────────────────

    private void StartExploration()
    {
        if (_explorationActive) return;
        _explorationActive = true;

        Debug.Log("[ExplorationManager] Starting exploration phase.");

        // Запускаем ambient с первого сегмента
        PlayAmbient(seqAmbientStart);
    }

    // ── Ambient helpers ──────────────────────────────────────────────

    /// <summary>Запускает указанный ambient-сегмент и запоминает его.</summary>
    private void PlayAmbient(DialogueSequence seg)
    {
        if (seg == null) return;
        _currentAmbientSeg = seg;
        narratorChannel?.Raise(seg);
    }

    // ── Area Triggers (одноразовые) ──────────────────────────────────

    /// <summary>Вызывается из NarratorTrigger (triggerId: 0 = A, 1 = B).</summary>
    public void OnAreaTrigger(int triggerId)
    {
        if (!_explorationActive) return;

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
        // Прервать ambient (но _currentAmbientSeg уже сохранён)
        narratorChannel?.Stop();

        // Запустить триггер
        narratorChannel?.Raise(triggerSeq);
    }

    // ── Narrator Completed ───────────────────────────────────────────

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (!_explorationActive) return;

        bool isTrigger = (completed == seqTriggerA || completed == seqTriggerB);

        if (isTrigger)
        {
            // После триггера — возобновить ambient с сохранённого сегмента
            PlayAmbient(_currentAmbientSeg);
        }
        else
        {
            // Специальный сегмент: таймер появляется после его завершения
            if (completed == seqTimerTrigger && timerLabel != null)
            {
                if (_decorativeCo != null) StopCoroutine(_decorativeCo);
                _decorativeCo = StartCoroutine(DecorativeCountdown());
            }

            // Ambient-сегмент завершился.
            // NarratorManager сам запускает следующий через nextSequence.
            // Если nextSequence == null — цепочка кончилась → стартуем квест.
            if (completed.nextSequence == null)
            {
                OnAmbientChainCompleted();
            }
        }
    }

    // ── Ambient Chain End → Quest ────────────────────────────────────

    private void OnAmbientChainCompleted()
    {
        Debug.Log("[ExplorationManager] Ambient dialogue finished — starting quest.");
        _explorationActive = false;

        if (PaintingQuestManager.Instance != null)
            PaintingQuestManager.Instance.StartQuest();
    }

    // ── Decorative Countdown ─────────────────────────────────────────

    private IEnumerator DecorativeCountdown()
    {
        timerLabel.gameObject.SetActive(true);
        float remaining = decorativeTimerDuration;

        while (remaining > 0f)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            timerLabel.text = $"{minutes:0}:{seconds:00}";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        timerLabel.text = "0:00";
        // Таймер иссяк — рассказчик к этому моменту уже всё сказал
    }
}
