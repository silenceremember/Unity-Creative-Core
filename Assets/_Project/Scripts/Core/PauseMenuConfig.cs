using UnityEngine;

/// <summary>
/// Configuration for the pause menu.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Pause Menu Config", fileName = "PauseMenuConfig")]
public class PauseMenuConfig : ScriptableObject
{
    [Header("Blocked States")]
    [Tooltip("GameStates where ESC is blocked")]
    [SerializeField] private GameState[] blockedStates = { GameState.Menu, GameState.IntroCrawl, GameState.Final };
    public GameState[] BlockedStates => blockedStates;
}
