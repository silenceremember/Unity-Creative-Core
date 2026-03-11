using UnityEngine;

/// <summary>
/// Включает/выключает TV: меняет материал экрана, аудио и свет.
/// Повесь на любой GameObject (например менеджер сцены).
/// </summary>
public class TVController : MonoBehaviour
{
    [Header("Renderer")]
    public MeshRenderer tvRenderer;
    [Tooltip("Индекс слота материала экрана (обычно 1)")]
    public int screenMaterialIndex = 1;
    public Material screenOn;   // TV_Screen.mat  (создай сам из TV_Screen.shadergraph)
    public Material screenOff;  // TV_ScreenOff.mat

    [Header("Audio")]
    public AudioSource tvAudio;

    [Header("Light")]
    public Light tvLight;

    // ── Текущее состояние ──────────────────────
    private bool _isOn = true;

    void Start()
    {
        // Определяем начальное состояние по материалу
        if (tvRenderer != null && screenOff != null)
        {
            var mats = tvRenderer.sharedMaterials;
            _isOn = (screenMaterialIndex < mats.Length) &&
                    (mats[screenMaterialIndex].name != screenOff.name);
        }
    }

    // ── Public: привязываются к кнопкам ───────

    public void TurnOn()
    {
        SetState(true);
    }

    public void TurnOff()
    {
        SetState(false);
    }

    public void Toggle()
    {
        SetState(!_isOn);
    }

    // ── Логика ────────────────────────────────

    private void SetState(bool on)
    {
        _isOn = on;

        // Материал
        if (tvRenderer != null)
        {
            var mats = tvRenderer.materials; // копия массива
            if (screenMaterialIndex < mats.Length)
                mats[screenMaterialIndex] = on ? screenOn : screenOff;
            tvRenderer.materials = mats;
        }

        // Аудио
        if (tvAudio != null)
        {
            if (on) { if (!tvAudio.isPlaying) tvAudio.Play(); }
            else    { tvAudio.Stop(); }
        }

        // Свет
        if (tvLight != null)
            tvLight.enabled = on;
    }
}
