using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to the Play button in the main menu.
/// OnClick → TryPlay() → narrator wakes up.
/// Settings button is unlocked AFTER the narrator finishes speaking.
/// </summary>
public class BrokenPlayButton : MonoBehaviour
{
    [SerializeField] private NarratorChannel channel;

    [Tooltip("Dialogue sequence when pressing broken Play")]
    [SerializeField] private DialogueSequence brokenSequence;

    [Tooltip("Repeat presses — cycled")]
    [SerializeField] private DialogueSequence[] repeatSequences;

    [Header("Settings Lock")]
    [Tooltip("Settings button — locked until narrator finishes")]
    [SerializeField] private Button settingsButton;

    private int _pressCount = 0;
    private bool _settingsUnlocked = false;

    void Start()
    {
        if (settingsButton != null)
            settingsButton.interactable = false;
    }

    void OnEnable()
    {
        if (channel != null)
            channel.OnSequenceCompleted += OnSequenceFinished;
    }

    void OnDisable()
    {
        if (channel != null)
            channel.OnSequenceCompleted -= OnSequenceFinished;
    }

    public void TryPlay()
    {
        if (channel == null) return;

        if (_pressCount == 0 || repeatSequences == null || repeatSequences.Length == 0)
            channel.Raise(brokenSequence);
        else
        {
            int idx = (_pressCount - 1) % repeatSequences.Length;
            channel.Raise(repeatSequences[idx]);
        }

        _pressCount++;
    }

    private void OnSequenceFinished(DialogueSequence completed)
    {
        if (!_settingsUnlocked && completed == brokenSequence)
        {
            _settingsUnlocked = true;
            if (settingsButton != null)
                settingsButton.interactable = true;
        }
    }
}
