using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Reusable async UI animation helpers (UniTask-based).
/// All methods are safe to cancel and restore original values.
/// </summary>
public static class UIAnimationHelper
{
    /// <summary>
    /// Horizontal shake with linear decay.
    /// </summary>
    public static async UniTask ShakeAsync(
        RectTransform rt,
        float duration,
        float magnitude,
        CancellationToken ct)
    {
        if (rt == null) return;

        Vector2 origin  = rt.anchoredPosition;
        float   elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float x = Random.Range(-magnitude, magnitude) * (1f - elapsed / duration);
                rt.anchoredPosition = origin + new Vector2(x, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        rt.anchoredPosition = origin;
    }

    /// <summary>
    /// Sine-based scale pulse (punch effect without DOTween).
    /// </summary>
    public static async UniTask PulseAsync(
        RectTransform rt,
        float duration,
        float intensity,
        CancellationToken ct)
    {
        if (rt == null) return;

        Vector3 originScale = rt.localScale;
        float   elapsed     = 0f;

        try
        {
            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float t = Mathf.Sin(elapsed / duration * Mathf.PI);
                rt.localScale = originScale * (1f + intensity * t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        rt.localScale = originScale;
    }
}
