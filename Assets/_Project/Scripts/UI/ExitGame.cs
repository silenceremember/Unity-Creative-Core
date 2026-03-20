using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Exit game. Bind to the Exit button via OnClick.
/// Locked by default; call Unlock() to enable.
/// </summary>
[RequireComponent(typeof(Button))]
public class ExitGame : MonoBehaviour
{
    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    void Start()
    {
        _button.interactable = false;
    }

    /// <summary>Unlocks the button. Call from Return button's OnClick.</summary>
    public void Unlock()
    {
        _button.interactable = true;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
