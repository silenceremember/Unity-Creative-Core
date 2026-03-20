using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages the final game sequence.
///
/// Trigger 0 (VoidChannel hdriFadeChannel):
///   Locks player XZ, smoothly fades HDRI exposure → 0.
///   (Dialogue is fired by FinalTrigger itself.)
///
/// Trigger 1 (VoidChannel finaleStartChannel):
///   1. Freezes player.
///   2. Camera detaches and moves to cameraEndAnchor, looking at player.
///   3. Starts seqFinalPart1 narrator dialogue.
///   4. After part1 → ESC hint + camera pull-back to cameraFarAnchor.
///   5. After part2 → Application.Quit().
/// </summary>
public class FinalSequenceManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;

    [Header("GameState")]
    [SerializeField] private GameStateChannel gameStateChannel;

    [Header("Config")]
    [SerializeField] private FinalConfig config;

    [Header("HDRI Fade")]
    [Tooltip("Skybox material (HDRI)")]
    [SerializeField] private Material hdriMaterial;

    [Header("Camera Cinematic")]
    [Tooltip("Camera destination point")]
    [SerializeField] private Transform cameraEndAnchor;

    [Tooltip("Camera far point — pull-back at the end")]
    [SerializeField] private Transform cameraFarAnchor;

    [Header("Narrator")]
    [SerializeField] private NarratorChannel narratorChannel;
    [SerializeField] private DialogueSequence seqFinalPart1;
    [SerializeField] private DialogueSequence seqFinalPart2;

    [Header("Channels")]
    [Tooltip("Raised by first FinalTrigger — starts HDRI fade")]
    [SerializeField] private VoidChannel hdriFadeChannel;

    [Tooltip("Raised by second FinalTrigger — starts finale")]
    [SerializeField] private VoidChannel finaleStartChannel;

    [Header("ESC Hint")]
    [SerializeField] private GameObject escHintUI;

    private enum FinalState { Idle, HdriFading, CameraMoving, NarratorPart1, EscWait, NarratorPart2 }
    private FinalState _state = FinalState.Idle;
    private bool       _escEnabled = false;
    private Material   _hdriClone;

    void Awake()
    {
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
        if (hdriFadeChannel != null)
            hdriFadeChannel.OnRaised += OnHdriFadeTrigger;
        if (finaleStartChannel != null)
            finaleStartChannel.OnRaised += OnFinaleStart;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (hdriFadeChannel != null)
            hdriFadeChannel.OnRaised -= OnHdriFadeTrigger;
        if (finaleStartChannel != null)
            finaleStartChannel.OnRaised -= OnFinaleStart;
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

    private void OnHdriFadeTrigger()
    {
        if (_state != FinalState.Idle) return;
        _state = FinalState.HdriFading;

        if (playerTransform != null)
        {
            var pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null) pc.SetLockXZ();
        }

        FadeHDRI(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnFinaleStart()
    {
        if (_state == FinalState.CameraMoving) return;
        _state = FinalState.CameraMoving;

        gameStateChannel?.Raise(GameState.Final);
        StartFinalSequence(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask FadeHDRI(CancellationToken ct)
    {
        string prop = config.HdriExposureProperty;
        if (_hdriClone == null || !_hdriClone.HasProperty(prop))
            return;

        float elapsed  = 0f;
        float startExp = _hdriClone.GetFloat(prop);

        while (elapsed < config.HdriDarkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / config.HdriDarkDuration));
            _hdriClone.SetFloat(prop, Mathf.Lerp(startExp, 0f, t));
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _hdriClone.SetFloat(prop, 0f);
    }

    private async UniTask StartFinalSequence(CancellationToken ct)
    {
        FreezePlayer();
        await MoveCameraToAnchor(ct);

        await UniTask.Delay(
            (int)(config.NarratorDelayAfterCamera * 1000f),
            cancellationToken: ct);

        if (seqFinalPart1 != null)
        {
            _state = FinalState.NarratorPart1;
            narratorChannel?.Raise(seqFinalPart1);
        }
        else
        {
            await QuitSequence(ct);
        }
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
        await MoveCameraToTarget(cameraEndAnchor, config.CameraTravelDuration, ct);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (_state == FinalState.NarratorPart1)
        {
            _state = FinalState.EscWait;
            _escEnabled = true;
            if (escHintUI != null) escHintUI.SetActive(true);
            if (cameraFarAnchor != null)
                MoveCameraToTarget(cameraFarAnchor, config.CameraFarDuration, this.GetCancellationTokenOnDestroy()).Forget();

            if (seqFinalPart2 != null)
            {
                _state = FinalState.NarratorPart2;
                narratorChannel?.Raise(seqFinalPart2);
            }
            return;
        }

        if (_state == FinalState.NarratorPart2)
        {
            QuitSequence(this.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    /// <summary>Smoothly moves camera to <paramref name="target"/>, looking at the player.</summary>
    private async UniTask MoveCameraToTarget(Transform target, float duration, CancellationToken ct)
    {
        if (mainCamera == null || target == null) return;

        var curve = config.CameraCurve;
        var    camT     = mainCamera.transform;
        Vector3    startPos = camT.position;
        Quaternion startRot = camT.rotation;
        float  elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve != null && curve.length > 0
                ? curve.Evaluate(Mathf.Clamp01(elapsed / duration))
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
        await UniTask.Delay((int)(config.QuitDelay * 1000f), cancellationToken: ct);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
