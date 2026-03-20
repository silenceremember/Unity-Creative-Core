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
}
