using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add to any GameObject with a Button.
/// clickClip — plays on press (only if button is active).
/// Works at Time.timeScale = 0 (ignoreListenerPause).
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(AudioSource))]
public class ButtonSoundEffect : MonoBehaviour, IPointerClickHandler
{
    [Header("Sounds")]
    [Tooltip("Click sound")]
    [SerializeField] private AudioClip clickClip;

    [Header("Settings")]
    [Tooltip("AudioMixerGroup (e.g. Master or SFX)")]
    [SerializeField] private AudioMixerGroup mixerGroup;

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
