using UnityEngine;

/// <summary>
/// One-shot trigger for the final sequence.
/// triggerId = 0 → HDRI fade
/// triggerId = 1 → VFX fall + player freeze + camera + narrator + quit
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FinalTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("0 = First trigger (HDRI fade), 1 = Second trigger (finale)")]
    [SerializeField] private int triggerId;

    [Header("Dependencies")]
    [SerializeField] private FinalSequenceManager finalSequenceManager;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (finalSequenceManager != null)
            finalSequenceManager.OnFinalTrigger(triggerId);

        gameObject.SetActive(false);
    }
}
