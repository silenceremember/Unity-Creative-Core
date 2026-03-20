using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Shared UI audio configuration — reusable click sounds, mixer group, etc.
/// Assign a single instance to all ButtonSoundEffect components.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/UI Audio Config", fileName = "UIAudioConfig")]
public class UIAudioConfig : ScriptableObject
{
    [Header("Button Click")]
    [Tooltip("Default click sound for UI buttons")]
    [SerializeField] private AudioClip clickSound;
    public AudioClip ClickSound => clickSound;

    [Header("Mixer")]
    [Tooltip("AudioMixerGroup for UI sounds (e.g. Master or SFX)")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    public AudioMixerGroup MixerGroup => mixerGroup;
}
