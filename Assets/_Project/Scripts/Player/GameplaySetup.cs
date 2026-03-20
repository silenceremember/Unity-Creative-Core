using UnityEngine;

/// <summary>
/// Listens to GameStateChannel and on transition to Gameplay:
///   1. Reparents Main Camera under Player (only once on first entry)
///   2. Hides the player capsule MeshRenderer
///   3. Adds CharacterController if missing and activates PlayerController
///
/// On exit from Gameplay:
///   — PlayerController is deactivated
/// </summary>
public class GameplaySetup : MonoBehaviour
{
    [Header("Channels")]
    [SerializeField] private GameStateChannel gameStateChannel;

    [Header("References")]
    [Tooltip("Player transform")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Main Camera (Character Camera)")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Camera offset from player pivot (eye height)")]
    [SerializeField] private Vector3 cameraLocalOffset = new Vector3(0f, 0.7f, 0f);

    private PlayerController _playerController;
    private MeshRenderer _capsuleRenderer;
    private bool _initialized = false;

    void Awake()
    {
        if (playerTransform != null)
        {
            _capsuleRenderer = playerTransform.GetComponent<MeshRenderer>();

            _playerController = playerTransform.GetComponent<PlayerController>();
            if (_playerController == null)
                _playerController = playerTransform.gameObject.AddComponent<PlayerController>();

            _playerController.enabled = false;
        }
    }

    void OnEnable()
    {
        if (gameStateChannel != null)
        {
            gameStateChannel.OnStateChanged += OnStateChanged;
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

            if (_capsuleRenderer != null)
                _capsuleRenderer.enabled = false;

            if (mainCamera != null && playerTransform != null)
            {
                mainCamera.transform.SetParent(playerTransform, worldPositionStays: false);
                mainCamera.transform.localPosition = Vector3.zero;
                mainCamera.transform.localRotation = Quaternion.identity;
            }
        }

        if (_playerController != null)
        {
            _playerController.SetCameraBaseOffset(cameraLocalOffset);
            _playerController.enabled = true;
            _playerController.Init(mainCamera != null ? mainCamera.transform : null);
        }
    }

    private void ExitGameplay()
    {
        if (_playerController != null)
            _playerController.enabled = false;
    }
}
