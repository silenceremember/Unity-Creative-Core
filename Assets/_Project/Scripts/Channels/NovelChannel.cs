using System;
using UnityEngine;

/// <summary>
/// SO event bus for the visual novel.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Novel Channel", fileName = "NovelChannel")]
public class NovelChannel : ScriptableObject
{
    [Header("Character Voices")]
    [Tooltip("Mary's voice blips")]
    public AudioClip[] maryBlips = new AudioClip[6];

    [Tooltip("Min pitch for Mary. 1.0 = original.")]
    public float maryPitchMin = 0.94f;

    [Tooltip("Max pitch for Mary.")]
    public float maryPitchMax = 1.19f;

    [Tooltip("Blip every N non-whitespace chars for Mary (Undertale-style).")]
    [Range(1, 10)]
    public int maryBlipInterval = 3;

    [Tooltip("John's voice blips")]
    public AudioClip[] johnBlips = new AudioClip[6];

    [Tooltip("Min pitch for John. 1.0 = original.")]
    public float johnPitchMin = 0.84f;

    [Tooltip("Max pitch for John.")]
    public float johnPitchMax = 1.05f;

    [Tooltip("Blip every N non-whitespace chars for John (Undertale-style).")]
    [Range(1, 10)]
    public int johnBlipInterval = 4;

    /// <summary>Returns a random blip for the given speaker.</summary>
    public AudioClip GetBlip(string speaker)
    {
        AudioClip[] arr = ResolveSpeaker(speaker);
        if (arr == null || arr.Length == 0) return null;
        int attempts = 0;
        AudioClip clip = null;
        while (clip == null && attempts < arr.Length * 2)
        {
            clip = arr[UnityEngine.Random.Range(0, arr.Length)];
            attempts++;
        }
        return clip;
    }

    /// <summary>Returns a random pitch for the given speaker.</summary>
    public float GetRandomPitch(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key)) return UnityEngine.Random.Range(maryPitchMin, maryPitchMax);
        if (IsJohn(key)) return UnityEngine.Random.Range(johnPitchMin, johnPitchMax);
        return 1f;
    }

    /// <summary>Returns blip interval (every N chars) for the speaker. Fallback = 4.</summary>
    public int GetBlipInterval(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key)) return maryBlipInterval;
        if (IsJohn(key)) return johnBlipInterval;
        return 4;
    }

    private bool IsMary(string key) =>
        string.Equals(key, "Mary",    StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Мэри",   StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Wife",    StringComparison.OrdinalIgnoreCase);

    private bool IsJohn(string key) =>
        string.Equals(key, "John",    StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Джон",   StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Husband", StringComparison.OrdinalIgnoreCase);

    private AudioClip[] ResolveSpeaker(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key))    return maryBlips;
        if (IsJohn(key))    return johnBlips;
        return null;
    }

    /// <summary>Call to start the novel.</summary>
    public event Action OnNovelStartRequested;
    public void RaiseStart() => OnNovelStartRequested?.Invoke();

    /// <summary>Fires when the novel is fully completed.</summary>
    public event Action OnNovelCompleted;
    public void NotifyCompleted() => OnNovelCompleted?.Invoke();

    /// <summary>Call to force-abort the novel (e.g. from pause menu).</summary>
    public event Action OnNovelAbortRequested;
    public void RaiseAbort() => OnNovelAbortRequested?.Invoke();
}
