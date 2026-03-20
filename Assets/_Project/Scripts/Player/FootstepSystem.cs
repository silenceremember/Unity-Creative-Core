using UnityEngine;

/// <summary>
/// Footstep sound system for first-person controller.
///
/// Principle (similar to Source Engine / Unreal):
///   - Steps tick based on DISTANCE TRAVELED, not a timer.
///   - Sound plays ONLY when isGrounded + horizontal speed > threshold.
///   - 18 variations + random pitch eliminate the "machine gun" effect.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepSystem : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;

    [Header("Audio Clips")]
    [Tooltip("Footstep clips (step-001 … step-018)")]
    [SerializeField] private AudioClip[] footstepClips;

    private CharacterController _cc;
    private AudioSource _audioSource;
    private float _distanceTravelled;
    private int _lastClipIndex = -1;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();

        _audioSource.playOnAwake  = false;
        _audioSource.loop         = false;
        _audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (!_cc.isGrounded)
        {
            _distanceTravelled = 0f;
            return;
        }

        Vector3 vel = _cc.velocity;
        float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;

        if (horizontalSpeed < config.MinMoveSpeed)
            return;

        _distanceTravelled += horizontalSpeed * Time.deltaTime;

        if (_distanceTravelled >= config.StepDistance)
        {
            PlayFootstep();
            _distanceTravelled -= config.StepDistance;
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = GetRandomClip();
        if (clip == null) return;

        _audioSource.pitch = Random.Range(config.FootstepPitchMin, config.FootstepPitchMax);
        _audioSource.PlayOneShot(clip, config.FootstepVolume);
    }

    /// <summary>
    /// Returns a random clip from footstepClips, excluding the previous one
    /// ("no-repeat" shuffle lite — without full shuffling).
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
