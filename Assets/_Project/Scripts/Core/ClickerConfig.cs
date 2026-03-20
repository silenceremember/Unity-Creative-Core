using UnityEngine;

/// <summary>
/// Configuration for clicker juice animations and heat system.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Clicker Config", fileName = "ClickerConfig")]
public class ClickerConfig : ScriptableObject
{
    [Header("Punch (main hit)")]
    [SerializeField] private float punchStrength = 0.45f;
    public float PunchStrength => punchStrength;

    [SerializeField] private float punchDuration = 0.28f;
    public float PunchDuration => punchDuration;

    [SerializeField] private int punchVibrato = 6;
    public int PunchVibrato => punchVibrato;

    [SerializeField] private float punchElasticity = 0.6f;
    public float PunchElasticity => punchElasticity;

    [Header("Mega Punch")]
    [SerializeField] private int megaEvery = 10;
    public int MegaEvery => megaEvery;

    [SerializeField] private float megaStrength = 1.1f;
    public float MegaStrength => megaStrength;

    [SerializeField] private float megaDuration = 0.5f;
    public float MegaDuration => megaDuration;

    [SerializeField] private float megaRotation = 12f;
    public float MegaRotation => megaRotation;

    [Header("Colors (by heat)")]
    [SerializeField] private Color colorCold = new Color(0.7f, 0.9f, 1.0f);
    public Color ColorCold => colorCold;

    [SerializeField] private Color colorWarm = new Color(1.0f, 0.85f, 0.3f);
    public Color ColorWarm => colorWarm;

    [SerializeField] private Color colorHot = new Color(1.0f, 0.35f, 0.1f);
    public Color ColorHot => colorHot;

    [SerializeField] private Color colorMega = new Color(1.0f, 1.0f, 1.0f);
    public Color ColorMega => colorMega;

    [Header("Heat (intensity)")]
    [SerializeField] private float heatPerClick = 0.08f;
    public float HeatPerClick => heatPerClick;

    [SerializeField] private float megaHeatMultiplier = 5f;
    public float MegaHeatMultiplier => megaHeatMultiplier;

    [SerializeField] private float heatDecayRate = 0.18f;
    public float HeatDecayRate => heatDecayRate;

    [Range(0f, 1f)]
    [SerializeField] private float heatWarmThreshold = 0.35f;
    public float HeatWarmThreshold => heatWarmThreshold;

    [Range(0f, 1f)]
    [SerializeField] private float heatHotThreshold = 0.70f;
    public float HeatHotThreshold => heatHotThreshold;

    [Header("Pop Sound")]
    [SerializeField] private float pitchMin = 0.85f;
    public float PitchMin => pitchMin;

    [SerializeField] private float pitchMax = 1.35f;
    public float PitchMax => pitchMax;
}
