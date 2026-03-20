using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Play button in the Settings menu.
/// Narrator speaks → UI locks → screen fades → game starts.
/// </summary>
public class SettingsPlayButton : MonoBehaviour
{
    [SerializeField] private NarratorChannel channel;
    [SerializeField] private IntroCrawl introCrawl;

    [Tooltip("What the narrator says on press")]
    [SerializeField] private DialogueSequence sequence;

    [Header("UI Lock")]
    [Tooltip("Buttons to lock on Play press")]
    [SerializeField] private Button[] buttonsToBlock;

    [Tooltip("Slider to lock on Play press")]
    [SerializeField] private Slider sliderToBlock;

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
        if (introCrawl != null)
            introCrawl.Play();
    }
}
