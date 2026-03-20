using UnityEngine;

/// <summary>
/// Language toggle button. Alternates between two sequences — RU to EN and back.
/// </summary>
public class LanguageButton : MonoBehaviour
{
    [SerializeField] private NarratorChannel channel;

    [Tooltip("RU → EN")]
    [SerializeField] private DialogueSequence seqToEnglish;

    [Tooltip("EN → RU")]
    [SerializeField] private DialogueSequence seqToRussian;

    private bool _isRussian = true;

    public void OnClick()
    {
        if (_isRussian)
            channel?.Raise(seqToEnglish);
        else
            channel?.Raise(seqToRussian);

        _isRussian = !_isRussian;
    }
}
