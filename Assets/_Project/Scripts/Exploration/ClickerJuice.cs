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
    [Header("Punch (main hit)")]
    [Tooltip("Punch scale strength on regular click")]
    [SerializeField] private float punchStrength   = 0.45f;
    [Tooltip("Punch animation duration")]
    [SerializeField] private float punchDuration   = 0.28f;
    [Tooltip("Punch vibration count")]
    [SerializeField] private int   punchVibrato    = 6;
    [Tooltip("Punch elasticity")]
    [SerializeField] private float punchElasticity = 0.6f;

    [Header("Mega Punch (every N clicks)")]
    [SerializeField] private int   megaEvery    = 10;
    [SerializeField] private float megaStrength = 1.1f;
    [SerializeField] private float megaDuration = 0.5f;
    [SerializeField] private float megaRotation = 12f;

    [Header("Colors (by heat)")]
    [SerializeField] private Color colorCold = new Color(0.7f, 0.9f, 1.0f);
    [SerializeField] private Color colorWarm = new Color(1.0f, 0.85f, 0.3f);
    [SerializeField] private Color colorHot  = new Color(1.0f, 0.35f, 0.1f);
    [SerializeField] private Color colorMega = new Color(1.0f, 1.0f, 1.0f);

    [Header("Heat (intensity)")]
    [Tooltip("Heat gained per regular click (0-1)")]
    [SerializeField] private float heatPerClick    = 0.08f;
    [Tooltip("Mega-click heat = heatPerClick × megaHeatMultiplier")]
    [SerializeField] private float megaHeatMultiplier = 5f;
    [Tooltip("Heat decay rate per second")]
    [SerializeField] private float heatDecayRate   = 0.18f;
    [Tooltip("Warm heat threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float heatWarmThreshold = 0.35f;
    [Tooltip("Hot heat threshold")]
    [Range(0f, 1f)]
    [SerializeField] private float heatHotThreshold  = 0.70f;

    [Header("Pop Sound (clicker)")]
    [Tooltip("1-15 clips — random on each click")]
    [SerializeField] private AudioClip[] popClips;
    [Tooltip("AudioSource for pop sounds (2D)")]
    [SerializeField] private AudioSource popSource;
    [Tooltip("Pitch at cold")]
    [SerializeField] private float pitchMin = 0.85f;
    [Tooltip("Pitch at hot")]
    [SerializeField] private float pitchMax = 1.35f;

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
            _heat = Mathf.Max(0f, _heat - heatDecayRate * Time.unscaledDeltaTime);

            if (!_megaFlashing)
                ApplyHeatColor(instant: false);
        }
    }

    /// <summary>Call on each LMB click.</summary>
    public void RegisterClick()
    {
        _count++;

        bool isMega = (_count % megaEvery == 0) && CurrentHeatState != HeatState.Cold;

        float heatGain = isMega
            ? heatPerClick * megaHeatMultiplier
            : heatPerClick;
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

        popSource.pitch = Mathf.Lerp(pitchMin, pitchMax, _heat);
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
        _heat >= heatHotThreshold  ? HeatState.Hot  :
        _heat >= heatWarmThreshold ? HeatState.Warm :
                                     HeatState.Cold;

    private void PlayPunch()
    {
        DOTween.Kill(_rect, complete: false);
        _rect.localScale = _baseScale;

        float boost = 1f + Mathf.Clamp01(_heat * 0.5f);
        _rect.DOPunchScale(
            Vector3.one * punchStrength * boost,
            punchDuration / boost,
            punchVibrato,
            punchElasticity
        ).SetUpdate(true).OnComplete(() => _rect.localScale = _baseScale);
    }

    private void PlayMegaPunch()
    {
        DOTween.Kill(_rect, complete: false);
        _rect.localScale    = _baseScale;
        _rect.localRotation = Quaternion.identity;

        _megaFlashing = true;
        _colorTween?.Kill();
        _colorTween = _label.DOColor(colorMega, 0.05f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _megaFlashing = false;
                ApplyHeatColor(instant: false);
            });

        Sequence s = DOTween.Sequence().SetUpdate(true);
        s.Join(_rect.DOPunchScale(Vector3.one * megaStrength, megaDuration, 8, 0.5f));
        s.Join(_rect.DOPunchRotation(Vector3.forward * megaRotation, megaDuration, 8, 0.5f));
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
            0.12f
        ).SetEase(Ease.OutExpo).SetUpdate(true);
    }

    private void ApplyHeatColor(bool instant)
    {
        Color target = CurrentHeatState switch
        {
            HeatState.Hot  => colorHot,
            HeatState.Warm => colorWarm,
            _              => colorCold,
        };

        _colorTween?.Kill();
        if (instant)
            _label.color = target;
        else
            _colorTween = _label.DOColor(target, 0.3f).SetUpdate(true);
    }
}
