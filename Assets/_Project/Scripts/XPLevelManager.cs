using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// XP/Level Manager.
/// - Уровень повышается АВТОМАТИЧЕСКИ при заполнении полоски (без ожидания X).
/// - Текст Lv.N меняется с анимацией (punch scale + flash).
/// - "▲ Level Up! [X]" мигает — X откроет канвас апгрейдов (пока просто скрывает).
/// - Оверфлоу XP автоматически переходит в следующий уровень.
/// - После level-up → рассказчик ведёт игрока по нарративу до исчезновения двери.
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
    public float levelLabelPunch = 1.35f;   // масштаб при смене уровня
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

    // ── State ─────────────────────────────────────────────────────────────

    private int   _level           = 0;
    private int   _currentXP       = 0;
    private float _displayXP       = 0f;
    private bool  _animating       = false;
    private bool  _promptVisible   = false;   // мигалка апгрейда
    private Color _fillOriginalColor;

    // Нарратив-состояние
    private bool  _xpBarNarrPlayed = false;   // играли ли нарратив XP-бара

    // Антиспам: кулдаун после диалога перед X
    private float _xBlockedUntil      = 0f;
    private const float InputSpamCooldown = 0.5f;

    [Header("Reject анимация")]
    [Tooltip("Амплитуда встряски подсказки X (пиксели)")]
    public float promptShakeMagnitude = 10f;
    [Tooltip("Длительность встряски подсказки X (сек)")]
    public float promptShakeDuration  = 0.35f;
    private bool _promptShaking = false;

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
        if (fillImage   != null) _fillOriginalColor = fillImage.color;
        if (rewardLabel != null)
            rewardLabel.text = string.Format(rewardFormat, questRewardXP);
        RefreshUI();

        // Подписываемся на событие выбора способности
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
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
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

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        bool dialoguePlaying = NarratorManager.Instance != null && NarratorManager.Instance.IsPlaying;

        // X — открыть канвас апгрейдов
        // Блокируем с reject-эффектом ТОЛЬКО во время диалога триггера A/B
        if (_promptVisible && kb != null && kb.xKey.wasPressedThisFrame)
        {
            bool triggerDialogue = ExplorationManager.Instance != null &&
                                   ExplorationManager.Instance.TriggerDialoguePlaying;
            if (triggerDialogue)
            {
                Debug.Log("[XPLevelManager] X заблокирован: идёт диалог триггера A/B.");
                if (!_promptShaking && levelUpPrompt != null)
                    StartCoroutine(ShakePrompt());
            }
            else
            {
                _xBlockedUntil = Time.unscaledTime + InputSpamCooldown;
                OnUpgradeKeyPressed();
            }
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void AddXP(int amount)
    {
        if (_animating) return;
        if (levelBar != null) levelBar.SetActive(true);

        // Нарратив при первом появлении XP-бара
        if (!_xpBarNarrPlayed && seqXPBarActivated != null)
        {
            _xpBarNarrPlayed = true;
            narratorChannel?.Raise(seqXPBarActivated);
        }

        // Сразу показываем реальную сумму награды
        if (rewardLabel != null)
            rewardLabel.text = string.Format(rewardFormat, amount);
        StopAllCoroutines();
        StartCoroutine(TransferAnimation(amount, isOverflow: false));
    }

    // ── Transfer Animation ─────────────────────────────────────────────────

    private IEnumerator TransferAnimation(int xpToAdd, bool isOverflow)
    {
        _animating = true;

        int   cap      = XPForCurrentLevel;
        int   canFill  = cap - _currentXP;
        int   fill     = Mathf.Min(xpToAdd, canFill);
        int   overflow = xpToAdd - fill;
        int   endXP    = _currentXP + fill;

        float startXP    = _currentXP;
        float dur        = fillDuration * Mathf.Max((float)fill / cap, 0.2f);
        float elapsed    = 0f;

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

            // Награда убывает только при первом проходе
            if (!isOverflow && rewardLabel != null)
            {
                int rewardLeft = Mathf.RoundToInt(Mathf.Lerp(xpToAdd, overflow, curve));
                rewardLabel.text = string.Format(rewardFormat, rewardLeft);
            }

            yield return null;
        }

        // Фиксируем финал
        _currentXP = endXP;
        _displayXP = endXP;
        if (xpSlider != null) xpSlider.value = endXP;
        if (xpLabel  != null) xpLabel.text   = $"{endXP} / {cap}";
        if (!isOverflow && rewardLabel != null)
            rewardLabel.text = string.Format(rewardFormat, overflow);

        _animating = false;

        if (_currentXP >= cap)
        {
            // Автоматически повышаем уровень
            yield return StartCoroutine(AutoLevelUp(overflow));
        }
    }

    // ── Auto Level Up ─────────────────────────────────────────────────────

    private IEnumerator AutoLevelUp(int overflow)
    {
        // Мигание полоски перед сбросом
        yield return StartCoroutine(FlashFill(times: 3, interval: 0.12f));

        _level++;
        _currentXP = 0;
        _displayXP = 0f;

        // Анимированная смена текста уровня
        StartCoroutine(PunchLevelLabel());

        // Сброс слайдера на новый уровень
        ResetFillColor();
        InitSlider();
        RefreshUI();

        // Нарратив при повышении уровня
        if (seqLevelUp != null)
            narratorChannel?.Raise(seqLevelUp);

        // Показываем мигалку апгрейда
        ShowUpgradePrompt();

        // Если есть оверфлоу — заливаем остаток
        if (overflow > 0)
        {
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(TransferAnimation(overflow, isOverflow: true));
        }
    }

    // ── Level Label Punch ─────────────────────────────────────────────────

    private IEnumerator PunchLevelLabel()
    {
        if (levelLabel == null) yield break;
        var rt    = levelLabel.GetComponent<RectTransform>();
        var canvg = levelLabel.GetComponent<CanvasRenderer>();

        // Обновляем текст сразу
        levelLabel.text = $"Lv.{_level}";

        // Punch scale
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            // 0→peak→1 через sin
            float scale = 1f + (levelLabelPunch - 1f) * Mathf.Sin(t * Mathf.PI);
            if (rt != null) rt.localScale = Vector3.one * scale;
            // Flash alpha
            if (canvg != null) canvg.SetAlpha(Mathf.PingPong(t * 6f, 1f));
            yield return null;
        }

        if (rt    != null) rt.localScale = Vector3.one;
        if (canvg != null) canvg.SetAlpha(1f);
    }

    // ── Upgrade Prompt (X) ────────────────────────────────────────────────

    private void ShowUpgradePrompt()
    {
        _promptVisible = true;
        if (levelUpPrompt != null) levelUpPrompt.SetActive(true);
        StartCoroutine(BlinkUpgradePrompt());
    }

    private void OnUpgradeKeyPressed()
    {
        _promptVisible = false;
        if (levelUpPrompt != null) levelUpPrompt.SetActive(false);

        // Нарратив «ого, новые способности»
        if (seqAbilityChosen != null)
            narratorChannel?.Raise(seqAbilityChosen);

        if (upgradeCanvas != null)
            upgradeCanvas.Show();
        else
            Debug.LogWarning("[XPLevelManager] upgradeCanvas не назначен в Inspector!");
    }

    private IEnumerator BlinkUpgradePrompt()
    {
        if (levelUpPrompt == null) yield break;
        var tmpText = levelUpPrompt.GetComponentInChildren<TextMeshProUGUI>();
        var rt      = levelUpPrompt.GetComponent<RectTransform>();
        Image fillImg = fillImage;

        while (_promptVisible)
        {
            float t   = (Mathf.Sin(Time.time * Mathf.PI * 2f) + 1f) * 0.5f;
            if (tmpText != null) tmpText.alpha = Mathf.Lerp(0.3f, 1f, t);
            if (rt      != null) rt.localScale = Vector3.one * Mathf.Lerp(1f, 1.06f, t);
            if (fillImg != null) fillImg.color = Color.Lerp(_fillOriginalColor, flashColor, t);
            yield return null;
        }

        if (tmpText != null) tmpText.alpha  = 1f;
        if (rt      != null) rt.localScale  = Vector3.one;
        if (fillImg != null) fillImg.color  = _fillOriginalColor;
    }

    // ── Ability Selection & Trial ─────────────────────────────────────────

    private void HandleAbilityChosen()
    {
        // Игрок закрыл меню навыков — сразу запускаем следующий диалог.
        Debug.Log("[XPLevelManager] Ability chosen — starting seqAbilityTried.");
        OnAbilityTried();
    }

    private void OnAbilityTried()
    {
        // Прерываем текущий диалог и сразу запускаем seqAbilityTried.
        // Дальнейшая цепочка (→ seqDoorUnlocked → дверь) идёт через OnNarratorCompleted.
        narratorChannel?.Stop();
        if (seqAbilityTried != null)
            narratorChannel?.Raise(seqAbilityTried);
        else if (seqDoorUnlocked != null)
            narratorChannel?.Raise(seqDoorUnlocked);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>Встряхивает levelUpPrompt как reject-эффект (аналог ShakeLabels в квесте).</summary>
    private IEnumerator ShakePrompt()
    {
        if (levelUpPrompt == null) yield break;
        _promptShaking = true;

        var rt = levelUpPrompt.GetComponent<RectTransform>();
        if (rt == null) { _promptShaking = false; yield break; }

        // Красим временно красным
        var txt = levelUpPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        Color originalColor = txt != null ? txt.color : Color.white;
        if (txt != null) txt.color = Color.red;

        Vector2 origin  = rt.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < promptShakeDuration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-promptShakeMagnitude, promptShakeMagnitude) *
                      (1f - elapsed / promptShakeDuration);
            rt.anchoredPosition = origin + new Vector2(x, 0f);
            yield return null;
        }

        rt.anchoredPosition = origin;
        if (txt != null) txt.color = originalColor;
        _promptShaking = false;
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

    private IEnumerator FlashFill(int times, float interval)
    {
        if (fillImage == null) yield break;

        for (int i = 0; i < times; i++)
        {
            fillImage.color = flashColor;
            yield return new WaitForSeconds(interval);
            fillImage.color = _fillOriginalColor;
            yield return new WaitForSeconds(interval);
        }
    }
}
