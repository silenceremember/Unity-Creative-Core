using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime dictionary of string→Transform. Scene objects register/unregister themselves.
/// Resets automatically on play-mode enter via OnEnable.
/// </summary>
[CreateAssetMenu(menuName = "Game/Runtime/Transform Registry", fileName = "TransformRegistry")]
public class TransformRegistry : ScriptableObject
{
    private readonly Dictionary<string, Transform> _map = new();

    void OnEnable() => _map.Clear();

    /// <summary>Register a transform under the given key.</summary>
    public void Register(string key, Transform t)
    {
        if (!string.IsNullOrEmpty(key) && t != null)
            _map[key] = t;
    }

    /// <summary>Remove a registration.</summary>
    public void Unregister(string key)
    {
        if (!string.IsNullOrEmpty(key))
            _map.Remove(key);
    }

    /// <summary>Try to resolve a key to a Transform.</summary>
    public bool TryGet(string key, out Transform t) => _map.TryGetValue(key, out t);
}
