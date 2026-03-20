using UnityEngine;

/// <summary>
/// Configuration for exploration phase timing.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Exploration Config", fileName = "ExplorationConfig")]
public class ExplorationConfig : ScriptableObject
{
    [Tooltip("Decorative countdown timer duration in seconds")]
    [SerializeField] private float decorativeTimerDuration = 30f;
    public float DecorativeTimerDuration => decorativeTimerDuration;
}
