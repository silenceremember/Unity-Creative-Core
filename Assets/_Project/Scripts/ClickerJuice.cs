using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Сочный кликер с heat-системой.
///
/// Heat (0-1) — непрерывно убывает со временем. Каждый клик добавляет
/// небольшую порцию тепла, мега-клик — в megaHeatMultiplier раз больше.
/// Цвет и питч звуков зависят от текущего heat, а не от комбо-счётчика.
///
/// Звуки:
///  - popClips[0..N]  → рандомный pop при каждом клике (через popSource)
///  - coinClip        → играет вместе с pop на мега-клике (через coinSource)
///  Питч popSource повышается с heat от pitchMin до pitchMax.
///
/// Вызывай RegisterClick() при каждом клике ЛКМ.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ClickerJuice : MonoBehaviour
{
    // ── Visual ────────────────────────────────────────────────────────

    [Header("Punch (основной удар)")]
    [Tooltip("Сила punch-scale при обычном клике")]
    public float punchStrength   = 0.45f;
    [Tooltip("Длительность punch-анимации")]
    public float punchDuration   = 0.28f;
    [Tooltip("Количество вибраций punch")]
    public int   punchVibrato    = 6;
    [Tooltip("Эластичность punch")]
    public float punchElasticity = 0.6f;

    [Header("Mega Punch (каждые N кликов)")]
    public int   megaEvery    = 10;
    public float megaStrength = 1.1f;
    public float megaDuration = 0.5f;
    public float megaRotation = 12f;

    [Header("Цвета (по heat)")]
    public Color colorCold = new Color(0.7f, 0.9f, 1.0f);
    public Color colorWarm = new Color(1.0f, 0.85f, 0.3f);
    public Color colorHot  = new Color(1.0f, 0.35f, 0.1f);
    public Color colorMega = new Color(1.0f, 1.0f, 1.0f);

    // ── Heat ──────────────────────────────────────────────────────────

    [Header("Heat (интенсивность)")]
    [Tooltip("Сколько heat даёт один обычный клик (0-1)")]
    public float heatPerClick    = 0.08f;
    [Tooltip("Мега-клик даёт heat = heatPerClick × megaHeatMultiplier")]
    public float megaHeatMultiplier = 5f;
    [Tooltip("Скорость убывания heat в секунду (heat/sec)")]
    public float heatDecayRate   = 0.18f;
    [Tooltip("Порог heat для Warm")]
    [Range(0f, 1f)]
    public float heatWarmThreshold = 0.35f;
    [Tooltip("Порог heat для Hot")]
    [Range(0f, 1f)]
    public float heatHotThreshold  = 0.70f;

    // ── Audio — Pop ───────────────────────────────────────────────────

    [Header("Звук Pop (кликер)")]
    [Tooltip("1-15 клипов — рандомный при каждом клике")]
    public AudioClip[] popClips;
    [Tooltip("AudioSource для pop-звуков (2D, создаётся автоматически если пусто)")]
    public AudioSource popSource;
    [Tooltip("Питч при cold (минимальный)")]
    public float pitchMin = 0.85f;
    [Tooltip("Питч при hot (максимальный)")]
    public float pitchMax = 1.35f;

    // ── Audio — Coin ──────────────────────────────────────────────────

    [Header("Звук Coin (мега-клик)")]
    [Tooltip("Звук монеты — играет вместе с pop на каждом мега-клике")]
    public AudioClip coinClip;
    [Tooltip("AudioSource для coin-звука (создаётся автоматически если пусто)")]
    public AudioSource coinSource;

    // ── State ─────────────────────────────────────────────────────────

    private TextMeshProUGUI _label;
    private RectTransform   _rect;
    private Vector3         _baseScale;

    private int   _count;
    private float _heat;           // текущее тепло 0-1
    private bool  _megaFlashing;

    private Tween _colorTween;
    private Tween _rollTween;

    // ── Lifecycle ─────────────────────────────────────────────────────

    void Awake()
    {
        _label      = GetComponent<TextMeshProUGUI>();
        _rect       = GetComponent<RectTransform>();
        _rect.pivot = new Vector2(0.5f, 0.5f);
        _baseScale  = _rect.localScale;

        // PopSource
        if (popSource == null)
        {
            popSource = gameObject.AddComponent<AudioSource>();
            popSource.playOnAwake  = false;
            popSource.spatialBlend = 0f;
        }

        // CoinSource
        if (coinSource == null)
        {
            coinSource = gameObject.AddComponent<AudioSource>();
            coinSource.playOnAwake  = false;
            coinSource.spatialBlend = 0f;
        }
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
        // Убываем heat со временем
        if (_heat > 0f)
        {
            _heat = Mathf.Max(0f, _heat - heatDecayRate * Time.unscaledDeltaTime);

            // Обновляем цвет только вне mega-вспышки
            if (!_megaFlashing)
                ApplyHeatColor(instant: false);
        }
    }

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>Вызвать при каждом клике ЛКМ.</summary>
    public void RegisterClick()
    {
        _count++;

        // Мега работает только со второй фазы (Warm / Hot)
        bool isMega = (_count % megaEvery == 0) && CurrentHeatState != HeatState.Cold;

        // — Добавляем heat —
        float heatGain = isMega
            ? heatPerClick * megaHeatMultiplier
            : heatPerClick;
        _heat = Mathf.Clamp01(_heat + heatGain);

        // — Число —
        RollNumber(_count);

        // — Звук —
        PlayPopSound();
        if (isMega) PlayCoinSound();

        // — Анимация —
        if (isMega)
            PlayMegaPunch();
        else
        {
            if (!_megaFlashing) ApplyHeatColor(instant: false);
            PlayPunch();
        }
    }

    // ── Sound ─────────────────────────────────────────────────────────

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

    // ── Heat helpers ──────────────────────────────────────────────────

    /// <summary>Состояние тепла по текущему heat.</summary>
    private enum HeatState { Cold, Warm, Hot }

    private HeatState CurrentHeatState =>
        _heat >= heatHotThreshold  ? HeatState.Hot  :
        _heat >= heatWarmThreshold ? HeatState.Warm :
                                     HeatState.Cold;

    // ── Visual ────────────────────────────────────────────────────────

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
