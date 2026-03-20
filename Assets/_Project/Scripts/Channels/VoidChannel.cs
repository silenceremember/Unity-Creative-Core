using System;
using UnityEngine;

/// <summary>
/// Parameterless SO event channel.
/// Fire-and-forget events between decoupled systems.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Void Channel", fileName = "VoidChannel")]
public class VoidChannel : ScriptableObject
{
    public event Action OnRaised;

    void OnEnable() => OnRaised = null;

    public void Raise() => OnRaised?.Invoke();
}
