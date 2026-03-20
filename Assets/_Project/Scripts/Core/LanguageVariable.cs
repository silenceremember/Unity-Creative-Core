using System;
using UnityEngine;

/// <summary>
/// Runtime language variable as ScriptableObject.
/// Resets to initial value on play-mode enter.
/// </summary>
[CreateAssetMenu(menuName = "Game/Variables/Language Variable", fileName = "LanguageVariable")]
public class LanguageVariable : ScriptableObject
{
    [SerializeField] private GameLanguage initialValue;

    [NonSerialized] private GameLanguage _runtimeValue;

    public GameLanguage Value
    {
        get => _runtimeValue;
        set
        {
            if (_runtimeValue == value) return;
            _runtimeValue = value;
            OnChanged?.Invoke(value);
        }
    }

    public event Action<GameLanguage> OnChanged;

    void OnEnable()
    {
        _runtimeValue = initialValue;
        OnChanged = null;
    }
}
