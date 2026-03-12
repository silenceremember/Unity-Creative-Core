#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Дебаг-хоткеи. Вешается на любой постоянный GameObject в сцене.
/// F1 — пропустить всё (меню + кроул), сразу запустить визуальную новеллу.
/// F2 — перейти напрямую в Gameplay (камера на игрока, управление).
/// </summary>
public class DebugSkip : MonoBehaviour
{
    [Tooltip("SO-канал состояний игры")]
    public GameStateChannel gameStateChannel;

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            SkipToNovel();

        if (Keyboard.current.f2Key.wasPressedThisFrame)
            SkipToGameplay();
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
    }

    void SkipToGameplay()
    {
        // Скрыть кроул если идёт
        if (IntroCrawl.Instance != null && IntroCrawl.Instance.crawlRoot != null)
            IntroCrawl.Instance.crawlRoot.SetActive(false);

        // Скрыть новеллу если открыта
        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.ForceHideNovelCanvas();

        // Переходим в Gameplay — GameplaySetup всё сделает сам
        gameStateChannel?.Raise(GameState.Gameplay);
        Debug.Log("[DebugSkip] F2 → Gameplay");
    }
}
#endif
