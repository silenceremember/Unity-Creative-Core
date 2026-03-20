using UnityEngine;

/// <summary>
/// Activates a target GameObject when a matching key is received via StringChannel.
/// Attach to any always-active parent. If target is not set, activates own GameObject.
/// </summary>
public class EventActivator : MonoBehaviour
{
    [Tooltip("Channel to listen on")]
    [SerializeField] private StringChannel channel;

    [Tooltip("Key that triggers activation (must match DialogueLine.activateObject)")]
    [SerializeField] private string key;

    [Tooltip("Object to activate. Leave empty to activate this GameObject.")]
    [SerializeField] private GameObject target;

    void OnEnable()
    {
        if (channel != null) channel.OnRaised += OnEventRaised;
    }

    void OnDisable()
    {
        if (channel != null) channel.OnRaised -= OnEventRaised;
    }

    private void OnEventRaised(string receivedKey)
    {
        if (receivedKey == key)
        {
            var obj = target != null ? target : gameObject;
            obj.SetActive(true);
        }
    }
}
