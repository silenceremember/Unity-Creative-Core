using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// First-person контроллер игрока:
///   - WASD движение с плавным ускорением/торможением
///   - Вращение мышью (горизонталь игрок, вертикаль камера)
///   - Camera wall-push: SphereCast предотвращает прохождение камеры через стены
///   - Near clip plane уменьшается до 0.02 чтобы убрать отсечение близкой геометрии
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─── Movement ────────────────────────────────────────────
    [Header("Movement")]
    [Tooltip("Максимальная скорость ходьбы (м/с)")]
    public float moveSpeed = 4f;

    [Tooltip("Время разгона до максимальной скорости (чем меньше — тем резче)")]
    [Range(0.05f, 0.5f)]
    public float acceleration = 0.12f;

    [Tooltip("Время торможения (чем меньше — тем резче)")]
    [Range(0.05f, 0.5f)]
    public float deceleration = 0.08f;

    [Tooltip("Сила гравитации")]
    public float gravity = -18f;   // реалистичнее чем -9.81

    // ─── Look ────────────────────────────────────────────────
    [Header("Look")]
    [Tooltip("Чувствительность мыши")]
    [Range(0.5f, 5f)]
    public float mouseSensitivity = 1.8f;

    [Tooltip("Мин/макс угол взгляда вверх/вниз")]
    public float pitchMin = -75f;
    public float pitchMax = 75f;

    // ─── Camera wall push ────────────────────────────────────
    [Header("Camera Wall Clip Prevention")]
    [Tooltip("Радиус сферы при проверке столкновения камеры со стеной")]
    [Range(0.01f, 0.3f)]
    public float cameraCollisionRadius = 0.1f;

    [Tooltip("Смещение камеры от пивота игрока в нормальном состоянии")]
    public Vector3 cameraBaseLocalOffset = new Vector3(0f, 0.7f, 0f);

    // ─── Private ─────────────────────────────────────────────
    private CharacterController _cc;
    private Camera _camera;
    private Transform _cameraTransform;

    private Vector3 _currentVelocityXZ;   // горизонтальная скорость для SmoothDamp
    private Vector3 _smoothDampVelocity;  // внутренний ref для SmoothDamp

    private float _cameraPitch = 0f;
    private float _verticalVelocity;

    // Маски для camera raycast — исключаем игрока
    private int _wallMask;

    public static PlayerController Instance { get; private set; }

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;
        _cc = GetComponent<CharacterController>();
        // Игнорируем слой самого игрока при camera raycast
        _wallMask = ~(1 << gameObject.layer);
    }

    /// <summary>
    /// Вызывается из GameplaySetup при входе в Gameplay.
    /// </summary>
    public void Init(Transform cameraTransform)
    {
        _cameraTransform = cameraTransform;
        _camera = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : null;

        // Уменьшаем near clip plane — убирает артефакты клиппинга вблизи
        if (_camera != null)
            _camera.nearClipPlane = 0.02f;

        // Начальный pitch
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
        // Camera wall push — вызываем ПОСЛЕ физики (LateUpdate)
        if (_cameraTransform != null)
            PushCameraFromWalls();
    }

    // ─── Rotation ─────────────────────────────────────────────

    private void HandleRotation()
    {
        if (Mouse.current == null) return;

        // delta.ReadValue() возвращает пиксели/фрейм → нормируем
        Vector2 delta = Mouse.current.delta.ReadValue() * (mouseSensitivity * 0.08f);

        // Горизонталь — поворот игрока
        transform.Rotate(Vector3.up, delta.x, Space.World);

        // Вертикаль — наклон камеры
        if (_cameraTransform != null)
        {
            _cameraPitch -= delta.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, pitchMin, pitchMax);
            _cameraTransform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
        }
    }

    // ─── Movement ─────────────────────────────────────────────

    private void HandleMovement()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Входной вектор
        float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        Vector3 wishDir = transform.right * h + transform.forward * v;
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);

        Vector3 targetVelocity = wishDir * moveSpeed;

        // Плавно интерполируем горизонтальную скорость
        float smoothTime = wishDir.sqrMagnitude > 0.01f ? acceleration : deceleration;
        _currentVelocityXZ = Vector3.SmoothDamp(
            _currentVelocityXZ, targetVelocity, ref _smoothDampVelocity, smoothTime);

        // Гравитация
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -4f;   // прижимаем к земле чтобы isGrounded не мигал

        _verticalVelocity += gravity * Time.deltaTime;

        // Итоговый вектор движения
        Vector3 move = _currentVelocityXZ;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);
    }

    // ─── Camera Wall Push ─────────────────────────────────────

    /// <summary>
    /// SphereCast от головы игрока к камере.
    /// Если по пути есть стена — двигаем камеру перед ней.
    /// </summary>
    private void PushCameraFromWalls()
    {
        // Мировая позиция базовой точки (голова игрока)
        Vector3 origin = transform.position + cameraBaseLocalOffset;

        // Желаемое положение камеры (без коллизии)
        Vector3 desiredPos = origin;  // first-person: камера прямо в голове

        // Для первого лица: камера УЖЕ в голове, клиппинг идёт от геометрии
        // которая вплотную к камере. Небольшой SphereCast назад (от головы
        // по look-direction) помогает при движении вплотную к стене.
        Vector3 lookDir = _cameraTransform.forward;
        float checkDist = 0.15f;

        // Проверяем есть ли стена прямо перед камерой (в 15 см)
        if (Physics.SphereCast(origin, cameraCollisionRadius,
                               lookDir, out RaycastHit hit,
                               checkDist, _wallMask,
                               QueryTriggerInteraction.Ignore))
        {
            // Отодвигаем камеру немного назад от стены
            float pushBack = checkDist - hit.distance + cameraCollisionRadius;
            desiredPos = origin - lookDir * pushBack;
        }

        _cameraTransform.position = desiredPos;
    }
}
