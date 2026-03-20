using System;
using UnityEngine;

/// <summary>
/// Game state SO event bus.
/// </summary>
[CreateAssetMenu(menuName = "Game/Channels/State Channel", fileName = "GameStateChannel")]
public class GameStateChannel : ScriptableObject
{
    [NonSerialized] private GameState _current = GameState.Menu;
    public GameState Current => _current;

    public event Action<GameState> OnStateChanged;

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
    }
}
