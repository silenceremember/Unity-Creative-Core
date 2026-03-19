using UnityEngine;

/// <summary>
/// Воспроизводит звук включения/выключения ТВ.
/// Назначь audioSource в Inspector. Вызывай Toggle() или SetOn(bool).
/// </summary>
public class TvSoundController : MonoBehaviour
{
    public AudioSource audioSource;

    private bool _isOn;

    public void SetOn(bool on)
    {
        if (_isOn == on) return;
        _isOn = on;
        if (audioSource != null) audioSource.Play();
    }

    /// <summary>Синхронизирует состояние без звука (для инициализации).</summary>
    public void SyncState(bool on) => _isOn = on;

    public void Toggle() => SetOn(!_isOn);
}
