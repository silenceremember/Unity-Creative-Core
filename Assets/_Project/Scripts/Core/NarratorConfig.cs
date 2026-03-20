using UnityEngine;

/// <summary>
/// Configuration for narrator typewriter, erase speed, and blip settings.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Narrator Config", fileName = "NarratorConfig")]
public class NarratorConfig : ScriptableObject
{
    [Tooltip("Typewriter speed (characters per second)")]
    [Range(20, 200)]
    [SerializeField] private float charsPerSecond = 50f;
    public float CharsPerSecond => charsPerSecond;

    [Tooltip("Erase speed (characters per second)")]
    [Range(50, 500)]
    [SerializeField] private float eraseSpeed = 1000f;
    public float EraseSpeed => eraseSpeed;

    [Tooltip("Fade speed multiplier")]
    [SerializeField] private float fadeSpeed = 4f;
    public float FadeSpeed => fadeSpeed;

    [Tooltip("Play blip sound every N non-whitespace characters")]
    [Range(1, 10)]
    [SerializeField] private int blipEveryNChars = 4;
    public int BlipEveryNChars => blipEveryNChars;
}
