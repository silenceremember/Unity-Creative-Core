using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC pause menu.
///
/// Opens on ESC in all states except blocked ones (configured via PauseMenuConfig).
/// In VisualNovel mode ESC also works — allows returning to main menu,
/// aborting the novel via NovelChannel.RaiseAbort().
///
/// Time.timeScale = 0 while pause is open.
/// Volume slider is handled by VolumeSlider component — PauseMenuManager only syncs its value on open.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Root GameObject of the ESC menu")]
    [SerializeField] private GameObject pauseMenuCanvas;

    [Header("Audio")]
    [SerializeField] private AudioConfig audioConfig;
    [Tooltip("Volume slider inside the pause menu (managed by VolumeSlider component)")]
    [SerializeField] private Slider volumeSlider;

    [Header("TV")]
    [SerializeField] private TVController tvController;

    [Header("Config")]
    [SerializeField] private PauseMenuConfig config;

    [Header("GameState")]
    [SerializeField] private GameStateChannel gameStateChannel;

    [Header("Novel")]
    [SerializeField] private NovelChannel novelChannel;

    [Header("Dependencies")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private LevelUpCanvas levelUpCanvas;

    [Header("Shared State")]
    [Tooltip("BoolVariable SO — true while pause menu is open")]
    [SerializeField] private BoolVariable isPausedVariable;

    private bool      _isPaused          = false;
    private GameState _curState          = GameState.Menu;
    private bool      _playerWasEnabled  = false;

    void Awake()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
    }

    void OnEnable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        if (gameStateChannel != null)
            gameStateChannel.OnStateChanged -= OnStateChanged;
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (IsBlockedState(_curState))
            return;

        if (levelUpCanvas != null && levelUpCanvas.gameObject.activeSelf)
            return;

        TogglePause();
    }

    private void OnStateChanged(GameState state)
    {
        _curState = state;

        if (_isPaused && IsBlockedState(state))
        {
            ClosePause();
        }
    }

    /// <summary>Resume — just closes the menu.</summary>
    public void Resume()
    {
        ClosePause();
    }

    /// <summary>Go to main menu — reloads scene (index 0).</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SetPaused(false);
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Go to main menu from novel.
    /// Aborts the novel via NovelChannel, then loads scene 0.
    /// </summary>
    public void GoToMainMenuFromNovel()
    {
        novelChannel?.RaiseAbort();
        GoToMainMenu();
    }

    /// <summary>Full game quit.</summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>TV On button.</summary>
    public void TVOn()  => tvController?.TurnOn();
    /// <summary>TV Off button.</summary>
    public void TVOff() => tvController?.TurnOff();

    private void TogglePause()
    {
        if (_isPaused) ClosePause();
        else           OpenPause();
    }

    private void OpenPause()
    {
        _playerWasEnabled = playerController != null && playerController.enabled;

        SetPaused(true);
        Time.timeScale = 0f;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        if (volumeSlider != null && audioConfig != null)
            volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(AudioPrefsKeys.MasterVolume, audioConfig.DefaultVolume));

        if (playerController != null) playerController.enabled = false;
    }

    private void ClosePause()
    {
        SetPaused(false);
        Time.timeScale = 1f;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        if (playerController != null) playerController.enabled = _playerWasEnabled;
    }

    private bool IsBlockedState(GameState state)
    {
        if (config == null) return false;
        return Array.IndexOf(config.BlockedStates, state) >= 0;
    }

    private void SetPaused(bool value)
    {
        _isPaused = value;
        if (isPausedVariable != null)
            isPausedVariable.Value = value;
    }
}
