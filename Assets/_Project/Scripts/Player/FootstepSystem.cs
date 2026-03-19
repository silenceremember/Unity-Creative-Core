using UnityEngine;

/// <summary>
/// Система звуков шагов для first-person контроллера.
///
/// Принцип (как в Source Engine / Unreal):
///   - Шаги тикают по ПРОЙДЕННОМУ РАССТОЯНИЮ, а не по таймеру.
///   - Звук играет ТОЛЬКО при isGrounded + горизонтальное движение > порога.

///   - 18 вариаций + случайный pitch устраняют эффект "машинного пулемёта".
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepSystem : MonoBehaviour
{
    // ─── Step Settings ────────────────────────────────────────────
    [Header("Step Settings")]
    [Tooltip("Расстояние между шагами в метрах (Half-Life 2 использует ~2.0)")]
    public float stepDistance = 2.1f;

    [Tooltip("Минимальная горизонтальная скорость для воспроизведения шага")]
    public float minMoveSpeed = 0.5f;

    // ─── Audio Clips ──────────────────────────────────────────────
    [Header("Audio Clips")]
    [Tooltip("Клипы шагов (step-001 … step-018)")]
    public AudioClip[] footstepClips;

    // ─── Pitch Variation ──────────────────────────────────────────
    [Header("Pitch Variation")]
    [Tooltip("Нижняя граница диапазона pitch")]
    [Range(0.5f, 1.5f)]
    public float pitchMin = 0.9f;

    [Tooltip("Верхняя граница диапазона pitch")]
    [Range(0.5f, 1.5f)]
    public float pitchMax = 1.1f;

    // ─── Volume ───────────────────────────────────────────────────
    [Header("Volume")]
    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;

    // ─── Private ──────────────────────────────────────────────────
    private CharacterController _cc;
    private AudioSource _audioSource;

    private float _distanceTravelled;   // накопленное расстояние с последнего шага

    private int _lastClipIndex = -1;    // индекс последнего клипа (не повторяем подряд)

    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();

        // AudioSource для шагов не должен играть в loop и иметь свой клип
        _audioSource.playOnAwake = false;
        _audioSource.loop        = false;
        _audioSource.spatialBlend = 0f; // 2D — FPS-шаги не нуждаются в 3D-позиционировании
    }

    void Update()
    {
        bool grounded = _cc.isGrounded;

        // ── Шаги ─────────────────────────────────────────────────
        if (!grounded)
        {
            // В воздухе — сбрасываем накопленное расстояние
            _distanceTravelled = 0f;
            return;
        }

        // Горизонтальная скорость (Y игнорируем — он про гравитацию)
        Vector3 vel = _cc.velocity;
        float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;

        if (horizontalSpeed < minMoveSpeed)
        {
            // Стоим — не накапливаем расстояние, но не сбрасываем
            // (чтобы не было "половинного" шага при остановке-старте)
            return;
        }

        _distanceTravelled += horizontalSpeed * Time.deltaTime;

        if (_distanceTravelled >= stepDistance)
        {
            PlayFootstep();
            // Вычитаем, а не обнуляем — сохраняем дробный остаток для точности
            _distanceTravelled -= stepDistance;
        }
    }

    // ─── Play helpers ─────────────────────────────────────────────

    private void PlayFootstep()
    {
        AudioClip clip = GetRandomClip();
        if (clip == null) return;

        _audioSource.pitch = Random.Range(pitchMin, pitchMax);
        _audioSource.PlayOneShot(clip, footstepVolume);
    }


    /// <summary>
    /// Возвращает случайный клип из footstepClips, исключая предыдущий
    /// (алгоритм "no-repeat" shuffle lite — без тяжёлой перетасовки).
    /// </summary>
    private AudioClip GetRandomClip()
    {
        if (footstepClips == null || footstepClips.Length == 0) return null;
        if (footstepClips.Length == 1) return footstepClips[0];

        int idx;
        do { idx = Random.Range(0, footstepClips.Length); }
        while (idx == _lastClipIndex);

        _lastClipIndex = idx;
        return footstepClips[idx];
    }
}
