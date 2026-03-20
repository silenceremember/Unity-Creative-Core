using UnityEngine;

/// <summary>
/// Single dialogue line. Serialized inline inside DialogueSequence.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 6)]
    [Tooltip("Line text (RU)")]
    [SerializeField] private string text;

    [TextArea(2, 6)]
    [Tooltip("Line text (EN)")]
    [SerializeField] private string textEn;

    [Tooltip("Pause before next line")]
    [SerializeField] private float pauseAfter = 0.3f;

    [Tooltip("GameObject name to SetActive(true) at the start of this line (empty = skip)")]
    [SerializeField] private string activateObject = "";

    public string Text => text;
    public string TextEn => textEn;
    public float PauseAfter => pauseAfter;
    public string ActivateObject => activateObject;

    /// <summary>Returns text for the given language. Falls back to RU if EN is empty.</summary>
    public string GetText(GameLanguage lang)
    {
        if (lang == GameLanguage.English && !string.IsNullOrEmpty(textEn))
            return textEn;
        return text;
    }

    /// <summary>Auto-calculated display duration based on resolved text length.</summary>
    public float GetDuration(GameLanguage lang) => Mathf.Max(1.5f, GetText(lang).Length / 50f);

    /// <summary>Auto-calculated display duration based on RU text length.</summary>
    public float GetDuration() => Mathf.Max(1.5f, text.Length / 50f);
}
