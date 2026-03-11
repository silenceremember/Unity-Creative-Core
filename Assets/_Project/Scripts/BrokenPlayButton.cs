using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Вешается на кнопку Play в главном меню.
/// OnClick → TryPlay() → рассказчик просыпается.
/// Кнопка Settings разблокируется ПОСЛЕ того как рассказчик договорит.
/// </summary>
public class BrokenPlayButton : MonoBehaviour
{
    [Tooltip("NarratorChannel ScriptableObject")]
    public NarratorChannel channel;

    [Tooltip("Последовательность реплик при нажатии на сломанный Play")]
    public DialogueSequence brokenSequence;

    [Tooltip("Повторные нажатия — циклически")]
    public DialogueSequence[] repeatSequences;

    [Header("Блокировка Settings")]
    [Tooltip("Кнопка Settings — заблокирована пока рассказчик не договорит")]
    public Button settingsButton;

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
        // Разблокируем Settings только после первой последовательности
        if (!_settingsUnlocked && completed == brokenSequence)
        {
            _settingsUnlocked = true;
            if (settingsButton != null)
                settingsButton.interactable = true;
        }
    }
}
