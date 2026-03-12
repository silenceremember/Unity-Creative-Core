using System;
using UnityEngine;

/// <summary>
/// SO-шина состояний игры.
/// Создай через ПКМ → Create → GameState → State Channel
/// </summary>
[CreateAssetMenu(menuName = "GameState/State Channel", fileName = "GameStateChannel")]
public class GameStateChannel : ScriptableObject
{
    // NonSerialized — не сохраняется между сессиями Play Mode
    [NonSerialized] private GameState _current = GameState.Menu;
    public GameState Current => _current;

    public event Action<GameState> OnStateChanged;

    // Вызывается при старте Play Mode — сбрасывает состояние и старые подписки
    void OnEnable()
    {
        _current = GameState.Menu;
        OnStateChanged = null;
    }

    public void Raise(GameState newState)
    {
        if (_current == newState) return;
        _current = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameState] → {newState}");
    }
}
