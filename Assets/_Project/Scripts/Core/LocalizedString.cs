using UnityEngine;

/// <summary>
/// SO holding two text variants (RU / EN).
/// </summary>
[CreateAssetMenu(menuName = "Game/Localization/Localized String", fileName = "LocStr_New")]
public class LocalizedString : ScriptableObject
{
    [TextArea(2, 10)]
    [SerializeField] private string ru;

    [TextArea(2, 10)]
    [SerializeField] private string en;

    /// <summary>Returns text for the given language. Falls back to RU if EN is empty.</summary>
    public string GetText(GameLanguage lang)
    {
        if (lang == GameLanguage.English && !string.IsNullOrEmpty(en))
            return en;
        return ru;
    }
}
