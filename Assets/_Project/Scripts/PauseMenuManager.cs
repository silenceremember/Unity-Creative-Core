using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ESC-меню паузы.
///
/// Открывается по ESC только в состояниях Gameplay / Quest
/// (не работает в Menu, IntroCrawl, VisualNovel, Final,
///  а также когда открыт LevelUpCanvas).
///
/// Time.timeScale = 0 пока пауза открыта.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    public static PauseMenuManager Instance { get; private set; }

    // ─── References ──────────────────────────────────────────────────────────

    [Header("UI")]
    [Tooltip("Корневой GameObject ESC-меню (начально выключен)")]
    public GameObject pauseMenuCanvas;

    [Header("Audio")]
    [Tooltip("AudioMixer для управления громкостью")]
    public AudioMixer audioMixer;

    [Tooltip("Имя exposed parameter (тот же что в SettingsMenuCanvas)")]
    public string exposedParam = "Master";

    [Tooltip("Слайдер громкости внутри меню паузы")]
    public Slider volumeSlider;

    [Header("TV")]
    [Tooltip("TVController в сцене — для кнопки TV on/off")]
    public TVController tvController;

    [Header("Portfolio")]
    [Tooltip("URL портфолио — открывается в браузере")]
    public string portfolioUrl = "https://example.com";

    [Header("GameState")]
    public GameStateChannel gameStateChannel;

    // ─── Private ─────────────────────────────────────────────────────────────

    private bool      _isPaused   = false;
    private GameState _curState   = GameState.Menu;

    /// <summary>True пока меню паузы открыто — используется другими системами для блокировки ввода.</summary>
    public static bool IsPaused => Instance != null && Instance._isPaused;

    private const string PREFS_KEY = "MasterVolumeV2";

    // ─────────────────────────────────────────────────────────────────────────

    void Awake()
    {
        Instance = this;

        // Гарантируем что меню скрыто при старте
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
        // Инициализируем слайдер сохранённым значением
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

        // Запрещённые состояния: меню, кроул, новелла, финал
        if (_curState == GameState.Menu        ||
            _curState == GameState.IntroCrawl  ||
            _curState == GameState.VisualNovel ||
            _curState == GameState.Final)
            return;

        // Запрещено когда открыт LevelUpCanvas
        if (LevelUpCanvas.Instance != null &&
            LevelUpCanvas.Instance.gameObject.activeSelf)
            return;

        TogglePause();
    }

    // ── GameState tracking ───────────────────────────────────────────────────

    private void OnStateChanged(GameState state)
    {
        _curState = state;

        // Если вошли в Final/Menu — принудительно закрываем паузу
        if (_isPaused &&
            (state == GameState.Final      ||
             state == GameState.Menu       ||
             state == GameState.IntroCrawl ||
             state == GameState.VisualNovel))
        {
            ClosePause();
        }
    }

    // ── Public API (привязываются к кнопкам) ────────────────────────────────

    /// <summary>Продолжить — просто закрывает меню.</summary>
    public void Resume()
    {
        ClosePause();
    }

    /// <summary>Выйти в главное меню — перезагружает сцену (index 0).</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;

        SceneManager.LoadScene(0);
    }

    /// <summary>Полный выход из игры.</summary>
    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>Применяет громкость слайдера к AudioMixer.</summary>
    public void ApplyVolume(float value)
    {
        if (audioMixer == null) return;

        // Та же формула что в VolumeSlider.cs: 0 → +10 dB, 0.25 → 0 dB, 1 → -30 dB
        float dB = Mathf.Lerp(10f, -30f, value);
        audioMixer.SetFloat(exposedParam, dB);

        PlayerPrefs.SetFloat(PREFS_KEY, value);
    }

    /// <summary>Кнопка TV On.</summary>
    public void TVOn()  => tvController?.TurnOn();

    /// <summary>Кнопка TV Off.</summary>
    public void TVOff() => tvController?.TurnOff();

    /// <summary>Кнопка-картинка: открывает URL портфолио в браузере.</summary>
    public void OpenPortfolio()
    {
        if (!string.IsNullOrEmpty(portfolioUrl))
            Application.OpenURL(portfolioUrl);
    }

    // ── Internal ─────────────────────────────────────────────────────────────

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

        // Синхронизируем слайдер с текущим значением громкости (мог измениться в Settings)
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PREFS_KEY, 0.25f));

        // Отключаем PlayerController — блокирует поворот камеры, движение, рейкасты.
        // OnDisable() PlayerController'а автоматически освобождает курсор.
        if (PlayerController.Instance != null && PlayerController.Instance.enabled)
            PlayerController.Instance.enabled = false;

        Debug.Log("[PauseMenuManager] Paused.");
    }

    private void ClosePause()
    {
        _isPaused      = false;
        Time.timeScale = 1f;

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Включаем PlayerController обратно — его OnEnable() автоматически
        // блокирует курсор (Locked/invisible) для gameplay.
        if (PlayerController.Instance != null && !PlayerController.Instance.enabled)
            PlayerController.Instance.enabled = true;

        Debug.Log("[PauseMenuManager] Resumed.");
    }

    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
