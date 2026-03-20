using UnityEngine;

/// <summary>
/// Configuration for first-person controller and footstep system.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Player Config", fileName = "PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Max walk speed (m/s)")]
    [SerializeField] private float moveSpeed = 4f;
    public float MoveSpeed => moveSpeed;

    [Tooltip("Time to reach max speed")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float acceleration = 0.12f;
    public float Acceleration => acceleration;

    [Tooltip("Time to stop")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float deceleration = 0.08f;
    public float Deceleration => deceleration;

    [Tooltip("Gravity force")]
    [SerializeField] private float gravity = -18f;
    public float Gravity => gravity;

    [Tooltip("Max fall speed (m/s)")]
    [SerializeField] private float maxFallSpeed = 10f;
    public float MaxFallSpeed => maxFallSpeed;

    [Header("Look")]
    [Tooltip("Mouse sensitivity")]
    [Range(0.5f, 5f)]
    [SerializeField] private float mouseSensitivity = 1.8f;
    public float MouseSensitivity => mouseSensitivity;

    [Tooltip("Min vertical look angle")]
    [SerializeField] private float pitchMin = -75f;
    public float PitchMin => pitchMin;

    [Tooltip("Max vertical look angle")]
    [SerializeField] private float pitchMax = 75f;
    public float PitchMax => pitchMax;

    [Header("Camera Wall Clip Prevention")]
    [Tooltip("Sphere radius for camera collision check")]
    [Range(0.01f, 0.3f)]
    [SerializeField] private float cameraCollisionRadius = 0.1f;
    public float CameraCollisionRadius => cameraCollisionRadius;

    [Header("Footsteps")]
    [Tooltip("Distance between steps in meters")]
    [SerializeField] private float stepDistance = 2.1f;
    public float StepDistance => stepDistance;

    [Tooltip("Min horizontal speed to trigger a step")]
    [SerializeField] private float minMoveSpeed = 0.5f;
    public float MinMoveSpeed => minMoveSpeed;

    [Tooltip("Pitch lower bound")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float footstepPitchMin = 0.9f;
    public float FootstepPitchMin => footstepPitchMin;

    [Tooltip("Pitch upper bound")]
    [Range(0.5f, 1.5f)]
    [SerializeField] private float footstepPitchMax = 1.1f;
    public float FootstepPitchMax => footstepPitchMax;

    [Tooltip("Footstep volume")]
    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.5f;
    public float FootstepVolume => footstepVolume;
}
