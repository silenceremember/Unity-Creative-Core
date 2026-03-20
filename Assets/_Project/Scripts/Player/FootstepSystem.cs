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
    [Header("Step Settings")]
    [Tooltip("Distance between steps in meters (Half-Life 2 uses ~2.0)")]
    [SerializeField] private float stepDistance = 2.1f;

    [Tooltip("Min horizontal speed to trigger a step")]
    [SerializeField] private float minMoveSpeed = 0.5f;

    [Header("Audio Clips")]
    [Tooltip("Footstep clips (step-001 … step-018)")]
    [SerializeField] private AudioClip[] footstepClips;

    [Header("Pitch Variation")]
    [Tooltip("Pitch lower bound")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float pitchMin = 0.9f;

    [Tooltip("Pitch upper bound")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float pitchMax = 1.1f;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.5f;

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

        if (horizontalSpeed < minMoveSpeed)
            return;

        _distanceTravelled += horizontalSpeed * Time.deltaTime;

        if (_distanceTravelled >= stepDistance)
        {
            PlayFootstep();
            _distanceTravelled -= stepDistance;
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = GetRandomClip();
        if (clip == null) return;

        _audioSource.pitch = Random.Range(pitchMin, pitchMax);
        _audioSource.PlayOneShot(clip, footstepVolume);
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
