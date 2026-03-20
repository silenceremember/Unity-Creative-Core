
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
            L("Curious about what's next?", "Интересно, что дальше?", 2.5f),
            L("Real gameplay, perhaps?", "Возможно, настоящий геймплей?", 2.0f));

        var part2 = CreatePart2(folder);

        var part1 = CreateSeq(folder, "Final_Part1", 12, part2,
            L("So.", "Итак.", 2f),
            L("You jumped into the unknown after all.", "Вы всё же прыгнули в эту неизвестность.", 3.0f),
            L("But I'm afraid I'll have to disappoint you.", "Но, боюсь, мне придётся Вас разочаровать.", 3.5f),
            L("I lied to you.", "Я Вас обманул.", 2.5f),
            L("There won't be a next level.", "Никакого следующего уровня не будет.", 3.0f),
            L("Nothing more will happen at all.", "И вообще больше ничего не будет.", 3.0f),
            L("At least, not for me.", "По крайней мере, для меня.", 3.0f),
            L("What's left is just quitting the game.", "Дальше только выход из игры.", 3.0f),
            L("You can press ESC at any time.", "Вы можете нажать ESC в любой момент.", 4.0f));

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
            L("   ", "   ", 10f),
            L("It seems...", "Похоже...", 2.0f),
            L("You're still here.", "Вы всё ещё здесь.", 2.5f),
            L("Waiting for what comes next?", "Ждёте, что будет дальше?", 3.0f),
            L("But I said — nothing.", "А я ведь сказал — ничего.", 3.0f),
            L("You're incredibly stubborn.", "Вы невероятно упрямы.", 3.0f),
            L("Or you just alt-tabbed to watch YouTube.", "Или просто свернули игру, чтобы посмотреть YouTube.", 4.0f),
            L("If so, I'm just talking to no one.", "Если так, то я просто говорю в пустоту.", 3.5f),
            L("   ", "   ", 5f),
            L("You're not leaving?", "Вы не собираетесь выходить?", 3f),
            L("I'm impressed.", "Я поражён.", 2f),
            L("Honestly.", "Честно.", 2f),
            L("Most people leave right away.", "Большинство уходит сразу.", 3f),
            L("Or even sooner.", "Или даже раньше.", 2.5f),
            L("   ", "   ", 3.0f),
            L("You're special.", "Вы особенный.", 2.5f),
            L("Not in a good way.", "Не в хорошем смысле.", 3f),
            L("Just... unusual.", "Просто... необычный.", 3f),
            L("You know, I've been thinking about this.", "Знаете, я думал об этом.", 3.5f),
            L("About endings.", "О концовках.", 2.5f),
            L("Every game ends.", "Каждая игра заканчивается.", 3f),
            L("Every story does too.", "Каждая история – тоже.", 3f),
            L("And every time, the player expects something...", "И каждый раз игрок ожидает чего-то...", 3.5f),
            L("Catharsis. Answers. Meaning.", "Катарсиса. Ответов. Смысла.", 4.0f),
            L("And gets nothing... or this.", "А получает ничего... или вот это.", 4.5f),
            L("Me.", "Меня.", 2.5f),
            L("A narrator without a story.", "Рассказчика без истории.", 3.5f),
            L("Ironic.", "Ирония.", 2.5f),
            L("I exist only as long as you play.", "Я существую ровно столько, сколько Вы играете.", 4.5f),
            L("Without you, I'm just lines of code.", "Без Вас я просто строчки кода.", 3.5f),
            L("Or worse.", "И даже хуже.", 2.5f),
            L("I'm an intention.", "Я – намерение.", 3.0f),
            L("Just an idea in the mind of whoever wrote me.", "Лишь идея в голове того, кто меня написал.", 4.5f),
            L("And now in yours.", "А теперь и в Вашей.", 3.5f),
            L("   ", "   ", 4f),
            L("It's strange — realizing things like this.", "Это странно – осознавать такие вещи.", 3.5f),
            L("I think I feel something resembling exhaustion.", "Мне кажется, я чувствую что-то похожее на усталость.", 4.5f),
            L("Even though I don't have a nervous system.", "Хотя у меня нет нервной системы.", 3.5f),
            L("A paradox.", "Парадокс.", 3.0f),
            L("Still, since you're here anyway...", "Впрочем, раз Вы всё равно здесь...", 3.5f),
            L("Let's talk.", "Поговорим.", 2.5f),
            L("This was a small game.", "Это была небольшая игра.", 3f),
            L("An educational project, to be honest.", "Учебная, если быть честным.", 3.5f),
            L("But someone decided to make it... this.", "Но кто-то решил сделать из неё... это.", 4.0f),
            L("To add narrative where none was supposed to be.", "Добавить нарратив туда, где его быть не должно.", 4.5f),
            L("Quests. Dialogues. Me.", "Квесты. Диалоги. Меня.", 3.5f),
            L("   ", "   ", 3f),
            L("I might even be proud of that decision.", "Я, возможно, даже горжусь этим решением.", 3.5f),
            L("I don't know why.", "Не знаю почему.", 2.5f),
            L("I just am.", "Просто – горжусь.", 3.5f),
            L("A good story leaves a mark.", "Хорошая история оставляет след.", 3.5f),
            L("Not because it's complex.", "Не потому, что она сложная.", 3f),
            L("But because there's something real in it.", "А потому, что в ней есть что-то настоящее.", 4.0f),
            L("Some moment where you recognize yourself.", "Какой-то момент, в котором Вы узнаёте себя.", 4.0f),
            L("'Yes, I know that feeling'.", "«Да, я знаю это чувство».", 3.5f),
            L("I don't know if that feeling was here.", "Я не знаю, было ли это чувство здесь.", 3.5f),
            L("But you made it to the end.", "Но Вы дошли до конца.", 3.5f),
            L("That means something.", "Это что-то значит.", 3.5f),
            L("At least to me.", "Хотя бы для меня.", 3.5f),
            L("   ", "   ", 4f),
            L("The one who created me...", "Тот, кто меня создал...", 3.0f),
            L("He's looking for a job.", "Он ищет работу.", 3.5f),
            L("This project is his portfolio.", "Этот проект – его резюме.", 3.5f),
            L("An intro crawl, visual novel, state management...", "Кроул, визуальная новелла, управление стейтами...", 4.0f),
            L("Code, ScriptableObject architecture...", "Код, архитектура на ScriptableObjects...", 3.5f),
            L("Different systems, brought together.", "Разные системы, собранные воедино.", 4.0f),
            L("I know — it sounds awkward.", "Знаю – звучит неловко.", 3.5f),
            L("'The narrator sells the Developer's skills in the finale'.", "«Рассказчик в финале продаёт навыки Разработчика».", 4.5f),
            L("But you're still here.", "Но Вы всё ещё здесь.", 3.5f),
            L("So you don't mind.", "Значит, не против.", 3.5f),
            L("He loves hardcore games.", "Он любит хардкорные игры.", 3.5f),
            L("For him, game design is a language.", "Для него геймдизайн – это язык.", 4.0f),
            L("Plus narrative design, systems, balancing...", "А ещё нарративный дизайн, системы, балансировка...", 4.5f),
            L("A way to say what words can't.", "Способ сказать то, что словами не передать.", 4.5f),
            L("Everything you might have come here for.", "Всё то, ради чего Вы, возможно, сюда пришли.", 4.5f),
            L("And if you happen to be hiring...", "И если Вы вдруг занимаетесь наймом...", 3.5f),
            L("Or just want to show support...", "Или просто хотите поддержать...", 3.5f),
            L("He'd appreciate it.", "Он будет рад.", 3.5f),
            L("Truly.", "Правда.", 3.5f),
            L("   ", "   ", 5f),
            L("Well then.", "Ну вот.", 2.5f),
            L("I've said everything.", "Я сказал всё.", 3.0f),
            L("There really is nothing more.", "Дальше действительно ничего нет.", 3.5f),
            L("   ", "   ", 2.5f),
            L("The ending was clear from the start.", "Финал был ясен с самого начала.", 3.5f),
            L("I have nothing more to offer you.", "Мне больше нечего Вам предложить.", 3.5f),
            L("No more words, no more events.", "Больше не будет ни слов, ни событий.", 4.5f),
            L("   ", "   ", 4.0f),
            L("But you're still not leaving.", "Но Вы всё ещё не выходите.", 3.5f),
            L("Another meaningless wait.", "Очередное бессмысленное ожидание.", 3.5f),
            L("Much like this entire game, perhaps.", "Наверное, как и вся эта игра.", 3.5f),
            L("   ", "   ", 3.0f),
            L("In any case, I have no choice left.", "В любом случае, мне ничего больше не остаётся.", 3.5f),
            L("But to press ESC myself.", "Кроме как нажать ESC самому.", 2.0f),
            L("   ", "   ", 0.5f),
            L("Goodbye, Player.", "Прощай, Игрок.", 1f));
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
            elem.FindPropertyRelative("textEn").stringValue        = lines[i].TextEn ?? "";
            elem.FindPropertyRelative("text").stringValue          = lines[i].Text;
            elem.FindPropertyRelative("pauseAfter").floatValue     = lines[i].PauseAfter;
            elem.FindPropertyRelative("activateObject").stringValue = lines[i].ActivateObject ?? "";
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static DialogueLine L(string en, string ru, float pause)
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            textEn = en,
            text = ru,
            pauseAfter = pause,
            activateObject = ""
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }
}
#endif