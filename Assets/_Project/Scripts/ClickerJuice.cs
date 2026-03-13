using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Сочный кликер с DOTween-анимациями.
/// Повесь на GameObject с TextMeshProUGUI.
/// Вызывай RegisterClick() при каждом клике.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class ClickerJuice : MonoBehaviour
{
    // ── Tune ─────────────────────────────────────────────────────────

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

    [Header("Combo-тайм-аут (сек без кликов → сбросить комбо)")]
    public float comboResetTime = 1.2f;

    [Header("Цвета (по комбо)")]
    public Color colorCold = new Color(0.7f, 0.9f, 1.0f);  // редкие клики
    public Color colorWarm = new Color(1.0f, 0.85f, 0.3f); // средний темп
    public Color colorHot  = new Color(1.0f, 0.35f, 0.1f); // быстрые клики
    public Color colorMega = new Color(1.0f, 1.0f, 1.0f);  // мега-клик flash

    // ── State ─────────────────────────────────────────────────────────

    private TextMeshProUGUI _label;
    private RectTransform   _rect;

    private int   _count;
    private int   _combo;
    private float _lastClickTime = -999f;
    private bool  _megaFlashing; // true пока идёт белая вспышка

    private Tween _colorTween;
    private Tween _rollTween;

    private Vector3 _baseScale;

    // ── Lifecycle ─────────────────────────────────────────────────────

    void Awake()
    {
        _label     = GetComponent<TextMeshProUGUI>();
        _rect      = GetComponent<RectTransform>();

        // Pivot должен быть в центре объекта, иначе DOPunchScale
        // будет масштабировать от чужой точки (например, центра канваса)
        _rect.pivot = new Vector2(0.5f, 0.5f);

        _baseScale = _rect.localScale;
    }

    void OnEnable()
    {
        // Сбрасываем состояние при повторном включении
        _combo        = 0;
        _megaFlashing = false;
        _lastClickTime = -999f;
        ApplyComboColor(instant: true);
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
        // Остывание: если прошло comboResetTime без кликов — сбрасываем комбо
        // и плавно возвращаем холодный цвет
        if (_combo > 0 && !_megaFlashing &&
            Time.unscaledTime - _lastClickTime >= comboResetTime)
        {
            _combo = 0;
            ApplyComboColor(instant: false);
        }
    }

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>Вызвать при каждом клике ЛКМ.</summary>
    public void RegisterClick()
    {
        _count++;

        // — Обновление комбо —
        float now = Time.unscaledTime;
        if (now - _lastClickTime < comboResetTime)
            _combo++;
        else
            _combo = 1;
        _lastClickTime = now;

        // — Обновить текст с roll-эффектом —
        RollNumber(_count);

        bool isMega = (_count % megaEvery == 0);
        if (isMega)
            PlayMegaPunch();
        else
        {
            // Цвет обновляем только вне mega-вспышки
            if (!_megaFlashing)
                ApplyComboColor(instant: false);
            PlayPunch();
        }
    }

    // ── Анимации ──────────────────────────────────────────────────────

    private void PlayPunch()
    {
        DOTween.Kill(_rect, complete: false);
        _rect.localScale = _baseScale;

        float boost = 1f + Mathf.Clamp01((_combo - 1) * 0.04f);
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

        // Вспышка белым → потом возврат к текущему комбо-цвету
        _megaFlashing = true;
        _colorTween?.Kill();
        _colorTween = _label.DOColor(colorMega, 0.05f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _megaFlashing = false;
                ApplyComboColor(instant: false);
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

    // ── Число с прокруткой ────────────────────────────────────────────

    private void RollNumber(int target)
    {
        _rollTween?.Kill();
        int displayed = target - 1;

        _rollTween = DOTween.To(
            () => displayed,
            v =>
            {
                displayed    = v;
                _label.text  = v.ToString();
            },
            target,
            0.12f
        ).SetEase(Ease.OutExpo).SetUpdate(true);
    }

    // ── Цвет по комбо ─────────────────────────────────────────────────

    private void ApplyComboColor(bool instant)
    {
        Color target;
        if      (_combo >= 8) target = colorHot;
        else if (_combo >= 4) target = colorWarm;
        else                  target = colorCold;

        _colorTween?.Kill();
        if (instant)
            _label.color = target;
        else
            _colorTween = _label.DOColor(target, 0.3f).SetUpdate(true);
    }
}
