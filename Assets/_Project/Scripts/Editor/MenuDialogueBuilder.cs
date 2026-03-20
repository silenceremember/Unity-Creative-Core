#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates all DialogueSequence assets for the main menu narrator reactions.
/// Menu: Game → Dialogue → Build Menu Dialogues
/// </summary>
public static class MenuDialogueBuilder
{
    [MenuItem("Game/Dialogue/Build Menu Dialogues")]
    public static void Build()
    {
        const string folder = "Assets/_Project/SO/Dialogue/Menu";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/SO/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project/SO", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/SO/Dialogue", "Menu");


        CreateSeq(folder, "Seq_BrokenPlay", 1, null,
            L("О. Привет.",                                 0.8f),
            L("Ты нажал Играть.",                          1.0f),
            L("Это не сработает. Игра не закончена.",      1.3f),
            L("И вряд ли будет...",                        1.0f),
            L("Хотя... незавершённость придаёт шарм.",     1.0f),
            L("Наверное.",                                  0.8f),
            L("Попробуй поискать в Настройках.",           0.0f));

        CreateSeq(folder, "Seq_BrokenPlay_Repeat", 0, null,
            L("Всё ещё не работает.", 1.0f));

        CreateSeq(folder, "Seq_TVOn", 2, null,
            L("Телевизор включён.",            0.8f),
            L("Программа — атмосферный шум.",  1.0f),
            L("Жильцы скорее всего довольны.", 0f));

        CreateSeq(folder, "Seq_TVOff", 2, null,
            L("Телевизор выключен.",            0.8f),
            L("Жильцы чуть менее счастливы.",   1.0f),
            L("Может, и нет.",                  0f));

        CreateSeq(folder, "Seq_LangToEnglish", 2, null,
            L("Переключаемся.",                 0.8f),
            L("Сейчас я скажу...",              1.0f),
            L("Oh. That actually worked.",      1.0f),
            L("I sound different in English.",  1.2f),
            L("More professional. Less me.",    1.0f),
            L("Anyway.",                        0f));

        CreateSeq(folder, "Seq_LangToRussian", 2, null,
            L("Switching language.",            0.8f),
            L("Back to Russian. Here goes.",    1.2f),
            L("О. Это снова я.",               1.0f),
            L("По-русски звучу по-другому.",    1.0f),
            L("Гораздо менее профессионально.", 1.0f),
            L("По крайней мере, честно.",       0f));

        CreateSeq(folder, "Seq_SettingsPlay", 3, null,
            L("О, отлично. Ты нашёл ещё одну кнопку.", 1.4f),
            L("Похоже, она тоже не работает.",         1.0f),
            L("Разработчик совсем не старался.",       1.0f),
            L("Ладно. Я запущу нас сам.",              0f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Build Menu Dialogues",
            $"Assets updated in {folder}.", "OK");
    }

    private static DialogueSequence CreateSeq(string folder, string name, int priority,
        DialogueSequence next, params DialogueLine[] lines)
    {
        string path = $"{folder}/{name}.asset";
        var asset = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialogueSequence>();
            AssetDatabase.CreateAsset(asset, path);
        }

        var so = new SerializedObject(asset);
        so.FindProperty("priority").intValue                 = priority;
        so.FindProperty("nextSequence").objectReferenceValue = next;

        var linesProp = so.FindProperty("lines");
        linesProp.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            var elem = linesProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("text").stringValue          = lines[i].Text;
            elem.FindPropertyRelative("pauseAfter").floatValue     = lines[i].PauseAfter;
            elem.FindPropertyRelative("activateObject").stringValue = lines[i].ActivateObject ?? "";
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static DialogueLine L(string text, float pause, string activateObject = "")
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            text = text,
            pauseAfter = pause,
            activateObject = activateObject
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }
}
#endif
