using UnityEngine;

/// <summary>
/// One-shot collider trigger for the final sequence.
/// Each trigger instance fires its own actions:
///   - optionally plays a DialogueSequence via NarratorChannel
///   - optionally signals via VoidChannel (e.g., HDRI fade, finale start)
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FinalTrigger : MonoBehaviour
{
    [Header("Sequence (optional)")]
    [Tooltip("Dialogue to play when player enters")]
    [SerializeField] private DialogueSequence sequence;
    [SerializeField] private NarratorChannel narratorChannel;

    [Header("Signal (optional)")]
    [Tooltip("VoidChannel to raise on trigger (HDRI fade, finale start, etc.)")]
    [SerializeField] private VoidChannel signalChannel;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (sequence != null)
            narratorChannel?.Raise(sequence);

        signalChannel?.Raise();

        gameObject.SetActive(false);
    }
}
