using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Выход из игры. Привяжи к кнопке Exit через OnClick.
/// </summary>
public class ExitGame : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // В Editor — останавливаем Play Mode
#else
        Application.Quit();                  // В Build — закрываем приложение
#endif
    }
}
