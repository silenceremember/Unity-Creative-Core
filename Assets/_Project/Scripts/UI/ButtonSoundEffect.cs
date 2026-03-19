using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Добавьте на любой GameObject с Button.
/// clickClip — играет при нажатии (только если кнопка активна).
/// Работает при Time.timeScale = 0 (ignoreListenerPause).
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonSoundEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Звуки")]
    [Tooltip("Звук при нажатии")]
    public AudioClip clickClip;

    [Header("Настройки")]
    [Tooltip("AudioMixerGroup (например Master или SFX)")]
    public AudioMixerGroup mixerGroup;

    private AudioSource _audioSource;
    private Button _button;

    void Awake()
    {
        _button      = GetComponent<Button>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake           = false;
        _audioSource.ignoreListenerPause   = true;
        _audioSource.outputAudioMixerGroup = mixerGroup;
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (_button != null && !_button.interactable) return;
        if (clickClip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clickClip);
    }
}
