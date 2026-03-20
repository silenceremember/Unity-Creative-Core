using System;
using UnityEngine;

/// <summary>
/// SO event channel carrying a string payload.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/String Channel", fileName = "StringChannel")]
public class StringChannel : ScriptableObject
{
    public event Action<string> OnRaised;

    void OnEnable() => OnRaised = null;

    public void Raise(string value) => OnRaised?.Invoke(value);
}
