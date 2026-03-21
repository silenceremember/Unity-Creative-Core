using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Shared audio configuration for volume sliders and pause menu.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Audio Config", fileName = "AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header("Mixer")]
    [Tooltip("AudioMixer asset")]
    [SerializeField] private AudioMixer audioMixer;
    public AudioMixer AudioMixer => audioMixer;

    [Tooltip("Exposed parameter name in AudioMixer")]
    [SerializeField] private string exposedParam = "Master";
    public string ExposedParam => exposedParam;

    [Header("Volume Range")]
    [Tooltip("dB value at slider = 1 (max volume)")]
    [SerializeField] private float dBMax = 10f;
    public float DBMax => dBMax;

    [Tooltip("dB value at slider = 0 (min volume)")]
    [SerializeField] private float dBMin = -30f;
    public float DBMin => dBMin;

    [Tooltip("Default volume (0-1)")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultVolume = 0.25f;
    public float DefaultVolume => defaultVolume;

    /// <summary>Fired whenever volume changes (normalized 0-1).</summary>
    public event Action<float> OnVolumeChanged;

    /// <summary>Applies normalized volume (0-1) to the mixer, saves to PlayerPrefs, notifies listeners.</summary>
    public void ApplyVolume(float value)
    {
        if (audioMixer == null) return;
        float dB = Mathf.Lerp(dBMax, dBMin, value);
        audioMixer.SetFloat(exposedParam, dB);
        PlayerPrefs.SetFloat(AudioPrefsKeys.MasterVolume, value);
        OnVolumeChanged?.Invoke(value);
    }
}
