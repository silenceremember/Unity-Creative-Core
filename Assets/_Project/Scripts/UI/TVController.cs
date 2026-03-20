using UnityEngine;

/// <summary>
/// Turns TV on/off: changes screen material, audio, light, and plays on/off sound.
/// </summary>
public class TVController : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private MeshRenderer tvRenderer;
    [Tooltip("Screen material slot index (usually 1)")]
    [SerializeField] private int screenMaterialIndex = 1;
    [SerializeField] private Material screenOn;
    [SerializeField] private Material screenOff;

    [Header("Audio")]
    [SerializeField] private AudioSource tvAudio;

    [Header("On/Off Sound")]
    [Tooltip("AudioSource for the on/off toggle sound")]
    [SerializeField] private AudioSource toggleSoundSource;

    [Header("Light")]
    [SerializeField] private Light tvLight;

    private bool _isOn = true;

    void Start() => SetState(true);

    public void TurnOn()  => SetState(true);
    public void TurnOff() => SetState(false);
    public void Toggle()  => SetState(!_isOn);

    private void SetState(bool on)
    {
        bool changed = _isOn != on;
        _isOn = on;

        if (tvRenderer != null)
        {
            var mats = tvRenderer.materials;
            if (screenMaterialIndex < mats.Length)
                mats[screenMaterialIndex] = on ? screenOn : screenOff;
            tvRenderer.materials = mats;
        }

        if (tvAudio != null)
        {
            if (on) { if (!tvAudio.isPlaying) tvAudio.Play(); }
            else    { tvAudio.Stop(); }
        }

        if (tvLight != null)
            tvLight.enabled = on;

        if (changed && toggleSoundSource != null)
            toggleSoundSource.Play();
    }
}
