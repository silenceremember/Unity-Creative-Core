using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person player controller:
///   - WASD movement with smooth acceleration/deceleration
///   - Mouse look (horizontal = player, vertical = camera)
///   - Camera wall-push: SphereCast prevents camera from clipping through walls
///   - Near clip plane reduced to 0.02 to avoid close geometry clipping
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig config;



    private CharacterController _cc;
    private Camera _camera;
    private Transform _cameraTransform;

    private Vector3 _currentVelocityXZ;
    private Vector3 _smoothDampVelocity;

    private float _cameraPitch = 0f;
    private float _verticalVelocity;

    private int _wallMask;

    /// <summary>
    /// Lock horizontal movement (WASD).
    /// Gravity and CharacterController remain active — player keeps falling.
    /// </summary>
    [SerializeField, HideInInspector] private bool _lockMovement = false;
    public bool LockMovement { get => _lockMovement; set => _lockMovement = value; }

    /// <summary>
    /// Full X/Z freeze: instantly zeroes horizontal velocity
    /// and prevents it from accumulating. Player falls straight down.
    /// </summary>
    [SerializeField, HideInInspector] private bool _lockXZ = false;
    public bool LockXZ { get => _lockXZ; set => _lockXZ = value; }

    /// <summary>Enable strict vertical-fall mode.</summary>
    public void SetLockXZ()
    {
        _lockMovement       = true;
        _lockXZ             = true;
        _currentVelocityXZ  = Vector3.zero;
        _smoothDampVelocity = Vector3.zero;
    }

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _wallMask = ~(1 << gameObject.layer);
    }

    /// <summary>
    /// Called from GameplaySetup when entering Gameplay.
    /// </summary>
    public void Init(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
        _camera = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : null;

        if (_camera != null)
            _camera.nearClipPlane = 0.02f;

        _cameraPitch = cameraTransform != null
            ? cameraTransform.localEulerAngles.x
            : 0f;
        if (_cameraPitch > 180f) _cameraPitch -= 360f;
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void LateUpdate()
    {
        if (_cameraTransform != null)
            PushCameraFromWalls();
    }

    private void HandleRotation()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * (config.MouseSensitivity * 0.08f);

        transform.Rotate(Vector3.up, delta.x, Space.World);

        if (_cameraTransform != null)
        {
            _cameraPitch -= delta.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, config.PitchMin, config.PitchMax);
            _cameraTransform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float h = _lockMovement ? 0f : (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = _lockMovement ? 0f : (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 wishDir = transform.right * h + transform.forward * v;
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);

        Vector3 targetVelocity = wishDir * config.MoveSpeed;

        if (_lockXZ)
        {
            _currentVelocityXZ  = Vector3.zero;
            _smoothDampVelocity = Vector3.zero;
        }
        else
        {
            float smoothTime = wishDir.sqrMagnitude > 0.01f ? config.Acceleration : config.Deceleration;
            _currentVelocityXZ = Vector3.SmoothDamp(
                _currentVelocityXZ, targetVelocity, ref _smoothDampVelocity, smoothTime);
        }

        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -4f;

        _verticalVelocity += config.Gravity * Time.deltaTime;
        _verticalVelocity = Mathf.Max(_verticalVelocity, -config.MaxFallSpeed);

        Vector3 move = _currentVelocityXZ;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    /// <summary>
    /// SphereCast from player head to camera.
    /// If a wall is in the way — pushes the camera in front of it.
    /// </summary>
    private void PushCameraFromWalls()
    {
        Vector3 origin = transform.position + config.CameraLocalOffset;
        Vector3 desiredPos = origin;

        Vector3 lookDir = _cameraTransform.forward;
        float checkDist = 0.15f;

        if (Physics.SphereCast(origin, config.CameraCollisionRadius,
                               lookDir, out RaycastHit hit,
                               checkDist, _wallMask,
                               QueryTriggerInteraction.Ignore))
        {
            float pushBack = checkDist - hit.distance + config.CameraCollisionRadius;
            desiredPos = origin - lookDir * pushBack;
        }

        _cameraTransform.position = desiredPos;
    }


}
