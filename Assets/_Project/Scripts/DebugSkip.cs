#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Дебаг-хоткеи. Вешается на любой постоянный GameObject в сцене.
/// F1 — пропустить всё (меню + кроул), сразу запустить визуальную новеллу.
/// </summary>
public class DebugSkip : MonoBehaviour
{
    [Tooltip("SO-канал состояний игры")]
    public GameStateChannel gameStateChannel;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            SkipToNovel();
    }

    void SkipToNovel()
    {
        // Останавливаем кроул если он идёт
        if (IntroCrawl.Instance != null && IntroCrawl.Instance.crawlRoot != null)
            IntroCrawl.Instance.crawlRoot.SetActive(false);

        // Переключаем состояние — GameStateListener скроет меню автоматически
        gameStateChannel?.Raise(GameState.VisualNovel);

        // Запускаем новеллу напрямую
        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.StartNovel();
        else
            Debug.LogWarning("[DebugSkip] VisualNovelManager.Instance is null!");

        Debug.Log("[DebugSkip] F1 → VisualNovel");
        enabled = false;
    }
}
#endif
