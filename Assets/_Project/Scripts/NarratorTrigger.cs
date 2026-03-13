using UnityEngine;

/// <summary>
/// Box Trigger, который при входе игрока уведомляет ExplorationManager.
/// Добавь BoxCollider (isTrigger = true) на тот же GameObject.
/// </summary>
[RequireComponent(typeof(Collider))]
public class NarratorTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("0 = Триггер A, 1 = Триггер B")]
    public int triggerId = 0;

    [Tooltip("Срабатывает только один раз")]
    public bool oneShot = true;

    [Header("Визуализация (только в редакторе)")]
    public Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.25f);

    private bool _fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_fired && oneShot) return;
        if (!other.CompareTag("Player")) return;

        _fired = true;

        if (ExplorationManager.Instance != null)
            ExplorationManager.Instance.OnAreaTrigger(triggerId);

        Debug.Log($"[NarratorTrigger] Fired trigger id={triggerId}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        var col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Matrix4x4 old = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = old;
        }
    }
#endif
}
