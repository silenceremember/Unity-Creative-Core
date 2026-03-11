using UnityEngine;

/// <summary>
/// Кнопка смены языка. Чередует два sequence — с русского на английский и обратно.
/// </summary>
public class LanguageButton : MonoBehaviour
{
    public NarratorChannel channel;

    [Tooltip("RU → EN")]
    public DialogueSequence seqToEnglish;

    [Tooltip("EN → RU")]
    public DialogueSequence seqToRussian;

    // false = сейчас RU, нажатие переключит на EN
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

