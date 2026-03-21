using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the exploration phase:
///   • Starts ambient narrator chain on GameState.Gameplay
///   • Transitions to Quest when the chain completes (nextSequence == null)
///   • Timer and clicker are activated via StringChannel / EventActivator
///   • Area triggers (NarratorTrigger) fire their own sequences independently
/// </summary>
public class ExplorationManager : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateChannel gameStateChannel;
    [SerializeField] private NarratorChannel  narratorChannel;

    [Header("Ambient Sequence")]
    [Tooltip("First segment of the ambient chain")]
    [SerializeField] private DialogueSequence seqAmbientStart;

    [Header("Decorative Timer (flavor, optional)")]
    [Tooltip("Countdown TMP_Text. Leave empty to skip.")]
    [SerializeField] private TMP_Text timerLabel;
    [Header("Config")]
    [SerializeField] private ExplorationConfig config;

    [Header("Canvas")]
    [Tooltip("Root Canvas for timer and clicker")]
    [SerializeField] private GameObject explorationCanvas;

    [Header("Clicker")]
    [Tooltip("Click counter TMP_Text. Leave empty to skip.")]
    [SerializeField] private TMP_Text clickerLabel;
    [SerializeField] private ClickerJuice clickerJuice;

    [Header("Painting Shift")]
    [Tooltip("GameObject activated")]
    [SerializeField] private GameObject paintingShiftObject;

    [Header("Activation Channel")]
    [Tooltip("Same StringChannel used by NarratorManager's activateChannel")]
    [SerializeField] private StringChannel activateChannel;

    [Header("Dependencies")]
    [SerializeField] private BoolVariable isPausedVariable;

    [Header("Quest Channel")]
    [SerializeField] private VoidChannel questStartChannel;

    private bool             _explorationActive;
    private CancellationTokenSource _timerCts;
    private bool             _clickerActive;
    private DialogueSequence _lastAmbientSequence;

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
        if (activateChannel != null)
            activateChannel.OnRaised += OnActivateEvent;
    }

    void OnDisable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged -= OnStateChanged;
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (activateChannel != null)
            activateChannel.OnRaised -= OnActivateEvent;
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

        // Walk the chain to find the terminal ambient sequence
        _lastAmbientSequence = seqAmbientStart;
        while (_lastAmbientSequence != null && _lastAmbientSequence.NextSequence != null)
            _lastAmbientSequence = _lastAmbientSequence.NextSequence;

        if (explorationCanvas != null)
            explorationCanvas.SetActive(true);

        if (seqAmbientStart != null)
            narratorChannel?.Raise(seqAmbientStart);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (!_explorationActive) return;

        if (completed == _lastAmbientSequence)
            OnAmbientChainCompleted();
    }

    /// <summary>Handles activation keys from StringChannel (timer, clicker).</summary>
    private void OnActivateEvent(string key)
    {
        if (key == "Timer" && timerLabel != null && !timerLabel.gameObject.activeSelf)
        {
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            DecorativeCountdownAsync(_timerCts.Token).SuppressCancellationThrow().Forget();
        }

        if (key == "Clicker" && clickerLabel != null && !clickerLabel.gameObject.activeSelf)
        {
            ShowClicker();
        }

        if (key == "PaintingShift" && paintingShiftObject != null && !paintingShiftObject.activeSelf)
        {
            paintingShiftObject.SetActive(true);
        }
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
            float display = config.DecorativeTimerDuration - elapsed;
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

    /// <summary>Instantly shows timer and clicker.</summary>
    public void ForceShowTimerAndClicker()
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
}
