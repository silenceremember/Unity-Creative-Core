using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Вешается на дочерний GameObject с trigger-коллайдером рядом с картиной.
/// При входе игрока — сообщает PaintingQuestManager (который показывает E-prompt).
/// При нажатии E — картина плавно встаёт на место (одноразово).
/// </summary>
[RequireComponent(typeof(Collider))]
public class PaintingInteractable : MonoBehaviour
{
    [Header("Картина")]
    [Tooltip("Transform самой картины (не триггер-зоны)")]
    public Transform paintingTransform;

    [Tooltip("Цифра кода которую вносит эта картина (1-4)")]
    [Range(1, 4)]
    public int codeDigit = 1;

    [Header("Правильный поворот")]
    [Tooltip("Целевые localEuler-углы когда картина выровнена")]
    public Vector3 correctLocalEuler;

    [Header("Анимация")]
    public float snapDuration = 0.5f;

    [Header("Звуки слайда (3D)")]
    [Tooltip("1–4 клипа. При каждом повороте картины выбирается рандомный.")]
    public AudioClip[] slideClips;

    // ── state ─────────────────────────────────────────────────────────────

    public bool IsUsed          { get; private set; }

    /// <summary>
    /// Индекс в pictureLabels (0-3), назначается при первом нажатии навсегда.
    /// -1 — ещё не назначен.
    /// </summary>
    public int  AssignedSlotIndex { get; set; } = -1;


    private Collider     _col;
    private Quaternion   _initialRotation;  // поворот при старте — нужен для reset после reject
    private AudioSource  _audioSource;

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
        // ВАЖНО: НЕ захватываем здесь — PaintingShiftController ещё не наклонил картины.
        // Захват делается через CaptureQuestStart() из PaintingQuestManager.StartQuest().

        // 3D AudioSource — вешается на сам объект картины (игрок слышит откуда именно)
        _audioSource = paintingTransform != null
            ? paintingTransform.gameObject.GetComponent<AudioSource>()
              ?? paintingTransform.gameObject.AddComponent<AudioSource>()
            : gameObject.GetComponent<AudioSource>()
              ?? gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake  = false;
        _audioSource.spatialBlend = 1f;   // полностью 3D
        _audioSource.rolloffMode  = AudioRolloffMode.Logarithmic;
        _audioSource.maxDistance  = 10f;
    }

    /// <summary>
    /// Вызвать при старте квеста (после PaintingShiftController уже наклонил картины).
    /// Сохраняет текущий (наклонённый) поворот как начальную позицию для reset.
    /// </summary>
    public void CaptureQuestStart()
    {
        if (paintingTransform != null)
            _initialRotation = paintingTransform.localRotation;
    }

    // ── Trigger ───────────────────────────────────────────────────────────

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

    // ── Snap ──────────────────────────────────────────────────────────────

    /// <summary>Плавно возвращает картину в правильное положение (одноразово).</summary>
    public void SnapToCorrect()
    {
        if (IsUsed) return;
        IsUsed = true;
        _col.enabled = false;

        PlaySlideSound();

        if (paintingTransform != null)
            SnapToAsync(Quaternion.Euler(correctLocalEuler), snapDuration, destroyCancellationToken).Forget();
    }

    /// <summary>Возвращает картину в начальное (наклонённое) положение и сбрасывает флаг.</summary>
    public void ResetPainting(float duration = 0.4f)
    {
        IsUsed = false;
        _col.enabled = true;

        PlaySlideSound();

        if (paintingTransform != null)
            SnapToAsync(_initialRotation, duration, destroyCancellationToken).Forget();
    }

    /// <summary>Играет рандомный звук слайда картины (3D, из позиции самой картины).</summary>
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
            Debug.Log($"[PaintingInteractable] '{paintingTransform.name}' snapped.");
        }
        catch (System.OperationCanceledException) { }
    }
}
