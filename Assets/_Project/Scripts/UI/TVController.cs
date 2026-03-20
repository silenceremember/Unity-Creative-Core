using UnityEngine;

/// <summary>
/// Turns TV on/off: changes screen material, audio, and light.
/// Attach to any GameObject (e.g. scene manager).
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

    [Header("Sound Effect")]
    [Tooltip("TvSoundController on the TV object for on/off sound")]
    [SerializeField] private TvSoundController soundController;

    [Header("Light")]
    [SerializeField] private Light tvLight;

    private bool _isOn = true;

    void Start()
    {
        soundController?.SyncState(true);
        SetState(true);
    }

    public void TurnOn()  => SetState(true);
    public void TurnOff() => SetState(false);
    public void Toggle()  => SetState(!_isOn);

    private void SetState(bool on)
    {
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

        soundController?.SetOn(on);
    }
}
