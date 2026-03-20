using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Shifts paintings (rotates to target angles) simultaneously
/// when NarratorManager activates this object via activateObject = "PaintingShift".
/// </summary>
public class PaintingShiftController : MonoBehaviour
{
    [System.Serializable]
    public struct PaintingEntry
    {
        public Transform painting;
        [Tooltip("Target Euler angles (localRotation) after shift")]
        public Vector3 targetLocalEuler;
    }

    [Header("Paintings")]
    [SerializeField] private PaintingEntry[] paintings;

    [Header("Config")]
    [SerializeField] private QuestConfig config;

    private bool _triggered;



    /// <summary>[DEBUG] Force shift all paintings.</summary>
    public void ForceShift()
    {
        _triggered = false;
        DoShiftAsync(destroyCancellationToken).Forget();
        _triggered = true;
    }

    void OnEnable()
    {
        if (_triggered) return;
        _triggered = true;
        DoShiftAsync(destroyCancellationToken).Forget();
    }

    private async UniTask DoShiftAsync(CancellationToken ct)
    {
        if (paintings == null || paintings.Length == 0) return;

        for (int i = 0; i < paintings.Length; i++)
        {
            if (paintings[i].painting == null) continue;
            RotatePaintingAsync(paintings[i].painting, paintings[i].targetLocalEuler, ct).Forget();

            if (config.ShiftStagger > 0f)
                await UniTask.Delay(System.TimeSpan.FromSeconds(config.ShiftStagger), cancellationToken: ct);
        }
    }

    private async UniTask RotatePaintingAsync(Transform t, Vector3 targetEuler, CancellationToken ct)
    {
        Quaternion startRot = t.localRotation;
        Quaternion endRot   = Quaternion.Euler(targetEuler);

        if (config.ShiftDuration <= 0f)
        {
            t.localRotation = endRot;
            return;
        }

        float elapsed = 0f;
        while (elapsed < config.ShiftDuration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / config.ShiftDuration);
            t.localRotation = Quaternion.Slerp(startRot, endRot, p);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        t.localRotation = endRot;
    }
}
