using UnityEngine;

/// <summary>
/// Language toggle button. Switches LanguageVariable and plays a transition sequence.
/// </summary>
public class LanguageButton : MonoBehaviour
{
    [SerializeField] private LanguageVariable languageVar;
    [SerializeField] private NarratorChannel channel;
    [SerializeField] private DialogueSequence transitionSequence;

    public void OnClick()
    {
        if (languageVar == null) return;

        languageVar.Value = languageVar.Value == GameLanguage.Russian
            ? GameLanguage.English
            : GameLanguage.Russian;

        if (channel != null && transitionSequence != null)
            channel.Raise(transitionSequence);
    }
}
