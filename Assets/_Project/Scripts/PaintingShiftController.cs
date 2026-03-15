using System.Threading;
using Cysharp.Threading.Tasks;
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

    public static PaintingShiftController Instance { get; private set; }
    void Awake() => Instance = this;

    /// <summary>[DEBUG] Принудительно запустить наклон картин.</summary>
    public void ForceShift()
    {
        _triggered = false;   // сбрасываем флаг
        DoShiftAsync(destroyCancellationToken).Forget();
        _triggered = true;
    }

    void OnEnable()
    {
        Debug.Log($"[PaintingShift] OnEnable — triggered={_triggered}, entries={paintings?.Length}");
        if (_triggered) return;
        _triggered = true;
        DoShiftAsync(destroyCancellationToken).Forget();
    }

    private async UniTask DoShiftAsync(CancellationToken ct)
    {
        if (paintings == null || paintings.Length == 0)
        {
            Debug.LogWarning("[PaintingShift] paintings array is empty — назначь в Inspector!");
            return;
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
            RotatePaintingAsync(paintings[i].painting, paintings[i].targetLocalEuler, ct).Forget();

            if (stagger > 0f)
                await UniTask.Delay(System.TimeSpan.FromSeconds(stagger), cancellationToken: ct);
        }
    }

    private async UniTask RotatePaintingAsync(Transform t, Vector3 targetEuler, CancellationToken ct)
    {
        Quaternion startRot = t.localRotation;
        Quaternion endRot   = Quaternion.Euler(targetEuler);

        if (duration <= 0f)
        {
            t.localRotation = endRot;
            return;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            t.localRotation = Quaternion.Slerp(startRot, endRot, p);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        t.localRotation = endRot;
        Debug.Log($"[PaintingShift] Done rotating '{t.name}'");
    }
}
