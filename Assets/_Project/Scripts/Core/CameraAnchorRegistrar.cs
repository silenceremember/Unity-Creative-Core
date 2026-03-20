using UnityEngine;

/// <summary>
/// Registers this Transform in a TransformRegistry SO under the given anchor.
/// Attach to camera anchor GameObjects in the scene.
/// </summary>
public class CameraAnchorRegistrar : MonoBehaviour
{
    [Tooltip("Registry to register into")]
    [SerializeField] private TransformRegistry registry;

    [Tooltip("Camera anchor identity for this position")]
    [SerializeField] private CameraAnchor anchor;

    void OnEnable()
    {
        if (registry != null)
            registry.Register(anchor, transform);
    }

    void OnDisable()
    {
        if (registry != null)
            registry.Unregister(anchor);
    }
}
