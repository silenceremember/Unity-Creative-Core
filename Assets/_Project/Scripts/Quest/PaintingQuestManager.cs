using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Main manager for the "fix paintings" quest.
///
/// Logic:
///   - Slot order comes from QuestConfig.SlotOrder
///   - Code = string of (slotIndex + 1) values in press order
///   - Correct code = QuestConfig.CorrectCode
///   - Accept = green; Reject = red + shake
/// </summary>
public class PaintingQuestManager : MonoBehaviour
{
    [Header("Paintings (all 4 interactables)")]
    [SerializeField] private PaintingInteractable[] interactables = new PaintingInteractable[4];

    [Header("Quest Canvas")]
    [SerializeField] private GameObject questCanvas;

    [Header("Quest Lines (Picture1..4 in Group)")]
    [SerializeField] private TextMeshProUGUI[] pictureLabels = new TextMeshProUGUI[4];

    [Header("E-Prompt")]
    [Tooltip("UI object with '[E] Fix' hint")]
    [SerializeField] private GameObject ePrompt;

    [Header("Config")]
    [SerializeField] private QuestConfig config;

    [Header("Audio")]
    [SerializeField] private AudioSource questAudioSource;

    [Header("Dependencies")]
    [SerializeField] private IntChannel addXPChannel;
    [SerializeField] private BoolVariable isPausedVariable;
    [SerializeField] private BoolVariable narratorPlayingVar;

    [Header("Channels")]
    [SerializeField] private VoidChannel questStartChannel;

    [Header("Narrator")]
    [SerializeField] private NarratorChannel narratorChannel;

    [Tooltip("Dialogues on painting click (20)")]
    [SerializeField] private DialogueSequence[] paintingClickDialogues = new DialogueSequence[20];

    [Tooltip("Dialogues on reject (5)")]
    [SerializeField] private DialogueSequence[] rejectDialogues = new DialogueSequence[5];

    [Tooltip("Dialogue after successful quest completion")]
    [SerializeField] private DialogueSequence seqPostQuest;

    private string         _enteredCode   = "";
    private int            _pressCount    = 0;
    private bool           _questActive   = false;
    private bool           _resolved      = false;
    private int            _rejectCount   = 0;
    private bool           _pendingXPReward = false;
    private int            _clickDialogueIndex = 0;
    private bool           _selfNarrating      = false;

    private PaintingInteractable _nearPainting;

    private float _eBlockedUntil  = 0f;

    [Header("Reject E-Prompt Animation")]
    private bool _ePromptShaking = false;

    private CancellationTokenSource _questCts;


    void Start()
    {
        if (questCanvas  != null) questCanvas .SetActive(false);
        if (ePrompt      != null) ePrompt     .SetActive(false);
    }

    void OnEnable()
    {
        if (narratorChannel != null)
        {
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
            narratorChannel.OnSequenceRequested += OnAnySequenceStarted;
        }
        if (questStartChannel != null)
            questStartChannel.OnRaised += StartQuest;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
        {
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
            narratorChannel.OnSequenceRequested -= OnAnySequenceStarted;
        }
        if (questStartChannel != null)
            questStartChannel.OnRaised -= StartQuest;
    }

    void OnDestroy()
    {
        _questCts?.Cancel();
        _questCts?.Dispose();
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (_pendingXPReward && completed == seqPostQuest)
        {
            _pendingXPReward = false;
            addXPChannel?.Raise(config.QuestRewardXP);
        }
    }

    /// <summary>Tracks whether the currently playing sequence belongs to this quest.</summary>
    private void OnAnySequenceStarted(DialogueSequence seq)
    {
        _selfNarrating = IsOwnDialogue(seq);
    }

    private bool IsOwnDialogue(DialogueSequence seq)
    {
        if (seq == seqPostQuest) return true;
        if (paintingClickDialogues != null)
            foreach (var d in paintingClickDialogues)
                if (d == seq) return true;
        if (rejectDialogues != null)
            foreach (var d in rejectDialogues)
                if (d == seq) return true;
        return false;
    }

    /// <summary>
    /// Returns the slot index for the given press count from config.
    /// </summary>
    private int GetSlotForPress(int pressIndex)
    {
        var order = config.SlotOrder;
        return pressIndex < order.Length ? order[pressIndex] : pressIndex;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>[DEBUG] Instantly completes the quest (O key).</summary>
    public void DebugCompleteQuest()
    {
        if (!_questActive) StartQuest();

        _questCts?.Cancel();
        _questCts?.Dispose();
        _questCts = null;

        for (int i = 0; i < interactables.Length && i < 4; i++)
        {
            var p = interactables[i];
            if (p == null || p.IsUsed) continue;
            if (p.AssignedSlotIndex == -1)
                p.AssignedSlotIndex = GetSlotForPress(_pressCount);
            p.SnapToCorrect();
            _enteredCode += (p.AssignedSlotIndex + 1).ToString();

            int slotIndex = p.AssignedSlotIndex;
            if (slotIndex < pictureLabels.Length && pictureLabels[slotIndex] != null)
            {
                pictureLabels[slotIndex].color     = config.ColorDone;
                pictureLabels[slotIndex].fontStyle |= FontStyles.Strikethrough;
            }
            _pressCount++;
        }

        _enteredCode  = config.CorrectCode;
        _resolved     = true;
        _questActive  = false;

        _questCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        ResolveAsync(_questCts.Token).Forget();
    }
#endif

    void Update()
    {
        if (!_questActive || _resolved) return;
        if (_nearPainting == null)      return;

        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.eKey.wasPressedThisFrame)
        {
            if (isPausedVariable != null && isPausedVariable.Value) return;
            bool externalNarrator = narratorPlayingVar != null &&
                                    narratorPlayingVar.Value &&
                                    !_selfNarrating;
            if (externalNarrator)
            {
                if (!_ePromptShaking && ePrompt != null)
                {
                    if (questAudioSource != null && config.RejectSound != null)
                        questAudioSource.PlayOneShot(config.RejectSound);
                    ShakeEPromptAsync(destroyCancellationToken).Forget();
                }
                return;
            }
            _eBlockedUntil = Time.unscaledTime + config.ESpamCooldown;
            InteractPainting(_nearPainting);
        }
    }

    /// <summary>Called via VoidChannel — starts the quest.</summary>
    public void StartQuest()
    {
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.CaptureQuestStart();

        _questActive  = true;
        _resolved     = false;
        _enteredCode  = "";
        _pressCount   = 0;
        _rejectCount  = 0;

        if (questCanvas != null) questCanvas.SetActive(true);
    }

    /// <summary>PaintingInteractable calls this when the player enters the zone.</summary>
    public void OnPaintingEnter(PaintingInteractable painting)
    {
        if (!_questActive || _resolved || painting.IsUsed) return;
        _nearPainting = painting;
        if (ePrompt != null) ePrompt.SetActive(true);
    }

    /// <summary>PaintingInteractable calls this when the player exits the zone.</summary>
    public void OnPaintingExit(PaintingInteractable painting)
    {
        if (_nearPainting != painting) return;
        _nearPainting = null;
        if (ePrompt != null) ePrompt.SetActive(false);
    }

    private void InteractPainting(PaintingInteractable painting)
    {
        if (painting.IsUsed) return;

        if (questAudioSource != null && config.InteractSound != null)
            questAudioSource.PlayOneShot(config.InteractSound);

        painting.SnapToCorrect();

        if (painting.AssignedSlotIndex == -1)
            painting.AssignedSlotIndex = GetSlotForPress(_pressCount);

        _enteredCode += (painting.AssignedSlotIndex + 1).ToString();

        int slotIndex = painting.AssignedSlotIndex;
        if (slotIndex < pictureLabels.Length && pictureLabels[slotIndex] != null)
        {
            pictureLabels[slotIndex].color = config.ColorDone;
            pictureLabels[slotIndex].fontStyle |= FontStyles.Strikethrough;
        }

        _pressCount++;

        if (paintingClickDialogues != null && paintingClickDialogues.Length > 0)
        {
            int idx = _clickDialogueIndex % paintingClickDialogues.Length;
            _clickDialogueIndex++;
            var seq = paintingClickDialogues[idx];
            if (seq != null) narratorChannel?.Raise(seq);
        }

        if (ePrompt != null) ePrompt.SetActive(false);
        _nearPainting = null;

        if (_pressCount >= config.SlotOrder.Length)
        {
            _questCts?.Cancel();
            _questCts?.Dispose();
            _questCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            ResolveAsync(_questCts.Token).SuppressCancellationThrow().Forget();
        }
    }

    private async UniTask ResolveAsync(CancellationToken ct)
    {
        _resolved    = true;
        _questActive = false;

        await UniTask.Delay(System.TimeSpan.FromSeconds(config.ResolveDelay), cancellationToken: ct);

        while (narratorPlayingVar != null && narratorPlayingVar.Value)
            await UniTask.Yield(ct);

        bool accepted = _enteredCode == config.CorrectCode;

        Color resultColor = accepted ? config.ColorAccept : config.ColorReject;

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color = resultColor;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        if (accepted)
        {
            if (questAudioSource != null && config.AcceptSound != null)
                questAudioSource.PlayOneShot(config.AcceptSound);

            await PulseLabelsAsync(ct);

            if (seqPostQuest != null)
            {
                _pendingXPReward = true;
                narratorChannel?.Raise(seqPostQuest);
            }
            else
            {
                addXPChannel?.Raise(config.QuestRewardXP);
            }
        }
        else
        {
            if (questAudioSource != null && config.RejectSound != null)
                questAudioSource.PlayOneShot(config.RejectSound);

            await ShakeLabelsAsync(ct);

            _rejectCount++;

            int rejectIdx = _rejectCount - 1;
            if (rejectDialogues != null && rejectIdx < rejectDialogues.Length && rejectDialogues[rejectIdx] != null)
                narratorChannel?.Raise(rejectDialogues[rejectIdx]);

            if (_rejectCount >= 5)
            {
                await AutoSolveAfterDialogueAsync(ct);
            }
            else
            {
                await UniTask.Delay(System.TimeSpan.FromSeconds(config.ResetDelay), cancellationToken: ct);
                DoReset();
            }
        }
    }

    private async UniTask AutoSolveAfterDialogueAsync(CancellationToken ct)
    {
        // Wait for the reject dialogue to finish
        while (narratorPlayingVar != null && narratorPlayingVar.Value)
            await UniTask.Yield(ct);

        await UniTask.Delay(System.TimeSpan.FromSeconds(config.AutoSolveDelay), cancellationToken: ct);

        _resolved    = false;
        _questActive = true;
        _enteredCode = "";
        _pressCount  = 0;

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color     = config.ColorDefault;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }

        DebugCompleteQuest();
    }

    private void DoReset()
    {
        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.ResetPainting(config.RejectResetDuration);

        _enteredCode  = "";
        _pressCount   = 0;
        _resolved     = false;
        _questActive  = true;

        foreach (var lbl in pictureLabels)
            if (lbl != null)
            {
                lbl.color     = config.ColorDefault;
                lbl.fontStyle &= ~FontStyles.Strikethrough;
            }
    }

    private async UniTask ShakeLabelsAsync(CancellationToken ct)
    {
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) return;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        await UIAnimationHelper.ShakeAsync(groupRT, config.ShakeDuration, config.ShakeMagnitude, ct);
    }

    private async UniTask PulseLabelsAsync(CancellationToken ct)
    {
        if (pictureLabels.Length == 0 || pictureLabels[0] == null) return;
        var groupRT = pictureLabels[0].transform.parent as RectTransform;
        await UIAnimationHelper.PulseAsync(groupRT, config.PulseDuration, config.PulseAmount, ct);
    }

    private async UniTask ShakeEPromptAsync(CancellationToken ct)
    {
        if (ePrompt == null) return;
        _ePromptShaking = true;

        var rt = ePrompt.GetComponent<RectTransform>();
        if (rt == null) { _ePromptShaking = false; return; }

        var txt = ePrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Color originalColor = txt != null ? txt.color : Color.white;
        if (txt != null) txt.color = Color.red;

        await UIAnimationHelper.ShakeAsync(rt, config.EPromptShakeDuration, config.EPromptShakeMagnitude, ct);

        if (txt != null) txt.color = originalColor;
        _ePromptShaking = false;
    }
}
