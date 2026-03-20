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

    [Header("Dependencies")]
    [SerializeField] private IntroCrawl introCrawl;
    [SerializeField] private VisualNovelManager visualNovelManager;
    [SerializeField] private NarratorManager narratorManager;
    [SerializeField] private PaintingQuestManager paintingQuestManager;
    [SerializeField] private ExplorationManager explorationManager;

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
        if (introCrawl != null)
            introCrawl.HideCrawl();

        gameStateChannel?.Raise(GameState.VisualNovel);

        if (visualNovelManager != null)
            visualNovelManager.StartNovel();
    }

    void SkipToGameplay()
    {
        if (introCrawl != null)
            introCrawl.HideCrawl();

        if (visualNovelManager != null)
            visualNovelManager.ForceHideNovelCanvas();

        gameStateChannel?.Raise(GameState.Gameplay);
    }

    void SkipToQuest()
    {
        SkipToGameplay();

        gameStateChannel?.Raise(GameState.Quest);

        if (narratorManager != null)
            narratorManager.Stop();

        var shiftController = FindFirstObjectByType<PaintingShiftController>(FindObjectsInactive.Include);
        if (shiftController != null)
        {
            shiftController.gameObject.SetActive(true);
            shiftController.ForceShift();
        }

        if (paintingQuestManager != null)
            paintingQuestManager.StartQuest();

        if (explorationManager != null)
            explorationManager.DebugShowTimerAndClicker();
    }

    void CompleteQuest()
    {
        if (paintingQuestManager == null) return;
        paintingQuestManager.DebugCompleteQuest();
    }
}
#endif
