using UnityEngine;

/// <summary>
/// Shared audio configuration for volume sliders and pause menu.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Audio Config", fileName = "AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Tooltip("Exposed parameter name in AudioMixer")]
    [SerializeField] private string exposedParam = "Master";
    public string ExposedParam => exposedParam;

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
}
