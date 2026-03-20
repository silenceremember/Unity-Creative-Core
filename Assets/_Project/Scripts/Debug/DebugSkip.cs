#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug hotkeys.
/// F1 — skip everything (menu + crawl), start visual novel directly.
/// F2 — jump to Gameplay (camera on player, controls enabled).
/// F3 — same as F2 but also starts the painting quest.
/// O  — instantly complete the painting quest (accept).
/// </summary>
public class DebugSkip : MonoBehaviour
{
    [Tooltip("Game state SO channel")]
    [SerializeField] private GameStateChannel gameStateChannel;

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
    }

    void SkipToGameplay()
    {
        if (IntroCrawl.Instance != null && IntroCrawl.Instance.crawlRoot != null)
            IntroCrawl.Instance.crawlRoot.SetActive(false);

        if (VisualNovelManager.Instance != null)
            VisualNovelManager.Instance.ForceHideNovelCanvas();

        gameStateChannel?.Raise(GameState.Gameplay);
    }

    void SkipToQuest()
    {
        SkipToGameplay();

        gameStateChannel?.Raise(GameState.Quest);

        if (NarratorManager.Instance != null)
            NarratorManager.Instance.Stop();

        var shiftController = FindFirstObjectByType<PaintingShiftController>(FindObjectsInactive.Include);
        if (shiftController != null)
        {
            shiftController.gameObject.SetActive(true);
            shiftController.ForceShift();
        }

        if (PaintingQuestManager.Instance != null)
            PaintingQuestManager.Instance.StartQuest();

        if (ExplorationManager.Instance != null)
            ExplorationManager.Instance.DebugShowTimerAndClicker();
    }

    void CompleteQuest()
    {
        if (PaintingQuestManager.Instance == null) return;
        PaintingQuestManager.Instance.DebugCompleteQuest();
    }
}
#endif
