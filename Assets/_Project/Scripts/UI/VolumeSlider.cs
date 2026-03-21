using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls volume via AudioConfig SO.
/// Slider: 0 (silence) to 1 (max).
/// Syncs automatically with other VolumeSliders via AudioConfig.OnVolumeChanged.
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

    void OnEnable()
    {
        if (audioConfig != null)
            audioConfig.OnVolumeChanged += OnExternalVolumeChanged;
    }

    void OnDisable()
    {
        if (audioConfig != null)
            audioConfig.OnVolumeChanged -= OnExternalVolumeChanged;
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value) => audioConfig.ApplyVolume(value);

    private void OnExternalVolumeChanged(float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }
}

