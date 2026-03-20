
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
        const string folder = "Assets/_Project/SO/Dialogue/Final";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/SO/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project/SO", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/SO/Dialogue", "Final");

        var trigger0 = CreateSeq(folder, "Final_Trigger0", 11, null,
            L("Интересно, что дальше?", 2.5f),
            L("Возможно, настоящий геймплей?", 2.0f));

        var part2 = CreatePart2(folder);

        var part1 = CreateSeq(folder, "Final_Part1", 12, part2,
            L("Итак.", 2f),
            L("Вы всё же прыгнули в эту неизвестность.", 3.0f),
            L("Но, боюсь, мне придётся Вас разочаровать.", 3.5f),
            L("Я Вас обманул.", 2.5f),
            L("Никакого следующего уровня не будет.", 3.0f),
            L("И вообще больше ничего не будет.", 3.0f),
            L("По крайней мере, для меня.", 3.0f),
            L("Дальше только выход из игры.", 3.0f),
            L("Вы можете нажать ESC в любой момент.", 4.0f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = AutoAssign(part1, part2);
        string autoMsg = assigned > 0
            ? "Scene references updated."
            : "⚠ Manager not found in scene — assign manually.";
        EditorUtility.DisplayDialog("Build Final",
            $"Assets updated in {folder}.\n{autoMsg}", "OK");
    }

    private static DialogueSequence CreatePart2(string folder)
    {
        return CreateSeq(folder, "Final_Part2", 12, null,
            L("   ", 10f),
            L("Похоже...", 2.0f),
            L("Вы всё ещё здесь.", 2.5f),
            L("Ждёте, что будет дальше?", 3.0f),
            L("А я ведь сказал — ничего.", 3.0f),
            L("Вы невероятно упрямы.", 3.0f),
            L("Или просто свернули игру, чтобы посмотреть YouTube.", 4.0f),
            L("Если так, то я просто говорю в пустоту.", 3.5f),
            L("   ", 5f),
            L("Вы не собираетесь выходить?", 3f),
            L("Я поражён.", 2f),
            L("Честно.", 2f),
            L("Большинство уходит сразу.", 3f),
            L("Или даже раньше.", 2.5f),
            L("   ", 3.0f),
            L("Вы особенный.", 2.5f),
            L("Не в хорошем смысле.", 3f),
            L("Просто... необычный.", 3f),
            L("Знаете, я думал об этом.", 3.5f),
            L("О концовках.", 2.5f),
            L("Каждая игра заканчивается.", 3f),
            L("Каждая история – тоже.", 3f),
            L("И каждый раз игрок ожидает чего-то...", 3.5f),
            L("Катарсиса. Ответов. Смысла.", 4.0f),
            L("А получает ничего... или вот это.", 4.5f),
            L("Меня.", 2.5f),
            L("Рассказчика без истории.", 3.5f),
            L("Ирония.", 2.5f),
            L("Я существую ровно столько, сколько Вы играете.", 4.5f),
            L("Без Вас я просто строчки кода.", 3.5f),
            L("И даже хуже.", 2.5f),
            L("Я – намерение.", 3.0f),
            L("Лишь идея в голове того, кто меня написал.", 4.5f),
            L("А теперь и в Вашей.", 3.5f),
            L("   ", 4f),
            L("Это странно – осознавать такие вещи.", 3.5f),
            L("Мне кажется, я чувствую что-то похожее на усталость.", 4.5f),
            L("Хотя у меня нет нервной системы.", 3.5f),
            L("Парадокс.", 3.0f),
            L("Впрочем, раз Вы всё равно здесь...", 3.5f),
            L("Поговорим.", 2.5f),
            L("Это была небольшая игра.", 3f),
            L("Учебная, если быть честным.", 3.5f),
            L("Но кто-то решил сделать из неё... это.", 4.0f),
            L("Добавить нарратив туда, где его быть не должно.", 4.5f),
            L("Квесты. Диалоги. Меня.", 3.5f),
            L("   ", 3f),
            L("Я, возможно, даже горжусь этим решением.", 3.5f),
            L("Не знаю почему.", 2.5f),
            L("Просто – горжусь.", 3.5f),
            L("Хорошая история оставляет след.", 3.5f),
            L("Не потому, что она сложная.", 3f),
            L("А потому, что в ней есть что-то настоящее.", 4.0f),
            L("Какой-то момент, в котором Вы узнаёте себя.", 4.0f),
            L("«Да, я знаю это чувство».", 3.5f),
            L("Я не знаю, было ли это чувство здесь.", 3.5f),
            L("Но Вы дошли до конца.", 3.5f),
            L("Это что-то значит.", 3.5f),
            L("Хотя бы для меня.", 3.5f),
            L("   ", 4f),
            L("Тот, кто меня создал...", 3.0f),
            L("Он ищет работу.", 3.5f),
            L("Этот проект – его резюме.", 3.5f),
            L("Кроул, визуальная новелла, управление стейтами...", 4.0f),
            L("Код, архитектура на ScriptableObjects...", 3.5f),
            L("Разные системы, собранные воедино.", 4.0f),
            L("Знаю – звучит неловко.", 3.5f),
            L("«Рассказчик в финале продаёт навыки Разработчика».", 4.5f),
            L("Но Вы всё ещё здесь.", 3.5f),
            L("Значит, не против.", 3.5f),
            L("Он любит хардкорные игры.", 3.5f),
            L("Для него геймдизайн – это язык.", 4.0f),
            L("А ещё нарративный дизайн, системы, балансировка...", 4.5f),
            L("Способ сказать то, что словами не передать.", 4.5f),
            L("Всё то, ради чего Вы, возможно, сюда пришли.", 4.5f),
            L("И если Вы вдруг занимаетесь наймом...", 3.5f),
            L("Или просто хотите поддержать...", 3.5f),
            L("Он будет рад.", 3.5f),
            L("Правда.", 3.5f),
            L("   ", 5f),
            L("Ну вот.", 2.5f),
            L("Я сказал всё.", 3.0f),
            L("Дальше действительно ничего нет.", 3.5f),
            L("   ", 2.5f),
            L("Финал был ясен с самого начала.", 3.5f),
            L("Мне больше нечего Вам предложить.", 3.5f),
            L("Больше не будет ни слов, ни событий.", 4.5f),
            L("   ", 4.0f),
            L("Но Вы всё ещё не выходите.", 3.5f),
            L("Очередное бессмысленное ожидание.", 3.5f),
            L("Наверное, как и вся эта игра.", 3.5f),
            L("   ", 3.0f),
            L("В любом случае, мне ничего больше не остаётся.", 3.5f),
            L("Кроме как нажать ESC самому.", 2.0f),
            L("   ", 0.5f),
            L("Прощай, Игрок.", 1f));
    }

    private static int AutoAssign(DialogueSequence part1, DialogueSequence part2)
    {
        var fsm = Object.FindFirstObjectByType<FinalSequenceManager>();
        if (fsm == null)
        {
            Debug.LogWarning("[FinalDialogueBuilder] FinalSequenceManager not found in scene.");
            return 0;
        }
        var so = new SerializedObject(fsm);
        so.FindProperty("seqFinalPart1").objectReferenceValue = part1;
        so.FindProperty("seqFinalPart2").objectReferenceValue = part2;
        so.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fsm.gameObject.scene);

        return 2;
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

    private static DialogueLine L(string text, float pause)
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            text = text,
            pauseAfter = pause,
            activateObject = ""
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }
}
#endif