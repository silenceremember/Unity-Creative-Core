using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the exploration phase:
///   • seqAmbientStart   — linear narrator dialogue chain
///   • seqTimerTrigger   — segment AFTER which the decorative timer appears
///   • seqClickerTrigger — segment AFTER which the clicker appears
///   • Quest starts when the full ambient chain completes (last segment, nextSequence == null)
///   • Trigger A / B     — interrupt ambient, then resume from the same place (one-shot)
/// </summary>
public class ExplorationManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateChannel gameStateChannel;
    [SerializeField] private NarratorChannel  narratorChannel;

    [Header("Ambient Sequence")]
    [Tooltip("First segment of the ambient chain")]
    [SerializeField] private DialogueSequence seqAmbientStart;

    [Header("Timer Trigger")]
    [Tooltip("Segment AFTER which the decorative timer appears")]
    [SerializeField] private DialogueSequence seqTimerTrigger;

    [Header("Clicker Trigger")]
    [Tooltip("Segment AFTER which the clicker appears")]
    [SerializeField] private DialogueSequence seqClickerTrigger;

    [Header("Trigger Sequences (one-shot)")]
    [SerializeField] private DialogueSequence seqTriggerA;
    [SerializeField] private DialogueSequence seqTriggerB;

    [Header("Decorative Timer (flavor, optional)")]
    [Tooltip("Countdown TMP_Text. Leave empty to skip.")]
    [SerializeField] private TMP_Text timerLabel;
    [Tooltip("Decorative timer duration in seconds")]
    [SerializeField] private float decorativeTimerDuration = 30f;

    [Header("Canvas")]
    [Tooltip("Root Canvas for timer and clicker")]
    [SerializeField] private GameObject explorationCanvas;

    [Header("Clicker")]
    [Tooltip("Click counter TMP_Text. Leave empty to skip.")]
    [SerializeField] private TMP_Text clickerLabel;
    [SerializeField] private ClickerJuice clickerJuice;

    [Header("Dependencies")]
    [SerializeField] private BoolVariable isPausedVariable;
    [SerializeField] private BoolVariable triggerDialoguePlayingVar;

    [Header("Quest Channel")]
    [SerializeField] private VoidChannel questStartChannel;
    [SerializeField] private IntChannel areaTriggerChannel;

    private bool             _explorationActive;
    private bool             _triggerAUsed;
    private bool             _triggerBUsed;

    /// <summary>True only while a trigger A or B dialogue is actively playing.</summary>
    public bool TriggerDialoguePlaying => triggerDialoguePlayingVar != null && triggerDialoguePlayingVar.Value;

    private CancellationTokenSource _timerCts;
    private bool             _clickerActive;


    void Start()
    {
        if (timerLabel != null)   timerLabel.gameObject.SetActive(false);
        if (clickerLabel != null) clickerLabel.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged += OnStateChanged;
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
        if (areaTriggerChannel != null)
            areaTriggerChannel.OnRaised += OnAreaTrigger;
    }

    void OnDisable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged -= OnStateChanged;
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (areaTriggerChannel != null)
            areaTriggerChannel.OnRaised -= OnAreaTrigger;
    }

    void OnDestroy()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
    }

    void Update()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (_clickerActive && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (isPausedVariable != null && isPausedVariable.Value) return;
            if (clickerJuice != null)
                clickerJuice.RegisterClick();
        }
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Gameplay)
            StartExploration();
        else if (state == GameState.Quest)
            _explorationActive = false;
    }

    private void StartExploration()
    {
        if (_explorationActive) return;
        _explorationActive = true;

        if (explorationCanvas != null)
            explorationCanvas.SetActive(true);

        PlayAmbient(seqAmbientStart);
    }

    private void PlayAmbient(DialogueSequence seg)
    {
        if (seg == null) return;
        narratorChannel?.Raise(seg);
    }

    /// <summary>Called from NarratorTrigger (triggerId: 0 = A, 1 = B). One-shot.</summary>
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
        if (triggerDialoguePlayingVar != null)
            triggerDialoguePlayingVar.Value = true;

        narratorChannel?.Raise(triggerSeq);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        bool isTrigger = (completed == seqTriggerA || completed == seqTriggerB);

        if (isTrigger)
        {
            if (triggerDialoguePlayingVar != null)
                triggerDialoguePlayingVar.Value = false;
            return;
        }

        if (!_explorationActive) return;

        if (completed == seqTimerTrigger && timerLabel != null)
        {
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            DecorativeCountdownAsync(_timerCts.Token).SuppressCancellationThrow().Forget();
        }

        if (completed == seqClickerTrigger && clickerLabel != null)
        {
            ShowClicker();
        }

        if (completed.NextSequence == null)
            OnAmbientChainCompleted();
    }

    private void OnAmbientChainCompleted()
    {
        if (!_explorationActive) return;

        _explorationActive = false;

        gameStateChannel?.Raise(GameState.Quest);

        questStartChannel?.Raise();
    }

    private async UniTask DecorativeCountdownAsync(CancellationToken ct)
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

            await UniTask.Delay(System.TimeSpan.FromSeconds(1f), cancellationToken: ct);
            elapsed += 1f;
        }
    }

    private void ShowClicker()
    {
        _clickerActive = true;
        clickerLabel.gameObject.SetActive(true);
        clickerLabel.text = "0";
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>[DEBUG] Instantly shows timer and clicker (F3 in DebugSkip).</summary>
    public void DebugShowTimerAndClicker()
    {
        if (timerLabel != null && !timerLabel.gameObject.activeSelf)
        {
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            DecorativeCountdownAsync(_timerCts.Token).SuppressCancellationThrow().Forget();
        }

        if (clickerLabel != null && !clickerLabel.gameObject.activeSelf)
            ShowClicker();
    }
#endif
}
