using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages the final game sequence (two triggers).
///
/// Trigger 0: smoothly fades HDRI (skybox exposure → 0) + starts dialogue.
/// Trigger 1:
///   1. Freezes the player (PlayerController + CharacterController disabled).
///   2. Camera smoothly detaches and moves to cameraEndAnchor, looking at the player.
///   3. Starts the final narrator dialogue.
///   4. After dialogue ends → Application.Quit().
/// </summary>
public class FinalSequenceManager : MonoBehaviour
{
    public static FinalSequenceManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;

    [Header("GameState")]
    [SerializeField] private GameStateChannel gameStateChannel;

    [Header("HDRI Fade (Trigger 1)")]
    [Tooltip("Skybox material (HDRI)")]
    [SerializeField] private Material hdriMaterial;

    [Tooltip("HDRI fade duration in seconds")]
    [SerializeField] private float hdriDarkDuration = 4f;

    [Tooltip("Exposure property name in the material")]
    [SerializeField] private string hdriExposureProperty = "_Exposure";

    [Header("Narrator (Trigger 0)")]
    [SerializeField] private DialogueSequence seqTrigger0;

    [Header("Camera Cinematic (Trigger 2)")]
    [Tooltip("Camera destination point")]
    [SerializeField] private Transform cameraEndAnchor;

    [Tooltip("Camera travel duration (sec)")]
    [SerializeField] private float cameraTravelDuration = 2.5f;

    [SerializeField] private AnimationCurve cameraCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Camera far point — pull-back at the end")]
    [SerializeField] private Transform cameraFarAnchor;

    [Tooltip("Pull-back duration (sec)")]
    [SerializeField] private float cameraFarDuration = 4f;

    [Header("Narrator")]
    [SerializeField] private NarratorChannel narratorChannel;
    [SerializeField] private DialogueSequence seqFinalPart1;
    [SerializeField] private DialogueSequence seqFinalPart2;

    [Tooltip("Delay before starting final dialogue (sec)")]
    [SerializeField] private float narratorDelayAfterCamera = 0.5f;

    [Header("ESC Hint")]
    [SerializeField] private GameObject escHintUI;

    [Header("Quit")]
    [Tooltip("Delay after part 2 ends before quitting (sec)")]
    [SerializeField] private float quitDelay = 1.5f;

    private bool     _trigger1Done  = false;
    private bool     _trigger2Done  = false;
    private bool     _escEnabled    = false;
    private Material _hdriClone;

    void Awake()
    {
        Instance = this;

        if (hdriMaterial != null)
        {
            _hdriClone = new Material(hdriMaterial);
            RenderSettings.skybox = _hdriClone;
        }
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
    }

    void Update()
    {
        if (!_escEnabled) return;
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            QuitSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    public void OnFinalTrigger(int triggerId)
    {
        if (triggerId == 0 && !_trigger1Done)
        {
            _trigger1Done = true;

            if (playerTransform != null)
            {
                var pc = playerTransform.GetComponent<PlayerController>();
                if (pc != null) pc.LockXZ();
            }

            FadeHDRI(this.GetCancellationTokenOnDestroy()).Forget();
            if (seqTrigger0 != null)
                narratorChannel?.Raise(seqTrigger0);
        }
        else if (triggerId == 1 && !_trigger2Done)
        {
            _trigger2Done = true;
            gameStateChannel?.Raise(GameState.Final);
            StartFinalSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    private async UniTask FadeHDRI(CancellationToken ct)
    {
        if (_hdriClone == null || !_hdriClone.HasProperty(hdriExposureProperty))
            return;

        float elapsed  = 0f;
        float startExp = _hdriClone.GetFloat(hdriExposureProperty);

        while (elapsed < hdriDarkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / hdriDarkDuration));
            _hdriClone.SetFloat(hdriExposureProperty, Mathf.Lerp(startExp, 0f, t));
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _hdriClone.SetFloat(hdriExposureProperty, 0f);
    }

    private async UniTask StartFinalSequence(CancellationToken ct)
    {
        FreezePlayer();
        await MoveCameraToAnchor(ct);

        await UniTask.Delay(
            (int)(narratorDelayAfterCamera * 1000f),
            cancellationToken: ct);

        if (seqFinalPart1 != null)
            narratorChannel?.Raise(seqFinalPart1);
        else
            await QuitSequence(ct);
    }

    private void FreezePlayer()
    {
        if (playerTransform != null)
        {
            var pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            var cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            var mr = playerTransform.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = true;
                mr.lightProbeUsage = LightProbeUsage.Off;
            }
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private async UniTask MoveCameraToAnchor(CancellationToken ct)
    {
        if (mainCamera == null || cameraEndAnchor == null) return;

        mainCamera.transform.SetParent(null, worldPositionStays: true);
        await MoveCameraToTarget(cameraEndAnchor, cameraTravelDuration, ct);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (completed == seqFinalPart1)
        {
            _escEnabled = true;
            if (escHintUI != null) escHintUI.SetActive(true);
            if (cameraFarAnchor != null)
                MoveCameraToTarget(cameraFarAnchor, cameraFarDuration, this.GetCancellationTokenOnDestroy()).Forget();
            return;
        }

        if (completed == seqFinalPart2)
        {
            QuitSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    /// <summary>Smoothly moves camera to <paramref name="target"/>, looking at the player.</summary>
    private async UniTask MoveCameraToTarget(Transform target, float duration, CancellationToken ct)
    {
        if (mainCamera == null || target == null) return;

        var    camT     = mainCamera.transform;
        Vector3    startPos = camT.position;
        Quaternion startRot = camT.rotation;
        float  elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = cameraCurve != null && cameraCurve.length > 0
                ? cameraCurve.Evaluate(Mathf.Clamp01(elapsed / duration))
                : Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            camT.position = Vector3.Lerp(startPos, target.position, t);

            if (playerTransform != null)
            {
                Vector3 dir = (playerTransform.position - camT.position).normalized;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                    camT.rotation = Quaternion.Lerp(startRot, targetRot, t);
                }
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        camT.position = target.position;
        if (playerTransform != null)
        {
            Vector3 dir = (playerTransform.position - camT.position).normalized;
            if (dir.sqrMagnitude > 0.001f)
                camT.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    private async UniTask QuitSequence(CancellationToken ct)
    {
        await UniTask.Delay((int)(quitDelay * 1000f), cancellationToken: ct);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
