using TMPro;
using UnityEngine;

/// <summary>
/// Listens to LanguageVariable and updates TMP text from LocalizedString.
/// Attach to any TMP_Text component.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedLabel : MonoBehaviour
{
    [SerializeField] private LocalizedString localizedString;
    [SerializeField] private LanguageVariable languageVar;

    private TMP_Text _text;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (languageVar != null)
            languageVar.OnChanged += OnLanguageChanged;
        Refresh();
    }

    void OnDisable()
    {
        if (languageVar != null)
            languageVar.OnChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(GameLanguage lang) => Refresh();

    private void Refresh()
    {
        if (_text == null || localizedString == null || languageVar == null) return;
        _text.text = localizedString.GetText(languageVar.Value);
    }
}
