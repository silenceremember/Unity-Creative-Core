using System;
using UnityEngine;

/// <summary>
/// Narrator event channel. ScriptableObject event bus.
/// NarratorManager subscribes; buttons/triggers call Raise().
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Narrator Channel", fileName = "NarratorChannel")]
public class NarratorChannel : ScriptableObject
{
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
}
