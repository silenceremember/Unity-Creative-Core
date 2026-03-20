using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Juicy clicker with a heat system.
///
/// Heat (0-1) — decays continuously over time. Each click adds
/// a small amount of heat; mega-click adds megaHeatMultiplier times more.
/// Color and sound pitch depend on current heat, not combo counter.
///
/// Sounds:
///  - popClips[0..N]  → random pop on each click (via popSource)
///  - coinClip        → plays alongside pop on mega-click (via coinSource)
///  Pop pitch increases with heat from pitchMin to pitchMax.
///
/// Call RegisterClick() on each LMB click.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ClickerJuice : MonoBehaviour
{
    [SerializeField] private ClickerConfig config;

    [Header("Pop Sound (clicker)")]
    [Tooltip("1-15 clips — random on each click")]
    [SerializeField] private AudioClip[] popClips;
    [Tooltip("AudioSource for pop sounds (2D)")]
    [SerializeField] private AudioSource popSource;

    [Header("Coin Sound (mega-click)")]
    [Tooltip("Coin sound — plays alongside pop on each mega-click")]
    [SerializeField] private AudioClip coinClip;
    [Tooltip("AudioSource for coin sound")]
    [SerializeField] private AudioSource coinSource;

    private TextMeshProUGUI _label;
    private RectTransform   _rect;
    private Vector3         _baseScale;

    private int   _count;
    private float _heat;
    private bool  _megaFlashing;

    private Tween _colorTween;
    private Tween _rollTween;

    void Awake()
    {
        _label      = GetComponent<TextMeshProUGUI>();
        _rect       = GetComponent<RectTransform>();
        _rect.pivot = new Vector2(0.5f, 0.5f);
        _baseScale  = _rect.localScale;
    }

    void OnEnable()
    {
        _heat         = 0f;
        _megaFlashing = false;
        ApplyHeatColor(instant: true);
    }

    void OnDisable()
    {
        _colorTween?.Kill();
        _rollTween?.Kill();
        DOTween.Kill(_rect);
        _rect.localScale    = _baseScale;
        _rect.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (_heat > 0f)
        {
            _heat = Mathf.Max(0f, _heat - config.HeatDecayRate * Time.unscaledDeltaTime);

            if (!_megaFlashing)
                ApplyHeatColor(instant: false);
        }
    }

    /// <summary>Call on each LMB click.</summary>
    public void RegisterClick()
    {
        _count++;

        bool isMega = (_count % config.MegaEvery == 0) && CurrentHeatState != HeatState.Cold;

        float heatGain = isMega
            ? config.HeatPerClick * config.MegaHeatMultiplier
            : config.HeatPerClick;
        _heat = Mathf.Clamp01(_heat + heatGain);

        RollNumber(_count);

        PlayPopSound();
        if (isMega) PlayCoinSound();

        if (isMega)
            PlayMegaPunch();
        else
        {
            if (!_megaFlashing) ApplyHeatColor(instant: false);
            PlayPunch();
        }
    }

    private void PlayPopSound()
    {
        if (popSource == null || popClips == null || popClips.Length == 0) return;
        var clip = popClips[Random.Range(0, popClips.Length)];
        if (clip == null) return;

        popSource.pitch = Mathf.Lerp(config.PitchMin, config.PitchMax, _heat);
        popSource.PlayOneShot(clip);
    }

    private void PlayCoinSound()
    {
        if (coinSource == null || coinClip == null) return;
        coinSource.PlayOneShot(coinClip);
    }

    /// <summary>Heat state by current heat value.</summary>
    private enum HeatState { Cold, Warm, Hot }

    private HeatState CurrentHeatState =>
        _heat >= config.HeatHotThreshold  ? HeatState.Hot  :
        _heat >= config.HeatWarmThreshold ? HeatState.Warm :
                                     HeatState.Cold;

    private void PlayPunch()
    {
        DOTween.Kill(_rect, complete: false);
        _rect.localScale = _baseScale;

        float boost = 1f + Mathf.Clamp01(_heat * 0.5f);
        _rect.DOPunchScale(
            Vector3.one * config.PunchStrength * boost,
            config.PunchDuration / boost,
            config.PunchVibrato,
            config.PunchElasticity
        ).SetUpdate(true).OnComplete(() => _rect.localScale = _baseScale);
    }

    private void PlayMegaPunch()
    {
        DOTween.Kill(_rect, complete: false);
        _rect.localScale    = _baseScale;
        _rect.localRotation = Quaternion.identity;

        _megaFlashing = true;
        _colorTween?.Kill();
        _colorTween = _label.DOColor(config.ColorMega, 0.05f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _megaFlashing = false;
                ApplyHeatColor(instant: false);
            });

        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Join(_rect.DOPunchScale(Vector3.one * config.MegaStrength, config.MegaDuration, 8, 0.5f));
        s.Join(_rect.DOPunchRotation(Vector3.forward * config.MegaRotation, config.MegaDuration, 8, 0.5f));
        s.OnComplete(() =>
        {
            _rect.localScale    = _baseScale;
            _rect.localRotation = Quaternion.identity;
        });
    }

    private void RollNumber(int target)
    {
        _rollTween?.Kill();
        int displayed = target - 1;

        _rollTween = DOTween.To(
            () => displayed,
            v =>
            {
                displayed   = v;
                _label.text = v.ToString();
            },
            target,
            config.RollNumberDuration
        ).SetEase(Ease.OutExpo).SetUpdate(true);
    }

    private void ApplyHeatColor(bool instant)
    {
        Color target = CurrentHeatState switch
        {
            HeatState.Hot  => config.ColorHot,
            HeatState.Warm => config.ColorWarm,
            _              => config.ColorCold,
        };

        _colorTween?.Kill();
        if (instant)
            _label.color = target;
        else
            _colorTween = _label.DOColor(target, config.ColorTransitionDuration).SetUpdate(true);
    }
}
