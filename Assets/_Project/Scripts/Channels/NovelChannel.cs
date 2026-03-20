using System;
using UnityEngine;

/// <summary>
/// SO event bus for the visual novel.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Novel Channel", fileName = "NovelChannel")]
public class NovelChannel : ScriptableObject
{
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
