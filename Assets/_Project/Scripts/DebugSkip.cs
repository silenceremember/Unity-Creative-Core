#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Дебаг-хоткеи.
/// F1 — пропустить всё (меню + кроул), сразу запустить визуальную новеллу.
/// F2 — перейти напрямую в Gameplay (камера на игрока, управление).
/// F3 — то же что F2, но сразу + квест с картинами.
/// O  — мгновенно выполнить квест картин (accept).
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

        if (Keyboard.current.f3Key.wasPressedThisFrame)
            SkipToQuest();

        if (Keyboard.current.oKey.wasPressedThisFrame)
            CompleteQuest();
    }

    void SkipToNovel()
    {
        if (IntroCrawl.Instance != null && IntroCrawl.Instance.crawlRoot != null)
            IntroCrawl.Instance.crawlRoot.SetActive(false);

        gameStateChannel?.Raise(GameState.VisualNovel);

        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.StartNovel();
        else
            Debug.LogWarning("[DebugSkip] VisualNovelManager.Instance is null!");

        Debug.Log("[DebugSkip] F1 → VisualNovel");
    }

    void SkipToGameplay()
    {
        if (IntroCrawl.Instance != null && IntroCrawl.Instance.crawlRoot != null)
            IntroCrawl.Instance.crawlRoot.SetActive(false);

        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.ForceHideNovelCanvas();

        gameStateChannel?.Raise(GameState.Gameplay);
        Debug.Log("[DebugSkip] F2 → Gameplay");
    }

    void SkipToQuest()
    {
        // Шаг 1: Gameplay — активирует камеру, игрока, ExplorationCanvas
        SkipToGameplay();

        // Шаг 2: Quest — GameStateListener включит NarratorCanvas и QuestCanvas
        gameStateChannel?.Raise(GameState.Quest);

        // Останавливаем нарратора (ambient-цепочку) — квест стартует чисто
        if (NarratorManager.Instance != null)
            NarratorManager.Instance.Stop();

        // Наклоняем картины — GO может быть неактивным, ищем включая неактивные
        var shiftController = FindFirstObjectByType<PaintingShiftController>(FindObjectsInactive.Include);
        if (shiftController != null)
        {
            shiftController.gameObject.SetActive(true);
            shiftController.ForceShift();
        }
        else
            Debug.LogWarning("[DebugSkip] PaintingShiftController not found!");

        // Запускаем квест
        if (PaintingQuestManager.Instance != null)
            PaintingQuestManager.Instance.StartQuest();
        else
            Debug.LogWarning("[DebugSkip] PaintingQuestManager.Instance is null!");

        Debug.Log("[DebugSkip] F3 → Quest");
    }

    void CompleteQuest()
    {
        if (PaintingQuestManager.Instance == null)
        {
            Debug.LogWarning("[DebugSkip] PaintingQuestManager.Instance is null — сначала F3.");
            return;
        }

        PaintingQuestManager.Instance.DebugCompleteQuest();
        Debug.Log("[DebugSkip] O → Quest complete");
    }
}
#endif
