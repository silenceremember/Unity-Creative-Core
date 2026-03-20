using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Smoothly moves Main Camera between main menu and settings menu positions.
/// Settings Camera is used only as an anchor (target point).
/// </summary>
public class CameraTransition : MonoBehaviour
{
    [Header("Anchors")]
    [Tooltip("Main menu anchor (usually Main Camera itself — auto-filled)")]
    [SerializeField] private Transform mainAnchor;

    [Tooltip("Settings menu anchor (Settings Camera)")]
    [SerializeField] private Transform settingsAnchor;

    [Header("Config")]
    [SerializeField] private CameraTransitionConfig config;

    private Transform _cam;
    private CancellationTokenSource _currentCts;
    private Vector3 _mainPos;
    private Quaternion _mainRot;

    void Awake()
    {
        _cam = transform;

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

    public void GoToSettings()
    {
        if (settingsAnchor == null) return;
        StartTransition(settingsAnchor.position, settingsAnchor.rotation);
    }

    public void GoToMain()
    {
        StartTransition(_mainPos, _mainRot);
    }

    private void StartTransition(Vector3 targetPos, Quaternion targetRot)
    {
        _currentCts?.Cancel();
        _currentCts?.Dispose();
        _currentCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

        DoTransitionAsync(targetPos, targetRot, _currentCts.Token).SuppressCancellationThrow().Forget();
    }

    private async UniTask DoTransitionAsync(Vector3 targetPos, Quaternion targetRot, CancellationToken ct)
    {
        Vector3 startPos = _cam.position;
        Quaternion startRot = _cam.rotation;
        float elapsed = 0f;

        while (elapsed < config.Duration)
        {
            ct.ThrowIfCancellationRequested();
            elapsed += Time.deltaTime;
            float t = config.BlendCurve.Evaluate(Mathf.Clamp01(elapsed / config.Duration));
            _cam.position = Vector3.Lerp(startPos, targetPos, t);
            _cam.rotation = Quaternion.Lerp(startRot, targetRot, t);
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _cam.position = targetPos;
        _cam.rotation = targetRot;
    }
}
