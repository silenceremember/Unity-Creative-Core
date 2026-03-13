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
        CreateSeqTVOn();
        CreateSeqTVOff();
        CreateSeqLangToEnglish();
        CreateSeqLangToRussian();
        CreateSeqSettingsPlay();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DialogueAssetFactory] Done: " + FOLDER);
    }

    // ──────────────────────────────────────────────

    private static void CreateSeqBrokenPlay()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_BrokenPlay");
        seq.priority = 1;

        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "О. Привет.",                                 pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Ты нажал Играть.",                          pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Это не сработает. Игра не закончена.",      pauseAfter = 1.3f },
            new DialogueLine { speaker = "Narrator", text = "И вряд ли будет...",                        pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Хотя... незавершённость придаёт шарм.",     pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Наверное.",                                  pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Попробуй поискать в Настройках.",           pauseAfter = 0.0f },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqBrokenRepeat()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_BrokenPlay_Repeat");
        seq.priority = 0;

        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "Всё ещё не работает.", pauseAfter = 1.0f },
        };
        EditorUtility.SetDirty(seq);
    }


    private static T CreateAsset<T>(string assetName) where T : ScriptableObject
    {
        string path = $"{FOLDER}/{assetName}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            // Перезаписываем данные — GUID сохраняется, ссылки в сцене не ломаются
            var fresh = ScriptableObject.CreateInstance<T>();
            EditorUtility.CopySerialized(fresh, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(fresh);
            return existing;
        }

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    // ── Settings sequences ────────────────────

    private static void CreateSeqTVOn()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_TVOn");
        seq.priority = 2;
        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "Телевизор включён.",            pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Программа — атмосферный шум.",  pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Жильцы скорее всего довольны.", pauseAfter = 0f   },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqTVOff()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_TVOff");
        seq.priority = 2;
        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "Телевизор выключен.",            pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Жильцы чуть менее счастливы.",   pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Может, и нет.",                  pauseAfter = 0f   },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqLangToEnglish()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_LangToEnglish");
        seq.priority = 2;
        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "Переключаемся.",                 pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Сейчас я скажу...",              pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Oh. That actually worked.",      pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "I sound different in English.",  pauseAfter = 1.2f },
            new DialogueLine { speaker = "Narrator", text = "More professional. Less me.",    pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Anyway.",                        pauseAfter = 0f   },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqLangToRussian()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_LangToRussian");
        seq.priority = 2;
        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "Switching language.",            pauseAfter = 0.8f },
            new DialogueLine { speaker = "Narrator", text = "Back to Russian. Here goes.",    pauseAfter = 1.2f },
            new DialogueLine { speaker = "Narrator", text = "О. Это снова я.",               pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "По-русски звучу по-другому.",    pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Гораздо менее профессионально.", pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "По крайней мере, честно.",       pauseAfter = 0f   },
        };
        EditorUtility.SetDirty(seq);
    }

    private static void CreateSeqSettingsPlay()
    {
        var seq = CreateAsset<DialogueSequence>("Seq_SettingsPlay");
        seq.priority = 3;
        seq.lines = new DialogueLine[]
        {
            new DialogueLine { speaker = "Narrator", text = "О, отлично. Ты нашёл ещё одну кнопку.", pauseAfter = 1.4f },
            new DialogueLine { speaker = "Narrator", text = "Похоже, она тоже не работает.",         pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Разработчик совсем не старался.",       pauseAfter = 1.0f },
            new DialogueLine { speaker = "Narrator", text = "Ладно. Я запущу нас сам.",              pauseAfter = 0f   },
        };
        EditorUtility.SetDirty(seq);
    }
}