using UnityEditor;
using UnityEngine;

/// <summary>
/// Run via menu: Game → Dialogue → Build Menu Dialogues
/// Creates all starting .asset files in Assets/_Project/Dialogue/
/// </summary>
public static class DialogueAssetFactory
{
    private const string FOLDER = "Assets/_Project/Dialogue";

    [MenuItem("Game/Dialogue/Build Menu Dialogues")]
    public static void CreateAll()
    {
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

    private static void CreateSeqBrokenPlay()
    {
        CreateSeq("Seq_BrokenPlay", 1,
            L("О. Привет.",                                 0.8f),
            L("Ты нажал Играть.",                          1.0f),
            L("Это не сработает. Игра не закончена.",      1.3f),
            L("И вряд ли будет...",                        1.0f),
            L("Хотя... незавершённость придаёт шарм.",     1.0f),
            L("Наверное.",                                  0.8f),
            L("Попробуй поискать в Настройках.",           0.0f));
    }

    private static void CreateSeqBrokenRepeat()
    {
        CreateSeq("Seq_BrokenPlay_Repeat", 0,
            L("Всё ещё не работает.", 1.0f));
    }


    private static T CreateAsset<T>(string assetName) where T : ScriptableObject
    {
        string path = $"{FOLDER}/{assetName}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            // Overwrite data — GUID preserved, scene refs stay intact
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

    private static void CreateSeqTVOn()
    {
        CreateSeq("Seq_TVOn", 2,
            L("Телевизор включён.",            0.8f),
            L("Программа — атмосферный шум.",  1.0f),
            L("Жильцы скорее всего довольны.", 0f));
    }

    private static void CreateSeqTVOff()
    {
        CreateSeq("Seq_TVOff", 2,
            L("Телевизор выключен.",            0.8f),
            L("Жильцы чуть менее счастливы.",   1.0f),
            L("Может, и нет.",                  0f));
    }

    private static void CreateSeqLangToEnglish()
    {
        CreateSeq("Seq_LangToEnglish", 2,
            L("Переключаемся.",                 0.8f),
            L("Сейчас я скажу...",              1.0f),
            L("Oh. That actually worked.",      1.0f),
            L("I sound different in English.",  1.2f),
            L("More professional. Less me.",    1.0f),
            L("Anyway.",                        0f));
    }

    private static void CreateSeqLangToRussian()
    {
        CreateSeq("Seq_LangToRussian", 2,
            L("Switching language.",            0.8f),
            L("Back to Russian. Here goes.",    1.2f),
            L("О. Это снова я.",               1.0f),
            L("По-русски звучу по-другому.",    1.0f),
            L("Гораздо менее профессионально.", 1.0f),
            L("По крайней мере, честно.",       0f));
    }

    private static void CreateSeqSettingsPlay()
    {
        CreateSeq("Seq_SettingsPlay", 3,
            L("О, отлично. Ты нашёл ещё одну кнопку.", 1.4f),
            L("Похоже, она тоже не работает.",         1.0f),
            L("Разработчик совсем не старался.",       1.0f),
            L("Ладно. Я запущу нас сам.",              0f));
    }

    private static void CreateSeq(string assetName, int priority, params DialogueLine[] lines)
    {
        string path = $"{FOLDER}/{assetName}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueSequence>();
            AssetDatabase.CreateAsset(asset, path);
        }

        var so = new SerializedObject(asset);
        so.FindProperty("priority").intValue = priority;

        var linesProp = so.FindProperty("lines");
        linesProp.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            var elem = linesProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("text").stringValue          = lines[i].Text;
            elem.FindPropertyRelative("pauseAfter").floatValue     = lines[i].PauseAfter;
            elem.FindPropertyRelative("duration").floatValue       = lines[i].Duration;
            elem.FindPropertyRelative("activateObject").stringValue = lines[i].ActivateObject ?? "";
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
    }

    private static DialogueLine L(string text, float pause)
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            text = text,
            pauseAfter = pause,
            duration = 0f,
            activateObject = ""
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }

    [System.Serializable]
    private struct DialogueLineData
    {
        public string text;
        public float duration;
        public float pauseAfter;
        public string activateObject;
    }
}