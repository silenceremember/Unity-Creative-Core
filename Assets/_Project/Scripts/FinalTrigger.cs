using UnityEngine;

/// <summary>
/// Одноразовый триггер финальной последовательности.
/// triggerId = 0 → затемнение HDRI
/// triggerId = 1 → VFX падения + заморозка игрока + камера + нарратор + выход
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FinalTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("0 = Первый триггер (HDRI fade), 1 = Второй триггер (финал)")]
    public int triggerId = 0;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (FinalSequenceManager.Instance != null)
            FinalSequenceManager.Instance.OnFinalTrigger(triggerId);

        gameObject.SetActive(false);
    }
}
