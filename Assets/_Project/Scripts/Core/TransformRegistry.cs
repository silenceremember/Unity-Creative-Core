using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime dictionary of CameraAnchor→Transform. Scene objects register/unregister themselves.
/// Resets automatically on play-mode enter via OnEnable.
/// </summary>
[CreateAssetMenu(menuName = "Game/Runtime/Transform Registry", fileName = "TransformRegistry")]
public class TransformRegistry : ScriptableObject
{
    private readonly Dictionary<CameraAnchor, Transform> _map = new();

    void OnEnable() => _map.Clear();

    /// <summary>Register a transform under the given anchor.</summary>
    public void Register(CameraAnchor anchor, Transform t)
    {
        if (t != null)
            _map[anchor] = t;
    }

    /// <summary>Remove a registration.</summary>
    public void Unregister(CameraAnchor anchor)
    {
        _map.Remove(anchor);
    }

    /// <summary>Try to resolve an anchor to a Transform.</summary>
    public bool TryGet(CameraAnchor anchor, out Transform t) => _map.TryGetValue(anchor, out t);
}
