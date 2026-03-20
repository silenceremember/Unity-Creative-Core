using UnityEngine;
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
    [Header("Config")]
    [SerializeField] private UIAudioConfig uiAudioConfig;

    private AudioSource _audioSource;
    private Button _button;

    void Awake()
    {
        _button      = GetComponent<Button>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake           = false;
        _audioSource.ignoreListenerPause   = true;

        if (uiAudioConfig != null)
            _audioSource.outputAudioMixerGroup = uiAudioConfig.MixerGroup;
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (_button != null && !_button.interactable) return;
        if (uiAudioConfig == null || uiAudioConfig.ClickSound == null || _audioSource == null) return;
        _audioSource.PlayOneShot(uiAudioConfig.ClickSound);
    }
}
