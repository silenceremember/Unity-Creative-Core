using System;
using UnityEngine;

/// <summary>
/// Narrator event channel. ScriptableObject event bus.
/// NarratorManager subscribes; buttons/triggers call Raise().
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Narrator Channel", fileName = "NarratorChannel")]
public class NarratorChannel : ScriptableObject
{
    [Header("Character Voice")]
    [Tooltip("Voice blip clips. A random clip is picked per character.")]
    public AudioClip[] voiceBlips;

    [Tooltip("Min pitch (lower bound). 1.0 = original, 0.89 ≈ −2 semitones.")]
    public float pitchMin = 0.89f;

    [Tooltip("Max pitch (upper bound). 1.19 ≈ +3 semitones.")]
    public float pitchMax = 1.19f;

    /// <summary>Subscribe to receive sequences.</summary>
    public event Action<DialogueSequence> OnSequenceRequested;

    /// <summary>Call to start a sequence.</summary>
    public void Raise(DialogueSequence sequence)
    {
        if (sequence == null) return;
        OnSequenceRequested?.Invoke(sequence);
    }

    /// <summary>Stop the current sequence.</summary>
    public event Action OnStopRequested;
    public void Stop() => OnStopRequested?.Invoke();

    /// <summary>Fires when a sequence is fully completed.</summary>
    public event Action<DialogueSequence> OnSequenceCompleted;
    public void NotifyCompleted(DialogueSequence sequence) =>
        OnSequenceCompleted?.Invoke(sequence);

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
    public float GetRandomPitch()
    {
        return UnityEngine.Random.Range(pitchMin, pitchMax);
    }
}
