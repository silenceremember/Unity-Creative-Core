using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Добавьте на любой GameObject с Button.
/// Звук маршрутизируется через AudioMixerGroup — громкость управляется микшером.
/// Работает при Time.timeScale = 0 (ignoreListenerPause).
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour
{
    [Tooltip("Звуковой клип (напр. MenuButton.mp3)")]
    public AudioClip clip;

    [Tooltip("AudioMixerGroup (например Master или SFX) — громкость регулируется в нём")]
    public AudioMixerGroup mixerGroup;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlaySound);
    }

    void OnDestroy()
    {
        if (TryGetComponent<Button>(out var btn))
            btn.onClick.RemoveListener(PlaySound);
    }

    private void PlaySound()
    {
        if (clip == null) return;

        var go     = new GameObject("ButtonSFX");
        var source = go.AddComponent<AudioSource>();
        source.ignoreListenerPause   = true;
        source.outputAudioMixerGroup = mixerGroup;
        source.PlayOneShot(clip);
        Destroy(go, clip.length + 0.1f);
    }
}
