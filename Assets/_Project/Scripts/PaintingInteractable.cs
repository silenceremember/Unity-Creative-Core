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

    // ── state ─────────────────────────────────────────────────────────────

    public bool IsUsed          { get; private set; }

    /// <summary>
    /// Индекс в pictureLabels (0-3), назначается при первом нажатии навсегда.
    /// -1 — ещё не назначен.
    /// </summary>
    public int  AssignedSlotIndex { get; set; } = -1;


    private Collider   _col;
    private Quaternion _initialRotation;  // поворот при старте — нужен для reset после reject

    void Awake()
    {
        _col = GetComponent<Collider>();
        _col.isTrigger = true;
        // ВАЖНО: НЕ захватываем здесь — PaintingShiftController ещё не наклонил картины.
        // Захват делается через CaptureQuestStart() из PaintingQuestManager.StartQuest().
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

        if (paintingTransform != null)
            SnapToAsync(Quaternion.Euler(correctLocalEuler), snapDuration, destroyCancellationToken).Forget();
    }

    /// <summary>Возвращает картину в начальное (наклонённое) положение и сбрасывает флаг.</summary>
    public void ResetPainting(float duration = 0.4f)
    {
        IsUsed = false;
        _col.enabled = true;
        if (paintingTransform != null)
            SnapToAsync(_initialRotation, duration, destroyCancellationToken).Forget();
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
