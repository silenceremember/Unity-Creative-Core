using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Attach to a child GameObject with a trigger collider near a painting.
/// On player enter — notifies PaintingQuestManager (which shows E-prompt).
/// On E press — painting smoothly snaps to correct position (one-shot).
/// </summary>
[RequireComponent(typeof(Collider))]
public class PaintingInteractable : MonoBehaviour
{
    [Header("Painting")]
    [Tooltip("Transform of the painting itself (not the trigger zone)")]
    [SerializeField] private Transform paintingTransform;

    [Tooltip("Code digit contributed by this painting (1-4)")]
    [Range(1, 4)]
    [SerializeField] private int codeDigit = 1;

    [Header("Correct Rotation")]
    [Tooltip("Target localEuler angles when painting is aligned")]
    [SerializeField] private Vector3 correctLocalEuler;

    [Header("Animation")]
    [SerializeField] private float snapDuration = 0.5f;

    [Header("Slide Sounds (3D)")]
    [Tooltip("1-4 clips. A random one is picked on each painting rotation.")]
    [SerializeField] private AudioClip[] slideClips;

    public bool IsUsed          { get; private set; }

    /// <summary>
    /// Index in pictureLabels (0-3), assigned on first press permanently.
    /// -1 = not yet assigned.
    /// </summary>
    public int  AssignedSlotIndex { get; set; } = -1;

    private Collider     _col;
    private Quaternion   _initialRotation;
    private AudioSource  _audioSource;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;

        // 3D AudioSource on the painting object itself
        _audioSource = paintingTransform != null
            ? paintingTransform.gameObject.GetComponent<AudioSource>()
              ?? paintingTransform.gameObject.AddComponent<AudioSource>()
            : gameObject.GetComponent<AudioSource>()
              ?? gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake  = false;
        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
        _audioSource.maxDistance  = 10f;
    }

    /// <summary>
    /// Call at quest start (after PaintingShiftController has already tilted paintings).
    /// Saves the current (tilted) rotation as the initial position for reset.
    /// </summary>
    public void CaptureQuestStart()
    {
        if (paintingTransform != null)
            _initialRotation = paintingTransform.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsUsed) return;
        if (!other.CompareTag("Player")) return;
        PaintingQuestManager.Instance?.OnPaintingEnter(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PaintingQuestManager.Instance?.OnPaintingExit(this);
    }

    /// <summary>Smoothly returns painting to correct position (one-shot).</summary>
    public void SnapToCorrect()
    {
        if (IsUsed) return;
        IsUsed = true;
        _col.enabled = false;

        PlaySlideSound();

        if (paintingTransform != null)
            SnapToAsync(Quaternion.Euler(correctLocalEuler), snapDuration, destroyCancellationToken).Forget();
    }

    /// <summary>Returns painting to initial (tilted) position and resets the flag.</summary>
    public void ResetPainting(float duration = 0.4f)
    {
        IsUsed = false;
        _col.enabled = true;

        PlaySlideSound();

        if (paintingTransform != null)
            SnapToAsync(_initialRotation, duration, destroyCancellationToken).Forget();
    }

    /// <summary>Plays a random 3D slide sound from the painting position.</summary>
    public void PlaySlideSound()
    {
        if (_audioSource == null || slideClips == null || slideClips.Length == 0) return;
        var clip = slideClips[Random.Range(0, slideClips.Length)];
        if (clip != null) _audioSource.PlayOneShot(clip);
    }

    private async UniTask SnapToAsync(Quaternion target, float dur, CancellationToken ct)
    {
        Quaternion start = paintingTransform.localRotation;
        float elapsed    = 0f;

        try
        {
            while (elapsed < dur)
            {
                ct.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float t  = Mathf.SmoothStep(0f, 1f, elapsed / dur);
                paintingTransform.localRotation = Quaternion.Slerp(start, target, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            paintingTransform.localRotation = target;
        }
        catch (System.OperationCanceledException) { }
    }
}
