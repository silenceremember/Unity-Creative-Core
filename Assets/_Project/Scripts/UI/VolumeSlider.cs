using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Controls volume via AudioMixer exposed parameter.
/// Slider: 0.0001 (−80 dB silence) to 1.0 (0 dB max).
/// </summary>
public class VolumeSlider : MonoBehaviour
{
    [Header("AudioMixer")]
    [Tooltip("Drag AudioMixer.mixer here")]
    [SerializeField] private AudioMixer audioMixer;

    [Tooltip("Exposed parameter name in AudioMixer")]
    [SerializeField] private string exposedParam = "Master";

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

            PlayerPrefs.DeleteKey(AudioPrefsKeys.MasterVolume);
            float saved = PlayerPrefs.GetFloat(AudioPrefsKeys.MasterVolume, 0.25f);
            slider.value = saved;

            slider.onValueChanged.AddListener(ApplyVolume);
            ApplyVolume(saved);
        }
    }

    public void ApplyVolume(float value)
    {
        if (audioMixer == null) return;

        float dB = Mathf.Lerp(10f, -30f, value);
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(AudioPrefsKeys.MasterVolume, value);
    }

    void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(ApplyVolume);
    }
}
