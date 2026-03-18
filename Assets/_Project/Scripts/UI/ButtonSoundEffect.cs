using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Добавьте на любой GameObject с Button.
/// Звук воспроизводится через AudioSource на том же объекте — без спавна.
/// hoverClip  — играет при наведении мыши.
/// clickClip  — играет при нажатии.
/// Работает при Time.timeScale = 0 (ignoreListenerPause).
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Звуки")]
    [Tooltip("Звук при наведении мыши")]
    public AudioClip hoverClip;

    [Tooltip("Звук при нажатии")]
    public AudioClip clickClip;

    [Header("Настройки")]
    [Tooltip("AudioMixerGroup (например Master или SFX)")]
    public AudioMixerGroup mixerGroup;

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake             = false;
        _audioSource.ignoreListenerPause     = true;
        _audioSource.outputAudioMixerGroup   = mixerGroup;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        Play(hoverClip);
    }

    public void OnPointerClick(PointerEventData _)
    {
        Play(clickClip);
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip);
    }
}
