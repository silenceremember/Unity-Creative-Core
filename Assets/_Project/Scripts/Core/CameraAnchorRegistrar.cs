using UnityEngine;

/// <summary>
/// Registers this Transform in a TransformRegistry SO under the given key.
/// Attach to camera anchor GameObjects in the scene.
/// </summary>
public class CameraAnchorRegistrar : MonoBehaviour
{
    [Tooltip("Registry to register into")]
    [SerializeField] private TransformRegistry registry;

    [Tooltip("Key for this anchor (e.g. 'John', 'Mary', 'Player')")]
    [SerializeField] private string anchorKey;

    void OnEnable()
    {
        if (registry != null)
            registry.Register(anchorKey, transform);
    }

    void OnDisable()
    {
        if (registry != null)
            registry.Unregister(anchorKey);
    }
}
