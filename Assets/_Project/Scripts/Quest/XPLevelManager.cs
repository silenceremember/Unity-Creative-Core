using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// XP/Level Manager (UniTask).
/// - Level increases AUTOMATICALLY when the bar fills up.
/// - Lv.N text changes with animation (punch scale + flash).
/// - "▲ Level Up! [X]" blinks — X opens the upgrade canvas.
/// - XP overflow automatically carries over to the next level.
/// - REWARD decreases continuously through first and second pass to 0, then TMP is hidden.
/// </summary>
public class XPLevelManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private QuestConfig config;

    [Header("UI — LevelBar")]
    [SerializeField] private GameObject      levelBar;
    [SerializeField] private TextMeshProUGUI levelLabel;
    [SerializeField] private TextMeshProUGUI xpLabel;
    [SerializeField] private Slider          xpSlider;
    [Tooltip("Direct reference to the fill bar Image")]
    [SerializeField] private Image           fillImage;
    [SerializeField] private GameObject      levelUpPrompt;
    [SerializeField] private LevelUpCanvas   upgradeCanvas;

    [Header("UI — Reward")]
    [SerializeField] private TextMeshProUGUI rewardLabel;



    [Header("XP Sounds")]
    [SerializeField] private AudioClip xpTickSound;
    [SerializeField] private AudioSource xpTickAudioSource;

    [Header("Level Up Sound")]
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioSource levelUpAudioSource;

    [Header("Prompt Sound")]
    [SerializeField] private AudioClip promptSound;
    [SerializeField] private AudioSource promptAudioSource;

    [Header("Narrator")]
    [SerializeField] private NarratorChannel narratorChannel;
    [Tooltip("Dialogue on XP bar activation")]
    [SerializeField] private DialogueSequence seqXPBarActivated;
    [Tooltip("Dialogue after level-up")]
    [SerializeField] private DialogueSequence seqLevelUp;
    [Tooltip("Dialogue when X is pressed")]
    [SerializeField] private DialogueSequence seqAbilityChosen;
    [Tooltip("Dialogue after trying the ability")]
    [SerializeField] private DialogueSequence seqAbilityTried;
    [Tooltip("Dialogue about unlocking new level + door disappearing")]
    [SerializeField] private DialogueSequence seqDoorUnlocked;

    [Header("Door")]
    [Tooltip("Door object — disabled during seqDoorUnlocked narrative")]
    [SerializeField] private GameObject doorObject;

    [Header("Dependencies")]
    [SerializeField] private LanguageVariable languageVar;
    [SerializeField] private BoolVariable isPausedVariable;

    [SerializeField] private BoolVariable narratorPlayingVar;

    [Header("Channels")]
    [SerializeField] private VoidChannel abilityChosenChannel;
    [SerializeField] private IntChannel addXPChannel;



    private int   _level         = 0;
    private int   _currentXP     = 0;
    private float _displayXP     = 0f;
    private bool  _promptVisible = false;
    private Color _fillOriginalColor;
    private bool  _xpBarNarrPlayed = false;
    private bool  _promptShaking   = false;


    private float _xBlockedUntil = 0f;

    private CancellationTokenSource _animCts;
    private CancellationTokenSource _blinkCts;

    private int XPForCurrentLevel =>
        _level < config.XPRequirements.Length
            ? config.XPRequirements[_level]
            : config.XPRequirements[config.XPRequirements.Length - 1];

    private string FormatReward(int amount)
    {
        var lang = languageVar != null ? languageVar.Value : GameLanguage.Russian;
        return string.Format(config.RewardFormat.GetText(lang), amount);
    }


    void Start()
    {
        if (levelBar      != null) levelBar.SetActive(false);
        if (levelUpPrompt != null) levelUpPrompt.SetActive(false);
        InitSlider();
        if (fillImage != null) _fillOriginalColor = fillImage.color;

        if (rewardLabel != null)
        {
            rewardLabel.gameObject.SetActive(true);
            rewardLabel.text = FormatReward(config.QuestRewardXP);
        }

        RefreshUI();

        if (abilityChosenChannel != null)
            abilityChosenChannel.OnRaised += HandleAbilityChosen;
    }

    void OnEnable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted += OnNarratorCompleted;
        if (addXPChannel != null)
            addXPChannel.OnRaised += AddXP;
        if (languageVar != null)
            languageVar.OnChanged += OnLanguageChanged;
    }

    void OnDisable()
    {
        if (narratorChannel != null)
            narratorChannel.OnSequenceCompleted -= OnNarratorCompleted;
        if (addXPChannel != null)
            addXPChannel.OnRaised -= AddXP;
        if (languageVar != null)
            languageVar.OnChanged -= OnLanguageChanged;
    }

    void OnDestroy()
    {
        if (abilityChosenChannel != null)
            abilityChosenChannel.OnRaised -= HandleAbilityChosen;
        CancelAnim();
        _blinkCts?.Cancel(); _blinkCts?.Dispose();
    }

    private void OnLanguageChanged(GameLanguage _)
    {
        if (rewardLabel != null && rewardLabel.gameObject.activeSelf)
            rewardLabel.text = FormatReward(config.QuestRewardXP);
    }

    private void OnNarratorCompleted(DialogueSequence completed)
    {
        if (completed == seqXPBarActivated)
        {
            if (seqLevelUp != null)
                narratorChannel?.Raise(seqLevelUp);
            return;
        }

        if (completed == seqLevelUp)
        {
            ShowUpgradePrompt();
            return;
        }

        if (completed == seqDoorUnlocked)
        {
            if (doorObject != null)
                doorObject.SetActive(false);
        }
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (!_promptVisible || kb == null || !kb.xKey.wasPressedThisFrame) return;
        if (isPausedVariable != null && isPausedVariable.Value) return;

        bool narratorActive  = narratorPlayingVar != null &&
                               narratorPlayingVar.Value;

        if (narratorActive)
        {
            if (!_promptShaking && levelUpPrompt != null)
                ShakePrompt(this.GetCancellationTokenOnDestroy()).Forget();
        }
        else
        {
            _xBlockedUntil = Time.unscaledTime + config.InputSpamCooldown;
            OnUpgradeKeyPressed();
        }
    }

    public void AddXP(int amount)
    {
        if (_animCts != null && !_animCts.IsCancellationRequested) return;

        if (levelBar != null) levelBar.SetActive(true);

        if (!_xpBarNarrPlayed && seqXPBarActivated != null)
        {
            _xpBarNarrPlayed = true;
            narratorChannel?.Raise(seqXPBarActivated);
        }

        if (rewardLabel != null)
        {
            rewardLabel.gameObject.SetActive(true);
            rewardLabel.text = FormatReward(amount);
        }

        _animCts = new CancellationTokenSource();
        TransferAnimation(amount, rewardDisplay: amount, _animCts.Token).SuppressCancellationThrow().Forget();
    }

    private async UniTask TransferAnimation(int xpToAdd, int rewardDisplay, CancellationToken ct)
    {
        int cap     = XPForCurrentLevel;
        int canFill = cap - _currentXP;
        int fill    = Mathf.Min(xpToAdd, canFill);
        int overflow = xpToAdd - fill;
        int endXP   = _currentXP + fill;

        int rewardEnd = rewardDisplay - fill;
        rewardEnd = Mathf.Max(rewardEnd, 0);

        float startXP = _currentXP;
        float dur     = config.FillDuration * Mathf.Max((float)fill / cap, 0.2f);
        float elapsed = 0f;
        float nextTick = 0f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float curve = config.XPFillCurve != null && config.XPFillCurve.length > 0
                ? config.XPFillCurve.Evaluate(t)
                : Mathf.SmoothStep(0f, 1f, t);

            _displayXP = Mathf.Lerp(startXP, endXP, curve);
            if (xpSlider != null) xpSlider.value = _displayXP;
            if (xpLabel  != null) xpLabel.text   = $"{Mathf.RoundToInt(_displayXP)} / {cap}";

            if (elapsed >= nextTick && xpTickSound != null && xpTickAudioSource != null)
            {
                xpTickAudioSource.pitch = Mathf.Lerp(config.TickPitchMin, config.TickPitchMax, curve);
                xpTickAudioSource.PlayOneShot(xpTickSound);
                nextTick = elapsed + config.TickInterval;
            }

            if (rewardLabel != null && rewardLabel.gameObject.activeSelf)
            {
                int rewardLeft = Mathf.RoundToInt(Mathf.Lerp(rewardDisplay, rewardEnd, curve));
                rewardLabel.text = FormatReward(rewardLeft);
                if (rewardLeft <= 0)
                    rewardLabel.gameObject.SetActive(false);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _currentXP = endXP;
        _displayXP = endXP;
        if (xpSlider != null) xpSlider.value = endXP;
        if (xpLabel  != null) xpLabel.text   = $"{endXP} / {cap}";

        if (rewardLabel != null && rewardLabel.gameObject.activeSelf)
        {
            rewardLabel.text = FormatReward(rewardEnd);
            if (rewardEnd <= 0)
                rewardLabel.gameObject.SetActive(false);
        }

        if (_currentXP >= cap)
            await AutoLevelUp(overflow, rewardEnd, ct);
        else
            FinishAnim();
    }

    private async UniTask AutoLevelUp(int overflow, int rewardLeft, CancellationToken ct)
    {
        await FlashFill(times: 3, interval: 0.12f, ct);

        if (levelUpSound != null && levelUpAudioSource != null)
        {
            levelUpAudioSource.pitch = 1f;
            levelUpAudioSource.PlayOneShot(levelUpSound);
        }

        _level++;
        _currentXP = 0;
        _displayXP = 0f;

        PunchLevelLabel(this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow().Forget();

        ResetFillColor();
        InitSlider();
        RefreshUI();

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

    private async UniTask PunchLevelLabel(CancellationToken ct)
    {
        if (levelLabel == null) return;
        var rt    = levelLabel.GetComponent<RectTransform>();
        var canvg = levelLabel.GetComponent<CanvasRenderer>();

        levelLabel.text = $"Lv.{_level}";

        float elapsed = 0f;
        while (elapsed < config.LevelPunchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / config.LevelPunchDuration;
            float scale = 1f + (config.LevelLabelPunch - 1f) * Mathf.Sin(t * Mathf.PI);
            if (rt    != null) rt.localScale = Vector3.one * scale;
            if (canvg != null) canvg.SetAlpha(Mathf.PingPong(t * 6f, 1f));
            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) break;
        }

        if (rt    != null) rt.localScale = Vector3.one;
        if (canvg != null) canvg.SetAlpha(1f);
    }

    private void ShowUpgradePrompt()
    {
        _promptVisible = true;
        if (levelUpPrompt != null) levelUpPrompt.SetActive(true);

        if (promptSound != null && promptAudioSource != null)
            promptAudioSource.PlayOneShot(promptSound);

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
    }

    private async UniTask BlinkUpgradePrompt(CancellationToken ct)
    {
        if (levelUpPrompt == null) return;
        var tmpText = levelUpPrompt.GetComponentInChildren<TextMeshProUGUI>();
        var rt      = levelUpPrompt.GetComponent<RectTransform>();
        Image fillImg = fillImage;

        while (true)
        {
            float t = (Mathf.Sin(Time.time * Mathf.PI * 2f) + 1f) * 0.5f;
            if (tmpText != null) tmpText.alpha = Mathf.Lerp(0.3f, 1f, t);
            if (rt      != null) rt.localScale = Vector3.one * Mathf.Lerp(1f, 1.06f, t);
            if (fillImg != null) fillImg.color = Color.Lerp(_fillOriginalColor, config.FlashColor, t);
            bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) break;
        }

        if (tmpText != null) tmpText.alpha = 1f;
        if (rt      != null) rt.localScale = Vector3.one;
        if (fillImg != null) fillImg.color = _fillOriginalColor;
    }

    private void HandleAbilityChosen()
    {
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

    private async UniTask ShakePrompt(CancellationToken ct)
    {
        if (levelUpPrompt == null) return;
        _promptShaking = true;

        var rt = levelUpPrompt.GetComponent<RectTransform>();
        if (rt == null) { _promptShaking = false; return; }

        var txt = levelUpPrompt.GetComponentInChildren<TextMeshProUGUI>();
        Color originalColor = txt != null ? txt.color : Color.white;
        if (txt != null) txt.color = Color.red;

        await UIAnimationHelper.ShakeAsync(rt, config.PromptShakeDuration, config.PromptShakeMagnitude, ct);

        if (txt != null) txt.color = originalColor;
        _promptShaking = false;
    }

    private async UniTask FlashFill(int times, float interval, CancellationToken ct)
    {
        if (fillImage == null) return;
        int ms = Mathf.RoundToInt(interval * 1000f);
        for (int i = 0; i < times; i++)
        {
            fillImage.color = config.FlashColor;
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
