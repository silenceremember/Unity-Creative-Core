using UnityEngine;

/// <summary>
/// Configuration for narrator typewriter, erase speed, blip settings, and voice.
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

    [Header("Voice")]
    [Tooltip("Voice blip clips. A random clip is picked per character.")]
    [SerializeField] private AudioClip[] voiceBlips;

    [Tooltip("Min pitch (lower bound). 1.0 = original, 0.89 ≈ −2 semitones.")]
    [SerializeField] private float pitchMin = 0.89f;

    [Tooltip("Max pitch (upper bound). 1.19 ≈ +3 semitones.")]
    [SerializeField] private float pitchMax = 1.19f;

    /// <summary>Returns a random clip from voiceBlips (null if empty).</summary>
    public AudioClip GetRandomBlip()
    {
        if (voiceBlips == null || voiceBlips.Length == 0) return null;
        AudioClip clip = null;
        int attempts = 0;
        while (clip == null && attempts < voiceBlips.Length * 2)
        {
            clip = voiceBlips[UnityEngine.Random.Range(0, voiceBlips.Length)];
            attempts++;
        }
        return clip;
    }

    /// <summary>Returns a random pitch from the [pitchMin, pitchMax] range.</summary>
    public float GetRandomPitch() => UnityEngine.Random.Range(pitchMin, pitchMax);
}
