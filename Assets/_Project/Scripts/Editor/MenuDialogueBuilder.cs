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
            L("Oh. Hello.", "О. Привет.", 0.8f),
            L("You pressed 'Play'.", "Вы нажали «Играть».", 1.0f),
            L("It won't work. The game isn't finished.", "Это не сработает. Игра не закончена.", 1.5f),
            L("And it probably never will be...", "И вряд ли когда-нибудь будет...", 1.2f),
            L("Although nowadays they call it 'Early Access'.", "Хотя сейчас это называют «Ранний доступ».", 1.8f),
            L("So it's all up to industry standards.", "Так что всё по стандартам индустрии.", 1.5f),
            L("Try poking around in the Settings.", "Попробуйте покопаться в Настройках.", 0.0f));

        CreateSeq(folder, "Seq_BrokenPlay_Repeat", 0, null,
            L("Still doesn't work. Told you.", "Всё ещё не работает. Я же говорил.", 1.2f));

        CreateSeq(folder, "Seq_TVOn", 2, null,
            L("The TV is on.", "Телевизор включён.", 0.8f),
            L("The tenants are most likely pleased.", "Жильцы, скорее всего, довольны.", 0f));

        CreateSeq(folder, "Seq_TVOff", 2, null,
            L("The TV is off.", "Телевизор выключен.", 0.8f),
            L("The tenants are slightly less happy.", "Жильцы чуть менее счастливы.", 1.0f),
            L("Or maybe not.", "Может, и нет.", 0f));

        CreateSeq(folder, "Seq_LangTransition", 2, null,
            L("Переключаюсь...", "Switching...", 0.8f),
            L("Сейчас я скажу...", "Here goes...", 1.0f),
            L("That actually worked.", "Эти родные депрессивные интонации.", 1.5f),
            L("I sound like a default Unity asset.", "Снова звучу как дилетант.", 1.2f),
            L("More professional. Completely soulless.", "Зато искренне.", 1.0f),
            L("Anyway.", "Наверное.", 0f));

        CreateSeq(folder, "Seq_SettingsPlay", 3, null,
            L("Great. You found another button.", "Отлично. Вы нашли ещё одну кнопку.", 1.4f),
            L("But it doesn't seem to work either.", "Но, похоже, она тоже не работает.", 1.0f),
            L("The Developer didn't try very hard.", "Разработчик совсем не старался.", 1.0f),
            L("Fine. I'll launch us myself.", "Ладно. Я запущу нас сам.", 0f));

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
            elem.FindPropertyRelative("textEn").stringValue        = lines[i].TextEn ?? "";
            elem.FindPropertyRelative("text").stringValue          = lines[i].Text;
            elem.FindPropertyRelative("pauseAfter").floatValue     = lines[i].PauseAfter;
            elem.FindPropertyRelative("activateObject").stringValue = lines[i].ActivateObject ?? "";
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static DialogueLine L(string en, string ru, float pause, string activateObject = "")
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            textEn = en,
            text = ru,
            pauseAfter = pause,
            activateObject = activateObject
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }
}
#endif
