using UnityEngine;

/// <summary>
/// Слушает GameStateChannel и при переходе в Gameplay:
///   1. Репарентирует Main Camera под Player (только один раз при входе)
///   2. Скрывает MeshRenderer капсулы игрока
///   3. Добавляет CharacterController если его нет и активирует PlayerController
///
/// При выходе из Gameplay:
///   — PlayerController деактивируется
/// </summary>
public class GameplaySetup : MonoBehaviour
{
    [Header("Channels")]
    public GameStateChannel gameStateChannel;

    [Header("References")]
    [Tooltip("Transform игрока")]
    public Transform playerTransform;

    [Tooltip("Main Camera (Character Camera)")]
    public Camera mainCamera;

    [Tooltip("Смещение камеры от пивота игрока (высота глаз)")]
    public Vector3 cameraLocalOffset = new Vector3(0f, 0.7f, 0f);

    // ─── Private ─────────────────────────────────────────────
    private PlayerController _playerController;
    private MeshRenderer _capsuleRenderer;
    private bool _initialized = false;

    // ─────────────────────────────────────────────────────────

    void Awake()
    {
        if (playerTransform != null)
        {
            _capsuleRenderer = playerTransform.GetComponent<MeshRenderer>();

            // Получить или добавить PlayerController
            _playerController = playerTransform.GetComponent<PlayerController>();
            if (_playerController == null)
                _playerController = playerTransform.gameObject.AddComponent<PlayerController>();

            // Отключаем сразу — включим при входе в Gameplay
            _playerController.enabled = false;
        }
    }

    void OnEnable()
    {
        if (gameStateChannel != null)
        {
            gameStateChannel.OnStateChanged += OnStateChanged;
            // Применяем текущее состояние сразу
            OnStateChanged(gameStateChannel.Current);
        }
    }

    void OnDisable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Gameplay || state == GameState.Quest)
            EnterGameplay();
        else
            ExitGameplay();
    }

    private void EnterGameplay()
    {
        if (!_initialized)
        {
            _initialized = true;

            // 1. Скрыть модель капсулы
            if (_capsuleRenderer != null)
                _capsuleRenderer.enabled = false;

            // 2. Репарентировать камеру под игрока
            if (mainCamera != null && playerTransform != null)
            {
                mainCamera.transform.SetParent(playerTransform, worldPositionStays: false);
                // localPosition = zero: PlayerController.PushCameraFromWalls
                // сам позиционирует камеру через cameraBaseLocalOffset
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localRotation = Quaternion.identity;
            }
        }

        // 3. Включить PlayerController и передать ссылку на камеру (всегда при входе в активные стейты)
        if (_playerController != null)
        {
            // Передаём offset из Inspector в контроллер
            _playerController.cameraBaseLocalOffset = cameraLocalOffset;
            _playerController.enabled = true;
            _playerController.Init(mainCamera != null ? mainCamera.transform : null);
        }

        Debug.Log("[GameplaySetup] Active state entered: player controller enabled.");
    }

    private void ExitGameplay()
    {
        // Отключаем управление при переходе в VisualNovel/Menu/IntroCrawl
        if (_playerController != null)
            _playerController.enabled = false;
    }
}
