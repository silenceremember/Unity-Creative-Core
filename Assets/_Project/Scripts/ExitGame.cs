using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Выход из игры. Привяжи к кнопке Exit через OnClick.
/// По умолчанию заблокирована; вызови Unlock() чтобы включить.
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

    /// <summary>Разблокирует кнопку. Вызывай из OnClick кнопки Return.</summary>
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
