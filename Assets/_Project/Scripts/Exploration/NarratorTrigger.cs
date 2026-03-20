using UnityEngine;

/// <summary>
/// Collider trigger that fires a DialogueSequence through NarratorChannel
/// when the player enters. One-shot (deactivates after first use).
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class NarratorTrigger : MonoBehaviour
{
    [Header("Sequence")]
    [Tooltip("Dialogue to play when player enters")]
    [SerializeField] private DialogueSequence sequence;

    [Header("Channel")]
    [SerializeField] private NarratorChannel narratorChannel;

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

        gameObject.SetActive(false);
    }
}
