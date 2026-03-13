using UnityEngine;

/// <summary>
/// Box/Sphere Trigger, который при входе игрока уведомляет ExplorationManager.
/// Добавь любой Collider (isTrigger = true) на тот же GameObject.
/// Rigidbody (kinematic) добавляется автоматически на этот же объект —
/// игроку Rigidbody НЕ нужен.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class NarratorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("0 = Триггер A, 1 = Триггер B")]
    public int triggerId = 0;

    private void Awake()
    {
        // Rigidbody на триггере: кинематика, нет гравитации — физику игрока не трогаем
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ExplorationManager.Instance != null)
            ExplorationManager.Instance.OnAreaTrigger(triggerId);

        // Отключаем сразу — больше не нужен
        gameObject.SetActive(false);
    }
}
