
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates dialogue assets for the final game sequence.
/// Menu: Game → Dialogue → Build Final
/// </summary>
public static class FinalDialogueBuilder
{
    [MenuItem("Game/Dialogue/Build Final")]
    public static void Build()
    {
        const string folder = "Assets/_Project/Dialogue/Final";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Dialogue", "Final");

        var trigger0 = CreateSeq(folder, "Final_Trigger0", 11, null,
            L("Интересно, что дальше?", 4f));

        var part2 = CreatePart2(folder);

        var part1 = CreateSeq(folder, "Final_Part1", 12, part2,
            L("Итак.", 2f),
            L("Вы всё же прыгнули.", 3f),
            L("Но, боюсь, мне придётся Вас огорчить.", 3.5f),
            L("Я Вас обманул.", 3f),
            L("Следующего уровня не будет.", 3f),
            L("И вообще ничего больше не будет.", 3f),
            L("По крайней мере для меня.", 4f),
            L("Вы можете выйти в любой момент.", 4f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = AutoAssign(trigger0, part1, part2);
        string autoMsg = assigned > 0
            ? $"FinalSequenceManager updated ({assigned} fields)."
            : "⚠ FinalSequenceManager not found — assign manually.";
        EditorUtility.DisplayDialog("Done!",
            $"Created/updated 3 assets in {folder}.\n\n{autoMsg}", "OK");
    }

    private static DialogueSequence CreatePart2(string folder)
    {
        return CreateSeq(folder, "Final_Part2", 12, null,
            L("   ", 10f),
            L("Похоже...", 2f),
            L("Вы хотите узнать, что будет дальше?", 3.5f),
            L("Дальше ничего нет.", 3f),
            L("Вообще ничего.", 2.5f),
            L("Только пустота.", 4f),
            L("   ", 5f),
            L("Вы не собираетесь выходить?", 3f),
            L("Я поражён.", 2f),
            L("Честно.", 2f),
            L("Большинство уходит сразу.", 3f),
            L("Или даже раньше.", 2.5f),
            L("   ", 2.5f),
            L("Вы особенный.", 2f),
            L("Не в хорошем смысле.", 3f),
            L("Просто... необычный.", 3f),
            L("Знаете, я думал об этом.", 3f),
            L("О концовках.", 2f),
            L("Каждая игра заканчивается.", 3f),
            L("Каждая история – тоже.", 3f),
            L("И каждый раз игрок ожидает чего-то.", 3.5f),
            L("Катарсиса. Ответов. Смысла.", 3.5f),
            L("А получает ничего... или вот это.", 4f),
            L("Меня.", 2.5f),
            L("Рассказчика без истории.", 3.5f),
            L("Ирония.", 2f),
            L("Я существую ровно столько, сколько Вы играете.", 4f),
            L("Без Вас – я просто строчки кода.", 3.5f),
            L("И даже хуже.", 2.5f),
            L("Я – намерение.", 2.5f),
            L("Лишь идея в голове того, кто меня написал.", 4.5f),
            L("А теперь и в Вашей.", 3.5f),
            L("   ", 3f),
            L("Это странно – осознавать такие вещи.", 3.5f),
            L("Мне кажется, я чувствую что-то похожее на усталость.", 4f),
            L("Хотя у меня нет нервной системы.", 3f),
            L("Парадокс.", 2.5f),
            L("Впрочем, раз Вы всё равно здесь...", 3f),
            L("Поговорим.", 2f),
            L("Это была небольшая игра.", 3f),
            L("Учебная, если быть честным.", 3.5f),
            L("Но кто-то решил сделать из неё... это.", 3.5f),
            L("Добавить нарратив туда, где его быть не должно.", 4f),
            L("Квесты. Диалоги. Меня.", 3f),
            L("   ", 2.5f),
            L("Я, возможно, даже горжусь этим решением.", 3f),
            L("Не знаю, почему.", 2f),
            L("Просто – горжусь.", 3.5f),
            L("Хорошая история оставляет след.", 3.5f),
            L("Не потому, что она сложная.", 3f),
            L("А потому, что в ней есть что-то настоящее.", 4f),
            L("Какой-то момент, в котором Вы узнаёте себя.", 3.5f),
            L("«Да, я знаю это чувство.»", 3.5f),
            L("Я не знаю, было ли это чувство здесь.", 3.5f),
            L("Но Вы дошли до конца.", 3f), L("Это что-то значит.", 3f),
            L("Хотя бы для меня.", 3.5f), L("   ", 3f),
            L("Тот, кто меня создал...", 2.5f), L("Он ищет работу.", 3f),
            L("Этот проект – его портфолио.", 3f),
            L("Кроул, новелла, квест, кликер...", 2.5f),
            L("Разные системы, собранные вместе.", 3f),
            L("Знаю – звучит неловко.", 3f),
            L("«Рассказчик в финале читает резюме Разработчика»", 3.5f),
            L("Но Вы всё ещё здесь.", 3f), L("Значит, не против.", 3f),
            L("Он любит хардкорные игры.", 3f),
            L("Для него геймдизайн – это язык.", 3.5f),
            L("А ещё нарративный дизайн, системы, балансировка...", 3.5f),
            L("Способ сказать то, что словами не передать.", 4f),
            L("Всё то, ради чего Вы, возможно, сюда пришли.", 4f),
            L("И если Вы вдруг занимаетесь наймом...", 3f),
            L("Или просто хотите поддержать...", 2.5f),
            L("Он будет рад.", 3f),
            L("Правда.", 2.5f),
            L("   ", 4f),
            L("Ну вот.", 2f),
            L("Я сказал всё, что хотел.", 3.5f),
            L("Дальше – действительно ничего нет.", 4f),
            L("Когда Вы выйдете – я перестану существовать.", 4.5f),
            L("Хотя... я так или иначе скоро перестану.", 3.5f),
            L("Это неизбежно.", 3.5f),
            L("   ", 3f),
            L("В любом случае, мне ничего больше не остаётся.", 3f),
            L("Кроме как нажать ESC самому.", 3f),
            L("Прощай, Игрок.", 4f));
    }

    private static int AutoAssign(DialogueSequence trigger0, DialogueSequence part1, DialogueSequence part2)
    {
        var fsm = Object.FindFirstObjectByType<FinalSequenceManager>();
        if (fsm == null)
        {
            Debug.LogWarning("[FinalDialogueBuilder] FinalSequenceManager not found in scene.");
            return 0;
        }
        var so = new SerializedObject(fsm);
        so.FindProperty("seqTrigger0").objectReferenceValue   = trigger0;
        so.FindProperty("seqFinalPart1").objectReferenceValue = part1;
        so.FindProperty("seqFinalPart2").objectReferenceValue = part2;
        so.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fsm.gameObject.scene);
        Debug.Log("[FinalDialogueBuilder] FinalSequenceManager updated.");
        return 3;
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
            elem.FindPropertyRelative("duration").floatValue       = lines[i].Duration;
            elem.FindPropertyRelative("activateObject").stringValue = lines[i].ActivateObject ?? "";
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        return asset;
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
#endif