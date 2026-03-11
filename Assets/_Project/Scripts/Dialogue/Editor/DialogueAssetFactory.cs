using UnityEditor;
using UnityEngine;

/// <summary>
/// Запусти через меню: Tools → Create Dialogue Assets
/// Создаёт все стартовые .asset файлы в Assets/_Project/Dialogue/
/// </summary>
public static class DialogueAssetFactory
{
    private const string FOLDER = "Assets/_Project/Dialogue";

    [MenuItem("Tools/Create Dialogue Assets")]
    public static void CreateAll()
    {
        // NarratorChannel
        CreateAsset<NarratorChannel>("NarratorChannel");

        CreateSeqBrokenPlay();
        CreateSeqBrokenRepeat();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DialogueAssetFactory] Done: " + FOLDER);
    }

    [MenuItem("Tools/Update Seq_BrokenPlay")]
    public static void UpdateBrokenPlay()
    {
        CreateSeqBrokenPlay();
        CreateSeqBrokenRepeat();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Factory] Seq_BrokenPlay updated.");
    }

    // ──────────────────────────────────────────────

    private static void CreateSeqBrokenPlay()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_BrokenPlay");

        seq.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "О. Привет.",
                pauseAfter = 0.8f
            },
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Ты нажал Играть.",
                pauseAfter = 0.6f
            },
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Это не сработает. Игра не закончена.",
                pauseAfter = 0.6f
            },
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Хотя... незавершённость придаёт шарм.",
                pauseAfter = 0.5f
            },
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Наверное.",
                pauseAfter = 0.8f
            },
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Попробуй поискать в Настройках.",
                pauseAfter = 0.0f
            },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqBrokenRepeat()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_BrokenPlay_Repeat");

        seq.lines = new DialogueLine[]
        {
            new DialogueLine
            {
                speaker    = "Narrator",
                text       = "Всё ещё не работает. И вряд-ли будет.",
                pauseAfter = 0.5f
            },
        };
        EditorUtility.SetDirty(seq);
    }


    private static T CreateAsset<T>(string assetName) where T : ScriptableObject
    {
        string path = $"{FOLDER}/{assetName}.asset";

        // Не перезаписываем если уже есть
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            Debug.Log($"[Factory] Already exists, skipping: {path}");
            return existing;
        }

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}