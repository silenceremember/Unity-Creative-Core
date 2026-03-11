using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Кнопка Play в меню Настроек.
/// Рассказчик говорит → блокирует UI → экран темнеет → игра начинается.
/// </summary>
public class SettingsPlayButton : MonoBehaviour
{
    [Tooltip("NarratorChannel ScriptableObject")]
    public NarratorChannel channel;

    [Tooltip("Что говорит рассказчик при нажатии")]
    public DialogueSequence sequence;

    [Header("Блокировка UI")]
    [Tooltip("Кнопки которые заблокируются при нажатии Play")]
    public Button[] buttonsToBlock;

    [Tooltip("Слайдер который заблокируется при нажатии Play")]
    public Slider sliderToBlock;

    private bool _triggered = false;

    void OnEnable()
    {
        if (channel != null)
            channel.OnSequenceCompleted += OnNarratorDone;
    }

    void OnDisable()
    {
        if (channel != null)
            channel.OnSequenceCompleted -= OnNarratorDone;
    }

    public void TryStart()
    {
        if (_triggered) return;
        _triggered = true;

        // Блокируем UI
        if (buttonsToBlock != null)
            foreach (var btn in buttonsToBlock)
                if (btn != null) btn.interactable = false;

        if (sliderToBlock != null)
            sliderToBlock.interactable = false;

        channel?.Raise(sequence);
    }

    private void OnNarratorDone(DialogueSequence completed)
    {
        if (completed != sequence) return;

        // Рассказчик договорил — запускаем кроул
        if (IntroCrawl.Instance != null)
            IntroCrawl.Instance.Play();
    }
}
