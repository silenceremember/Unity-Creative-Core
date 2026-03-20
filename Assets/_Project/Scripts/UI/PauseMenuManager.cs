using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC pause menu.
///
/// Opens on ESC in all states except Menu, IntroCrawl, and Final.
/// In VisualNovel mode ESC also works — allows returning to main menu,
/// aborting the novel via NovelChannel.RaiseAbort().
///
/// Time.timeScale = 0 while pause is open.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Root GameObject of the ESC menu")]
    [SerializeField] private GameObject pauseMenuCanvas;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    [Tooltip("Exposed parameter name")]
    [SerializeField] private string exposedParam = "Master";
    [Tooltip("Volume slider inside the pause menu")]
    [SerializeField] private Slider volumeSlider;

    [Header("TV")]
    [SerializeField] private TVController tvController;

    [Header("Portfolio")]
    [Tooltip("Portfolio URL")]
    [SerializeField] private string portfolioUrl = "https://example.com";

    [Header("GameState")]
    [SerializeField] private GameStateChannel gameStateChannel;

    [Header("Novel")]
    [SerializeField] private NovelChannel novelChannel;

    private bool      _isPaused   = false;
    private GameState _curState   = GameState.Menu;

    /// <summary>True while pause menu is open — used by other systems to block input.</summary>
    public static bool IsPaused => Instance != null && Instance._isPaused;

    private const string PREFS_KEY = "MasterVolumeV2";

    void Awake()
    {
        Instance = this;
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

    void Start()
    {
        if (volumeSlider != null)
        {
            float saved = PlayerPrefs.GetFloat(PREFS_KEY, 0.25f);
            volumeSlider.value = saved;
            volumeSlider.onValueChanged.AddListener(ApplyVolume);
        }
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

        if (_curState == GameState.Menu       ||
            _curState == GameState.IntroCrawl ||
            _curState == GameState.Final)
            return;

        if (LevelUpCanvas.Instance != null &&
            LevelUpCanvas.Instance.gameObject.activeSelf)
            return;

        TogglePause();
    }

    private void OnStateChanged(GameState state)
    {
        _curState = state;

        if (_isPaused &&
            (state == GameState.Final      ||
             state == GameState.Menu       ||
             state == GameState.IntroCrawl))
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
        _isPaused = false;
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

    /// <summary>Applies slider volume to AudioMixer.</summary>
    public void ApplyVolume(float value)
    {
        if (audioMixer == null) return;

        float dB = Mathf.Lerp(10f, -30f, value);
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(PREFS_KEY, value);
    }

    /// <summary>TV On button.</summary>
    public void TVOn()  => tvController?.TurnOn();
    /// <summary>TV Off button.</summary>
    public void TVOff() => tvController?.TurnOff();

    /// <summary>Portfolio image button: opens URL in browser.</summary>
    public void OpenPortfolio()
    {
        if (!string.IsNullOrEmpty(portfolioUrl))
            Application.OpenURL(portfolioUrl);
    }

    private void TogglePause()
    {
        if (_isPaused) ClosePause();
        else           OpenPause();
    }

    private void OpenPause()
    {
        _isPaused      = true;
        Time.timeScale = 0f;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PREFS_KEY, 0.25f));

        if (PlayerController.Instance != null && PlayerController.Instance.enabled)
            PlayerController.Instance.enabled = false;
    }

    private void ClosePause()
    {
        _isPaused      = false;
        Time.timeScale = 1f;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Re-enable PlayerController only in Gameplay/Quest, NOT in VisualNovel
        if (_curState != GameState.VisualNovel &&
            PlayerController.Instance != null && !PlayerController.Instance.enabled)
            PlayerController.Instance.enabled = true;
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
