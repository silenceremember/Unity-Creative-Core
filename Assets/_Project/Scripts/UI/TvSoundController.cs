using UnityEngine;

/// <summary>
/// Plays TV on/off sound effect.
/// Assign audioSource in Inspector. Call Toggle() or SetOn(bool).
/// </summary>
public class TvSoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private bool _isOn;

    public void SetOn(bool on)
    {
        if (_isOn == on) return;
        _isOn = on;
        if (audioSource != null) audioSource.Play();
    }

    /// <summary>Syncs state without playing sound (for initialization).</summary>
    public void SyncState(bool on) => _isOn = on;

    public void Toggle() => SetOn(!_isOn);
}
