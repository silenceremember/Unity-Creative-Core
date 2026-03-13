using System.Collections;
using UnityEngine;

/// <summary>
/// Сдвигает картины (поворачивает на заданные углы) одновременно,
/// когда NarratorManager активирует этот объект через activateObject = "PaintingShift".
///
/// Подключи этот компонент на пустой GameObject "PaintingShift" в сцене.
/// Назначь 4 картины в массив paintings и задай целевые углы targetLocalEuler.
/// Зарегистрируй "PaintingShift" → этот GameObject в NarratorManager → sceneObjects.
/// </summary>
public class PaintingShiftController : MonoBehaviour
{
    [System.Serializable]
    public struct PaintingEntry
    {
        public Transform painting;
        [Tooltip("Целевые Euler-углы (localRotation) после сдвига")]
        public Vector3 targetLocalEuler;
    }

    [Header("Картины и целевые углы")]
    public PaintingEntry[] paintings;

    [Header("Анимация")]
    [Tooltip("Время перехода в секундах (0 = мгновенно)")]
    public float duration = 0.6f;

    [Tooltip("Задержка между картинами (0 = все одновременно)")]
    public float stagger = 0f;

    // Уже сработал — не срабатываем повторно
    private bool _triggered;

    void OnEnable()
    {
        Debug.Log($"[PaintingShift] OnEnable — triggered={_triggered}, entries={paintings?.Length}");
        if (_triggered) return;
        _triggered = true;
        StartCoroutine(DoShift());
    }

    private IEnumerator DoShift()
    {
        if (paintings == null || paintings.Length == 0)
        {
            Debug.LogWarning("[PaintingShift] paintings array is empty — назначь в Inspector!");
            yield break;
        }

        Debug.Log($"[PaintingShift] Starting shift of {paintings.Length} paintings");

        for (int i = 0; i < paintings.Length; i++)
        {
            if (paintings[i].painting == null)
            {
                Debug.LogWarning($"[PaintingShift] paintings[{i}].painting is NULL — назначь Transform в Inspector!");
                continue;
            }
            Debug.Log($"[PaintingShift] Rotating '{paintings[i].painting.name}' → {paintings[i].targetLocalEuler}");
            StartCoroutine(RotatePainting(paintings[i].painting, paintings[i].targetLocalEuler));

            if (stagger > 0f)
                yield return new WaitForSeconds(stagger);
        }
    }

    private IEnumerator RotatePainting(Transform t, Vector3 targetEuler)
    {
        Quaternion startRot = t.localRotation;
        Quaternion endRot   = Quaternion.Euler(targetEuler);

        if (duration <= 0f)
        {
            t.localRotation = endRot;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            t.localRotation = Quaternion.Slerp(startRot, endRot, p);
            yield return null;
        }
        t.localRotation = endRot;
        Debug.Log($"[PaintingShift] Done rotating '{t.name}'");
    }
}
