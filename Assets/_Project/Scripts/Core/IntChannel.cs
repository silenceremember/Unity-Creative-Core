using System;
using UnityEngine;

/// <summary>
/// SO event channel carrying an int payload.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/Int Channel", fileName = "IntChannel")]
public class IntChannel : ScriptableObject
{
    public event Action<int> OnRaised;

    void OnEnable() => OnRaised = null;

    public void Raise(int value) => OnRaised?.Invoke(value);
}
