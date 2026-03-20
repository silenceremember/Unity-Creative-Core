using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// Main manager for the "fix paintings" quest.
///
/// Logic:
///   - Fixed slot order: [2, 0, 3, 1] (0-based Picture indices)
///     Means: 1st press → Picture3, 2nd → Picture1, 3rd → Picture4, 4th → Picture2
///   - Each PaintingInteractable has a codeDigit (1-4)
///   - Code = string of codeDigit values in press order
///   - Correct code = "1234"
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

    [Header("Quest Sounds")]
    [Tooltip("Sound on E press (not reject)")]
    [SerializeField] private AudioClip interactSound;
    [Tooltip("Sound on quest success (accept / green)")]
    [SerializeField] private AudioClip acceptSound;
    [Tooltip("Sound on quest failure (reject / red)")]
    [SerializeField] private AudioClip rejectSound;
    [SerializeField] private AudioSource questAudioSource;

    [Header("Dependencies")]
    [SerializeField] private IntChannel addXPChannel;
    [SerializeField] private BoolVariable isPausedVariable;
    [SerializeField] private BoolVariable triggerDialoguePlayingVar;

    [Header("Channels")]
    [SerializeField] private VoidChannel questStartChannel;

    [Header("Narrator")]
    [SerializeField] private NarratorChannel narratorChannel;

    [Tooltip("Dialogues on painting click (4 — cycled)")]
    [SerializeField] private DialogueSequence[] paintingClickDialogues = new DialogueSequence[4];

    [Tooltip("Dialogues on reject (5: reject 1..5)")]
    [SerializeField] private DialogueSequence[] rejectDialogues = new DialogueSequence[5];

    [Tooltip("Dialogue after successful quest completion")]
    [SerializeField] private DialogueSequence seqPostQuest;

    private static readonly int[] SlotOrder = { 2, 0, 3, 1 };

    private string         _enteredCode   = "";
    private int            _pressCount    = 0;
    private bool           _questActive   = false;
    private bool           _resolved      = false;
    private int            _rejectCount   = 0;

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
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
        if (questStartChannel != null)
            questStartChannel.OnRaised += StartQuest;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
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
        if (completed == seqPostQuest)
        {
            addXPChannel?.Raise(config.QuestRewardXP);
        }
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
                p.AssignedSlotIndex = SlotOrder[_pressCount];
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
            bool triggerDialogue = triggerDialoguePlayingVar != null &&
                                   triggerDialoguePlayingVar.Value;
            if (triggerDialogue)
            {
                if (!_ePromptShaking && ePrompt != null)
                {
                    if (questAudioSource != null && rejectSound != null)
                        questAudioSource.PlayOneShot(rejectSound);
                    ShakeEPromptAsync(destroyCancellationToken).Forget();
                }
                return;
            }
            _eBlockedUntil = Time.unscaledTime + config.ESpamCooldown;
            InteractPainting(_nearPainting);
        }
    }

    /// <summary>Called from ExplorationManager — starts the quest.</summary>
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

        if (questAudioSource != null && interactSound != null)
            questAudioSource.PlayOneShot(interactSound);

        painting.SnapToCorrect();

        if (painting.AssignedSlotIndex == -1)
            painting.AssignedSlotIndex = SlotOrder[_pressCount];

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
            int idx = (_pressCount - 1) % paintingClickDialogues.Length;
            var seq = paintingClickDialogues[idx];
            if (seq != null) narratorChannel?.Raise(seq);
        }

        if (ePrompt != null) ePrompt.SetActive(false);
        _nearPainting = null;

        if (_pressCount >= 4)
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

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.3f), cancellationToken: ct);

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
            if (questAudioSource != null && acceptSound != null)
                questAudioSource.PlayOneShot(acceptSound);

            await PulseLabelsAsync(ct);

            if (seqPostQuest != null)
                narratorChannel?.Raise(seqPostQuest);
            else
                addXPChannel?.Raise(config.QuestRewardXP);
        }
        else
        {
            if (questAudioSource != null && rejectSound != null)
                questAudioSource.PlayOneShot(rejectSound);

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
                await UniTask.Delay(System.TimeSpan.FromSeconds(0.8f), cancellationToken: ct);
                DoReset();
            }
        }
    }

    private async UniTask AutoSolveAfterDialogueAsync(CancellationToken ct)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(5f), cancellationToken: ct);

        foreach (var interactable in interactables)
            if (interactable != null)
                interactable.ResetPainting(0.4f);

        await UniTask.Delay(System.TimeSpan.FromSeconds(0.6f), cancellationToken: ct);

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
                interactable.ResetPainting(0.5f);

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
        await UIAnimationHelper.PulseAsync(groupRT, config.PulseDuration, 0.12f, ct);
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
