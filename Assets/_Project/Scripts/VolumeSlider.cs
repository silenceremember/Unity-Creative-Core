using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Управляет громкостью через AudioMixer exposed parameter.
/// Слайдер: 0.0001 (−80 dB тишина) до 1.0 (0 dB максимум).
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    [Header("AudioMixer")]
    [Tooltip("Перетащите сюда AudioMixer.mixer")]
    public AudioMixer audioMixer;

    [Tooltip("Имя exposed parameter в AudioMixer (expose его через ПКМ → Expose)")]
    public string exposedParam = "Master";

    [Header("UI")]
    [Tooltip("Слайдер (будет найден автоматически, если не назначен)")]
    public Slider slider;

    // Ключ для PlayerPrefs (v2 — чтобы не подхватывать старые значения)
    private const string PREFS_KEY = "MasterVolumeV2";

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;

            PlayerPrefs.DeleteKey(PREFS_KEY); // сброс устаревшего значения
            float saved = PlayerPrefs.GetFloat(PREFS_KEY, 0.25f);
            slider.value = saved;

            slider.onValueChanged.AddListener(ApplyVolume);

            // Start() — AudioMixer уже инициализирован
            ApplyVolume(saved);
        }
    }

    public void ApplyVolume(float value)
    {
        if (audioMixer == null) return;

        // Линейно: slider 0 = +10 dB, slider 0.25 = 0 dB, slider 1 = -30 dB
        float dB = Mathf.Lerp(10f, -30f, value);
        audioMixer.SetFloat(exposedParam, dB);

        // Сохраняем
        PlayerPrefs.SetFloat(PREFS_KEY, value);
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
