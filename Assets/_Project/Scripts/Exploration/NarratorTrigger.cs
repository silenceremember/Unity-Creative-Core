using UnityEngine;

/// <summary>
/// Box/Sphere trigger that notifies ExplorationManager when the player enters.
/// Add any Collider (isTrigger = true) on the same GameObject.
/// Rigidbody (kinematic) is auto-added — player does NOT need a Rigidbody.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class NarratorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("0 = Trigger A, 1 = Trigger B")]
    [SerializeField] private int triggerId;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ExplorationManager.Instance != null)
            ExplorationManager.Instance.OnAreaTrigger(triggerId);

        gameObject.SetActive(false);
    }
}
