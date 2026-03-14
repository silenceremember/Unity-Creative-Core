using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// XP/Level Manager (UniTask).
/// - Уровень повышается АВТОМАТИЧЕСКИ при заполнении полоски.
/// - Текст Lv.N меняется с анимацией (punch scale + flash).
/// - "▲ Level Up! [X]" мигает — X откроет канвас апгрейдов.
/// - Оверфлоу XP автоматически переходит в следующий уровень.
/// - НАГРАДА убывает непрерывно через первый и второй проход до 0, после чего TMP скрывается.
/// </summary>
public class XPLevelManager : MonoBehaviour
{
    public static XPLevelManager Instance { get; private set; }

    [Header("Данные")]
    [Tooltip("XP для каждого уровня. [0] = Lv.0→1, [1] = Lv.1→2 и т.д.")]
    public int[] xpRequirements = { 500, 750, 1000 };
    [Tooltip("Длительность анимации заполнения (сек)")]
    public float fillDuration = 4f;
    [Tooltip("Кривая заполнения. По умолчанию: медленный старт → разгон → плавная остановка")]
    public AnimationCurve fillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("UI — LevelBar")]
    public GameObject      levelBar;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI xpLabel;
    public Slider          xpSlider;
    [Tooltip("Прямая ссылка на Image полоски fill — назначь вручную в Inspector")]
    public Image           fillImage;
    public GameObject      levelUpPrompt;   // "▲ Level Up! [X]"
    [Tooltip("Канвас апгрейдов — назначь LevelUpCanvas компонент из сцены")]
    public LevelUpCanvas   upgradeCanvas;

    [Header("UI — Награда")]
    public TextMeshProUGUI rewardLabel;
    public string          rewardFormat = "НАГРАДА: {0} XP";
    [Tooltip("Отображаемая награда (XP) — отдельно от xpRequirements!")]
    public int             questRewardXP = 1000;

    [Header("Стиль")]
    public Color flashColor      = new Color(1f, 0.85f, 0f, 1f);   // золотой
    public float levelLabelPunch = 1.35f;
    public float punchDuration   = 0.35f;

    [Header("Нарратор")]
    [Tooltip("Канал рассказчика")]
    public NarratorChannel narratorChannel;
    [Tooltip("Реплика при активации XP-бара")]
    public DialogueSequence seqXPBarActivated;
    [Tooltip("Реплика после level-up (до промпта X)")]
    public DialogueSequence seqLevelUp;
    [Tooltip("Реплика когда X нажат — показывается канвас апгрейдов")]
    public DialogueSequence seqAbilityChosen;
    [Tooltip("Реплика после попытки опробовать способность (Ctrl/Shift/Space)")]
    public DialogueSequence seqAbilityTried;
    [Tooltip("Реплика об открытии нового уровня + исчезновении двери")]
    public DialogueSequence seqDoorUnlocked;

    [Header("Дверь")]
    [Tooltip("Объект двери — отключается при нарративе seqDoorUnlocked")]
    public GameObject doorObject;

    [Header("Reject анимация")]
    [Tooltip("Амплитуда встряски подсказки X (пиксели)")]
    public float promptShakeMagnitude = 10f;
    [Tooltip("Длительность встряски подсказки X (сек)")]
    public float promptShakeDuration  = 0.35f;

    // ── State ─────────────────────────────────────────────────────────────

    private int   _level         = 0;
    private int   _currentXP     = 0;
    private float _displayXP     = 0f;
    private bool  _promptVisible = false;
    private Color _fillOriginalColor;
    private bool  _xpBarNarrPlayed = false;
    private bool  _promptShaking   = false;

    private const float InputSpamCooldown = 0.5f;
    private float _xBlockedUntil = 0f;

    // ── UniTask CTS ────────────────────────────────────────────────────────

    /// <summary>Управляет цепочкой TransferAnimation → AutoLevelUp.</summary>
    private CancellationTokenSource _animCts;
    /// <summary>Управляет BlinkUpgradePrompt (отдельно от анимации).</summary>
    private CancellationTokenSource _blinkCts;

    private int XPForCurrentLevel =>
        _level < xpRequirements.Length
            ? xpRequirements[_level]
            : xpRequirements[xpRequirements.Length - 1];

    void Awake() => Instance = this;

    void Start()
    {
        if (levelBar      != null) levelBar.SetActive(false);
        if (levelUpPrompt != null) levelUpPrompt.SetActive(false);
        InitSlider();
        if (fillImage != null) _fillOriginalColor = fillImage.color;

        if (rewardLabel != null)
        {
            rewardLabel.gameObject.SetActive(true);
            rewardLabel.text = string.Format(rewardFormat, questRewardXP);
        }

        RefreshUI();
        LevelUpCanvas.OnAbilityChosen += HandleAbilityChosen;
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
    }

    void OnDestroy()
    {
        LevelUpCanvas.OnAbilityChosen -= HandleAbilityChosen;
        CancelAnim();
        _blinkCts?.Cancel(); _blinkCts?.Dispose();
    }

    // ── Narrator events ───────────────────────────────────────────────────

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        // seqXPBarActivated завершился → запускаем seqLevelUp
        if (completed == seqXPBarActivated)
        {
            if (seqLevelUp != null)
                narratorChannel?.Raise(seqLevelUp);
            return;
        }

        // seqLevelUp завершился → показываем промпт X
        if (completed == seqLevelUp)
        {
            ShowUpgradePrompt();
            return;
        }

        // seqDoorUnlocked завершился → гасим дверь
        if (completed == seqDoorUnlocked)
        {
            if (doorObject != null)
            {
                doorObject.SetActive(false);
                Debug.Log("[XPLevelManager] Door deactivated!");
            }
        }
    }

    // ── Input ─────────────────────────────────────────────────────────────

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (!_promptVisible || kb == null || !kb.xKey.wasPressedThisFrame) return;
        if (PauseMenuManager.IsPaused) return;

        bool triggerDialogue = ExplorationManager.Instance != null &&
                               ExplorationManager.Instance.TriggerDialoguePlaying;
        bool narratorActive  = NarratorManager.Instance != null &&
                               NarratorManager.Instance.IsPlaying;

        if (triggerDialogue || narratorActive)
        {
            Debug.Log("[XPLevelManager] X заблокирован: идёт диалог.");
            if (!_promptShaking && levelUpPrompt != null)
                ShakePrompt(this.GetCancellationTokenOnDestroy()).Forget();
        }
        else
        {
            _xBlockedUntil = Time.unscaledTime + InputSpamCooldown;
            OnUpgradeKeyPressed();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void AddXP(int amount)
    {
        // Не запускаем если анимация уже идёт
        if (_animCts != null && !_animCts.IsCancellationRequested) return;

        if (levelBar != null) levelBar.SetActive(true);

        // Нарратив при первом появлении XP-бара
        if (!_xpBarNarrPlayed && seqXPBarActivated != null)
        {
            _xpBarNarrPlayed = true;
            narratorChannel?.Raise(seqXPBarActivated);
        }

        // Показываем награду
        if (rewardLabel != null)
        {
            rewardLabel.gameObject.SetActive(true);
            rewardLabel.text = string.Format(rewardFormat, amount);
        }

        _animCts = new CancellationTokenSource();
        // rewardDisplay = сколько награды ещё нужно «съесть» в UI (убывает до 0)
        TransferAnimation(amount, rewardDisplay: amount, _animCts.Token).Forget();
    }

    // ── Transfer Animation ────────────────────────────────────────────────
    //
    // xpToAdd      — XP добавляемый в текущем проходе
    // rewardDisplay — остаток награды который нужно «списать» в UI в этом проходе
    //                 (при overflow передаём остаток, чтобы он дошёл до 0)

    private async UniTask TransferAnimation(int xpToAdd, int rewardDisplay, CancellationToken ct)
    {
        try
        {
            int cap     = XPForCurrentLevel;
            int canFill = cap - _currentXP;
            int fill    = Mathf.Min(xpToAdd, canFill);
            int overflow = xpToAdd - fill;
            int endXP   = _currentXP + fill;

            // Сколько награды «уходит» в этом проходе (пропорционально fill)
            // rewardEnd — остаток, который передадим дальше
            int rewardEnd = rewardDisplay - fill;
            // Защита: не даём уйти ниже нуля раньше времени (если fill > rewardDisplay)
            rewardEnd = Mathf.Max(rewardEnd, 0);

            float startXP = _currentXP;
            float dur     = fillDuration * Mathf.Max((float)fill / cap, 0.2f);
            float elapsed = 0f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / dur);
                float curve = fillCurve != null && fillCurve.length > 0
                    ? fillCurve.Evaluate(t)
                    : Mathf.SmoothStep(0f, 1f, t);

                _displayXP = Mathf.Lerp(startXP, endXP, curve);
                if (xpSlider != null) xpSlider.value = _displayXP;
                if (xpLabel  != null) xpLabel.text   = $"{Mathf.RoundToInt(_displayXP)} / {cap}";

                // Награда убывает от rewardDisplay → rewardEnd плавно
                if (rewardLabel != null && rewardLabel.gameObject.activeSelf)
                {
                    int rewardLeft = Mathf.RoundToInt(Mathf.Lerp(rewardDisplay, rewardEnd, curve));
                    rewardLabel.text = string.Format(rewardFormat, rewardLeft);
                    if (rewardLeft <= 0)
                        rewardLabel.gameObject.SetActive(false);
                }

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // Финальные значения
            _currentXP = endXP;
            _displayXP = endXP;
            if (xpSlider != null) xpSlider.value = endXP;
            if (xpLabel  != null) xpLabel.text   = $"{endXP} / {cap}";

            if (rewardLabel != null && rewardLabel.gameObject.activeSelf)
            {
                rewardLabel.text = string.Format(rewardFormat, rewardEnd);
                if (rewardEnd <= 0)
                    rewardLabel.gameObject.SetActive(false);
            }

            if (_currentXP >= cap)
                await AutoLevelUp(overflow, rewardEnd, ct);
            else
                FinishAnim();
        }
        catch (System.OperationCanceledException)
        {
            // нормальная отмена
        }
    }

    // ── Auto Level Up ─────────────────────────────────────────────────────

    private async UniTask AutoLevelUp(int overflow, int rewardLeft, CancellationToken ct)
    {
        try
        {
            // Мигание полоски перед сбросом
            await FlashFill(times: 3, interval: 0.12f, ct);

            _level++;
            _currentXP = 0;
            _displayXP = 0f;

            // Анимированная смена текста уровня (отдельный CT — не завязан на _animCts)
            PunchLevelLabel(this.GetCancellationTokenOnDestroy()).Forget();

            ResetFillColor();
            InitSlider();
            RefreshUI();

            // seqLevelUp запустится через OnNarratorCompleted(seqXPBarActivated→seqLevelUp),
            // промпт X — через OnNarratorCompleted(seqLevelUp).

            // Если есть оверфлоу — заливаем остаток (rewardLeft продолжает убывать)
            if (overflow > 0)
            {
                await UniTask.Delay(200, cancellationToken: ct);
                await TransferAnimation(overflow, rewardLeft, ct);
            }
            else
            {
                FinishAnim();
            }
        }
        catch (System.OperationCanceledException)
        {
            // нормальная отмена
        }
    }

    // ── PunchLevelLabel ───────────────────────────────────────────────────

    private async UniTask PunchLevelLabel(CancellationToken ct)
    {
        if (levelLabel == null) return;
        var rt    = levelLabel.GetComponent<RectTransform>();
        var canvg = levelLabel.GetComponent<CanvasRenderer>();

        levelLabel.text = $"Lv.{_level}";

        float elapsed = 0f;
        try
        {
            while (elapsed < punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / punchDuration;
                float scale = 1f + (levelLabelPunch - 1f) * Mathf.Sin(t * Mathf.PI);
                if (rt    != null) rt.localScale = Vector3.one * scale;
                if (canvg != null) canvg.SetAlpha(Mathf.PingPong(t * 6f, 1f));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        if (rt    != null) rt.localScale = Vector3.one;
        if (canvg != null) canvg.SetAlpha(1f);
    }

    // ── Upgrade Prompt (X) ────────────────────────────────────────────────

    private void ShowUpgradePrompt()
    {
        _promptVisible = true;
        if (levelUpPrompt != null) levelUpPrompt.SetActive(true);

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
        _blinkCts = new CancellationTokenSource();
        BlinkUpgradePrompt(_blinkCts.Token).Forget();
    }

    private void OnUpgradeKeyPressed()
    {
        _promptVisible = false;
        if (levelUpPrompt != null) levelUpPrompt.SetActive(false);

        _blinkCts?.Cancel();
        _blinkCts?.Dispose();
        _blinkCts = null;

        if (seqAbilityChosen != null)
            narratorChannel?.Raise(seqAbilityChosen);

        if (upgradeCanvas != null)
            upgradeCanvas.Show();
        else
            Debug.LogWarning("[XPLevelManager] upgradeCanvas не назначен в Inspector!");
    }

    private async UniTask BlinkUpgradePrompt(CancellationToken ct)
    {
        if (levelUpPrompt == null) return;
        var tmpText = levelUpPrompt.GetComponentInChildren<TextMeshProUGUI>();
        var rt      = levelUpPrompt.GetComponent<RectTransform>();
        Image fillImg = fillImage;

        try
        {
            while (true)
            {
                float t = (Mathf.Sin(Time.time * Mathf.PI * 2f) + 1f) * 0.5f;
                if (tmpText != null) tmpText.alpha = Mathf.Lerp(0.3f, 1f, t);
                if (rt      != null) rt.localScale = Vector3.one * Mathf.Lerp(1f, 1.06f, t);
                if (fillImg != null) fillImg.color = Color.Lerp(_fillOriginalColor, flashColor, t);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        if (tmpText != null) tmpText.alpha = 1f;
        if (rt      != null) rt.localScale = Vector3.one;
        if (fillImg != null) fillImg.color = _fillOriginalColor;
    }

    // ── Ability Selection & Trial ─────────────────────────────────────────

    private void HandleAbilityChosen()
    {
        Debug.Log("[XPLevelManager] Ability chosen — starting seqAbilityTried.");
        OnAbilityTried();
    }

    private void OnAbilityTried()
    {
        narratorChannel?.Stop();
        if (seqAbilityTried != null)
            narratorChannel?.Raise(seqAbilityTried);
        else if (seqDoorUnlocked != null)
            narratorChannel?.Raise(seqDoorUnlocked);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async UniTask ShakePrompt(CancellationToken ct)
    {
        if (levelUpPrompt == null) return;
        _promptShaking = true;

        var rt = levelUpPrompt.GetComponent<RectTransform>();
        if (rt == null) { _promptShaking = false; return; }

        var txt = levelUpPrompt.GetComponentInChildren<TextMeshProUGUI>();
        Color originalColor = txt != null ? txt.color : Color.white;
        if (txt != null) txt.color = Color.red;

        Vector2 origin  = rt.anchoredPosition;
        float   elapsed = 0f;

        try
        {
            while (elapsed < promptShakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-promptShakeMagnitude, promptShakeMagnitude) *
                          (1f - elapsed / promptShakeDuration);
                rt.anchoredPosition = origin + new Vector2(x, 0f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (System.OperationCanceledException) { }

        rt.anchoredPosition = origin;
        if (txt != null) txt.color = originalColor;
        _promptShaking = false;
    }

    private async UniTask FlashFill(int times, float interval, CancellationToken ct)
    {
        if (fillImage == null) return;
        int ms = Mathf.RoundToInt(interval * 1000f);
        for (int i = 0; i < times; i++)
        {
            fillImage.color = flashColor;
            await UniTask.Delay(ms, cancellationToken: ct);
            fillImage.color = _fillOriginalColor;
            await UniTask.Delay(ms, cancellationToken: ct);
        }
    }

    private void FinishAnim()
    {
        _animCts?.Dispose();
        _animCts = null;
    }

    private void CancelAnim()
    {
        _animCts?.Cancel();
        _animCts?.Dispose();
        _animCts = null;
    }

    private void InitSlider()
    {
        if (xpSlider == null) return;
        xpSlider.minValue     = 0;
        xpSlider.maxValue     = XPForCurrentLevel;
        xpSlider.value        = _displayXP;
        xpSlider.interactable = false;
        if (fillImage != null) _fillOriginalColor = fillImage.color;
    }

    private void RefreshUI()
    {
        if (levelLabel != null) levelLabel.text = $"Lv.{_level}";
        if (xpLabel    != null) xpLabel.text    = $"{_currentXP} / {XPForCurrentLevel}";
    }

    private void ResetFillColor()
    {
        if (fillImage != null) fillImage.color = _fillOriginalColor;
    }
}
