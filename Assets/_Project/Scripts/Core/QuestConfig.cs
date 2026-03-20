using UnityEngine;

/// <summary>
/// Configuration for painting quest, XP leveling, and interactable timing.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Quest Config", fileName = "QuestConfig")]
public class QuestConfig : ScriptableObject
{
    [Header("Quest Labels Shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    public float ShakeDuration => shakeDuration;

    [SerializeField] private float shakeMagnitude = 12f;
    public float ShakeMagnitude => shakeMagnitude;

    [SerializeField] private float pulseDuration = 0.4f;
    public float PulseDuration => pulseDuration;

    [Header("E-Prompt Shake")]
    [SerializeField] private float ePromptShakeMagnitude = 10f;
    public float EPromptShakeMagnitude => ePromptShakeMagnitude;

    [SerializeField] private float ePromptShakeDuration = 0.35f;
    public float EPromptShakeDuration => ePromptShakeDuration;

    [SerializeField] private float eSpamCooldown = 0.5f;
    public float ESpamCooldown => eSpamCooldown;

    [Header("Painting Snap")]
    [SerializeField] private float snapDuration = 0.5f;
    public float SnapDuration => snapDuration;

    [Header("XP Bar")]
    [SerializeField] private float fillDuration = 4f;
    public float FillDuration => fillDuration;

    [SerializeField] private int questRewardXP = 1000;
    public int QuestRewardXP => questRewardXP;

    [Header("Level Up Animation")]
    [SerializeField] private float levelLabelPunch = 1.35f;
    public float LevelLabelPunch => levelLabelPunch;

    [SerializeField] private float levelPunchDuration = 0.35f;
    public float LevelPunchDuration => levelPunchDuration;

    [Header("XP Tick Sound")]
    [SerializeField] private float tickInterval = 0.08f;
    public float TickInterval => tickInterval;

    [SerializeField] private float tickPitchMin = 0.8f;
    public float TickPitchMin => tickPitchMin;

    [SerializeField] private float tickPitchMax = 1.4f;
    public float TickPitchMax => tickPitchMax;

    [Header("X-Prompt Shake")]
    [SerializeField] private float promptShakeMagnitude = 10f;
    public float PromptShakeMagnitude => promptShakeMagnitude;

    [SerializeField] private float promptShakeDuration = 0.35f;
    public float PromptShakeDuration => promptShakeDuration;

    [SerializeField] private float inputSpamCooldown = 0.5f;
    public float InputSpamCooldown => inputSpamCooldown;

    [Header("Quest Colors")]
    [SerializeField] private Color colorDefault = Color.yellow;
    public Color ColorDefault => colorDefault;

    [SerializeField] private Color colorDone = Color.gray;
    public Color ColorDone => colorDone;

    [SerializeField] private Color colorAccept = Color.green;
    public Color ColorAccept => colorAccept;

    [SerializeField] private Color colorReject = Color.red;
    public Color ColorReject => colorReject;

    [Header("Quest Code")]
    [Tooltip("Correct code sequence")]
    [SerializeField] private string correctCode = "1234";
    public string CorrectCode => correctCode;

    [Header("Painting Shift")]
    [Tooltip("Painting shift transition duration")]
    [SerializeField] private float shiftDuration = 0.6f;
    public float ShiftDuration => shiftDuration;

    [Tooltip("Delay between individual painting shifts")]
    [SerializeField] private float shiftStagger = 0f;
    public float ShiftStagger => shiftStagger;

    [Header("XP Level Data")]
    [Tooltip("XP required for each level. [0] = Lv.0→1, etc.")]
    [SerializeField] private int[] xpRequirements = { 500, 750, 1000 };
    public int[] XPRequirements => xpRequirements;

    [Tooltip("XP fill animation curve")]
    [SerializeField] private AnimationCurve xpFillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve XPFillCurve => xpFillCurve;

    [Tooltip("Flash color on level up")]
    [SerializeField] private Color flashColor = new Color(1f, 0.85f, 0f, 1f);
    public Color FlashColor => flashColor;

    [Tooltip("Reward label format string ({0} = XP amount)")]
    [SerializeField] private string rewardFormat = "REWARD: {0} XP";
    public string RewardFormat => rewardFormat;
}
