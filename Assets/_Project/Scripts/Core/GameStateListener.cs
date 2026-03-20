using UnityEngine;

/// <summary>
/// Attach to any GameObject. Activates/deactivates it (or another target)
/// based on the current GameState.
///
/// Inspector setup:
///  1. Select a GameStateChannel
///  2. Specify activeInStates[] — states when this object should be active
///  3. Optionally: set targetOverride to control a different GameObject
/// </summary>
public class GameStateListener : MonoBehaviour
{
    [Tooltip("Game state channel")]
    [SerializeField] private GameStateChannel channel;

    [Tooltip("States in which this object (or target) is active")]
    [SerializeField] private GameState[] activeInStates;

    [Tooltip("If assigned — controls this object; otherwise controls own GameObject")]
    [SerializeField] private GameObject targetOverride;

    private GameObject _target;

    void Awake()
    {
        _target = targetOverride != null ? targetOverride : gameObject;
    }

    void OnEnable()
    {
        if (channel != null)
        {
            channel.OnStateChanged += OnStateChanged;
            Apply(channel.Current);
        }
    }

    void OnDisable()
    {
        if (channel != null)
            channel.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state) => Apply(state);

    private void Apply(GameState state)
    {
        bool shouldBeActive = System.Array.IndexOf(activeInStates, state) >= 0;
        if (_target != null && _target.activeSelf != shouldBeActive)
            _target.SetActive(shouldBeActive);
    }
}
