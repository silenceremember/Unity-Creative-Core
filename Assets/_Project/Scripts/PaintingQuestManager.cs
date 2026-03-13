using UnityEngine;

/// <summary>
/// Менеджер квеста «Поправить картины».
/// Пока заглушка — полная реализация будет после exploration-фазы.
/// </summary>
public class PaintingQuestManager : MonoBehaviour
{
    public static PaintingQuestManager Instance { get; private set; }

    void Awake() => Instance = this;

    /// <summary>Запускается из ExplorationManager по истечении таймера.</summary>
    public void StartQuest()
    {
        Debug.Log("[PaintingQuestManager] Quest started (stub).");
        // TODO: показать HUD, активировать картины
    }
}
