using System;
using UnityEngine;

/// <summary>
/// Runtime bool variable as ScriptableObject.
/// Resets to initial value on play-mode enter.
/// </summary>
[CreateAssetMenu(menuName = "Game/Variables/Bool Variable", fileName = "BoolVariable")]
public class BoolVariable : ScriptableObject
{
    [SerializeField] private bool initialValue;

    [NonSerialized] private bool _runtimeValue;

    public bool Value
    {
        get => _runtimeValue;
        set
        {
            if (_runtimeValue == value) return;
            _runtimeValue = value;
            OnChanged?.Invoke(value);
        }
    }

    public event Action<bool> OnChanged;

    void OnEnable()
    {
        _runtimeValue = initialValue;
        OnChanged = null;
    }
}
