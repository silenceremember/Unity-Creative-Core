using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls volume via AudioConfig SO.
/// Slider: 0 (silence) to 1 (max).
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private AudioConfig audioConfig;

    [Header("UI")]
    [Tooltip("Slider (auto-found if not assigned)")]
    [SerializeField] private Slider slider;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;

            float saved = PlayerPrefs.GetFloat(AudioPrefsKeys.MasterVolume, audioConfig.DefaultVolume);
            slider.value = saved;

            slider.onValueChanged.AddListener(OnSliderChanged);
            audioConfig.ApplyVolume(saved);
        }
    }

    private void OnSliderChanged(float value) => audioConfig.ApplyVolume(value);

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
