using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Плавно перемещает Main Camera между позицией главного меню и меню настроек.
/// Settings Camera используется только как anchor (целевая точка).
/// </summary>
public class CameraTransition : MonoBehaviour
{
    [Header("Anchors")]
    [Tooltip("Якорная точка главного меню (обычно сам Main Camera - заполнится автоматически)")]
    public Transform mainAnchor;

    [Tooltip("Якорная точка меню настроек (Settings Camera)")]
    public Transform settingsAnchor;

    [Header("Settings")]
    [Range(0.1f, 3f)]
    public float duration = 0.8f;

    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ──────────────────────────────────────────
    private Transform _cam;
    private CancellationTokenSource _currentCts;
    private Vector3 _mainPos;
    private Quaternion _mainRot;

    void Awake()
    {
        _cam = transform;

        // Если anchor не назначен — запоминаем текущую позицию камеры как «главное меню»
        if (mainAnchor == null)
        {
            _mainPos = _cam.position;
            _mainRot = _cam.rotation;
        }
        else
        {
            _mainPos = mainAnchor.position;
            _mainRot = mainAnchor.rotation;
        }
    }

    void OnDestroy()
    {
        _currentCts?.Cancel();
        _currentCts?.Dispose();
    }

    // ──── Public API ────────────────────────────

    public void GoToSettings()
    {
        if (settingsAnchor == null) return;
        StartTransition(settingsAnchor.position, settingsAnchor.rotation);
    }

    public void GoToMain()
    {
        StartTransition(_mainPos, _mainRot);
    }

    // ──────────────────────────────────────────

    private void StartTransition(Vector3 targetPos, Quaternion targetRot)
    {
        // Отменяем предыдущую анимацию
        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        DoTransitionAsync(targetPos, targetRot, _currentCts.Token).Forget();
    }

    private async UniTask DoTransitionAsync(Vector3 targetPos, Quaternion targetRot, CancellationToken ct)
    {
        Vector3 startPos = _cam.position;
        Quaternion startRot = _cam.rotation;
        float elapsed = 0f;

        try
        {
            while (elapsed < duration)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
                _cam.position = Vector3.Lerp(startPos, targetPos, t);
                _cam.rotation = Quaternion.Lerp(startRot, targetRot, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Финальный снэп, чтобы не было погрешности
            _cam.position = targetPos;
            _cam.rotation = targetRot;
        }
        catch (System.OperationCanceledException) { }
    }
}
