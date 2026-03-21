
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates all DialogueSequence assets for the Exploration phase (60 sec).
/// Menu: Game → Dialogue → Build Exploration
/// </summary>
public static class ExplorationDialogueBuilder
{
    [MenuItem("Game/Dialogue/Build Exploration")]
    public static void Build()
    {
        const string folder = "Assets/_Project/SO/Dialogue/Exploration";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/SO/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project/SO", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/SO/Dialogue", "Exploration");

        var segmentLines = new DialogueLine[43][];

        segmentLines[0] = new[] {
            L("So.", "Итак.", 1.5f),
            L("You're in someone else's home.", "Вы находитесь в чужом доме.", 1f),
            L("Nobody invited you.", "Никто Вас не звал.", 1f) };
        segmentLines[1] = new[] {
            L("Then again, it's always like this in these games.", "Впрочем, в таких играх всегда так.", 2f),
            L("You just appear and look around.", "Просто оказываетесь и смотрите.", 1.5f),
            L("You don't touch anything.", "Ничего не трогаете.", 1.5f) };
        segmentLines[2] = new[] {
            L("Although there's nothing to touch.", "Хотя трогать и нечего.", 1.5f),
            L("Everything is locked.", "Всё заблокировано.", 1.5f),
            L("The Developer couldn't figure out what to do with it.", "Разработчик не придумал, что с этим делать.", 2f) };
        segmentLines[3] = new[] {
            L("He thought the atmosphere would be enough.", "Он думал, атмосферы будет достаточно.", 2f),
            L("Maybe it will be.", "Может, и будет.", 1.5f),
            L("I'm not one to judge.", "Я не судья.", 1.5f) };
        segmentLines[4] = new[] {
            L("Mary is on the balcony.", "На балконе Мэри.", 1.5f),
            L("Meant to be strong, with opinions of her own.", "По задумке сильная, со своим мнением.", 2f) };
        segmentLines[5] = new[] {
            L("For now, she just stands there.", "Пока что она просто стоит.", 2f),
            L("Her opinions are apparently still in development.", "Мнение, видимо, ещё в разработке.", 2f) };
        segmentLines[6] = new[] {
            L("John is in the doorway. As always.", "Джон в дверном проёме. Как всегда.", 2f),
            L("He'd love to leave, but there's no script attached to him.", "Он бы и рад уйти, но к нему не привязан скрипт.", 2f) };
        segmentLines[7] = new[] {
            L("They're a couple with a story.", "Они пара с историей.", 2f),
            L("What story? You'll find out later.", "Что за история, узнаете позже.", 2f),
            L("The Developer promises.", "Обещает Разработчик.", 1.5f) };
        segmentLines[8] = new[] {
            L("'Later' is the standard answer.", "«Позже» — это стандартный ответ.", 2f),
            L("Like 'it's on the roadmap' and 'we'll fix it soon'.", "Как «в планах» и «мы скоро пофиксим это».", 2f) };
        segmentLines[9] = new[] {
            L("Still, there are things to look at.", "Впрочем, тут есть что смотреть.", 1.5f),
            L("Paintings, for instance.", "Картины, например.", 1.5f) };
        segmentLines[10] = new[] {
            L("My role, really...", "Вообще, моя роль...", 2f),
            L("Is just to talk while you walk.", "Просто говорить, пока Вы ходите.", 1.5f),
            L("To explain the obvious.", "Объяснять очевидное.", 1.5f) };
        segmentLines[11] = new[] {
            L("This genre is called a Walking Simulator.", "Жанр таких игр называется Walking Simulator.", 2f),
            L("You walk. I talk.", "Вы ходите. Я говорю.", 1.5f),
            L("It turns out atmospheric.", "Получается атмосферно.", 1f) };
        segmentLines[12] = new[] {
            L("Silence, by the way, is also narrative.", "Тишина, кстати, тоже нарратив.", 1.5f),
            L("But if I go quiet...", "Но если я замолчу...", 1.5f),
            L("You'll get too bored.", "Вам станет слишком скучно.", 2f) };
        segmentLines[13] = new[] {
            L("And it seems you're already getting a bit bored.", "И похоже, вам уже становится скучновато.", 2f),
            L("I'm not sure I can entertain you much longer.", "Не знаю, смогу ли я Вас ещё долго развлекать.", 2f),
            L("But the Developer gave me a few tools.", "Но Разработчик выделил мне несколько инструментов.", 2f) };
        segmentLines[14] = new[] {
            L("Look...", "Смотрите...", 1.5f),
            L("A timer!", "Таймер!", 2f, "Timer") };
        segmentLines[15] = new[] {
            L("This is the average playtime for the game.", "Это среднее время прохождения игры.", 2f),
            L("Well, for what's left of it.", "Точнее, её оставшейся части.", 1.5f),
            L("Not yours.", "Не Ваше.", 1.5f) };
        segmentLines[16] = new[] {
            L("Fine. The timer didn't impress you, it seems.", "Ладно. Похоже, таймер Вас не впечатлил.", 2f),
            L("How about a clicker?", "А что насчёт кликера?", 2f, "Clicker") };
        segmentLines[17] = new[] {
            L("It's a simple counter.", "Это обычный счётчик.", 1.5f),
            L("Left-click makes a pleasant sound.", "При нажатии ЛКМ издаёт приятный звук.", 2f),
            L("Now this is genuinely interesting.", "Это уже действительно интересно.", 2f) };
        segmentLines[18] = new[] {
            L("Want to know why you're here?", "Хотите знать, зачем Вы здесь?", 2f),
            L("Fair question.", "Нормальный вопрос.", 1.5f),
            L("I don't really know myself.", "Я сам не очень понимаю.", 2f) };
        segmentLines[19] = new[] {
            L("The house, overall, is nice.", "Дом, в общем, хороший.", 2f),
            L("You can tell the Developer tried.", "Видно, что Разработчик старался.", 1.5f),
            L("There are details.", "Есть детали.", 1.5f) };
        segmentLines[20] = new[] {
            L("Look at the walls.", "Посмотрите на стены.", 1.5f),
            L("The Developer tried to figure out shaders.", "Разработчик попытался разобраться в шейдерах.", 2f),
            L("Doesn't look perfect. But at least it's not the pink material.", "Выглядит не идеально. Но хотя бы не розовый материал.", 2f) };
        segmentLines[21] = new[] {
            L("The lighting, by the way, is baked.", "Свет, кстати, запечённый.", 1.5f),
            L("Lightmap. Global illumination.", "Lightmap. Глобальное освещение.", 2f),
            L("Sounds simpler than it looks in the settings.", "Звучит проще, чем выглядит в настройках.", 2f) };
        segmentLines[22] = new[] {
            L("And the sounds?", "А звуки?", 1.5f),
            L("The Developer spent more time on them...", "Разработчик потратил на них больше времени...", 2f),
            L("Than on gameplay, to be honest.", "Чем на геймплей, по правде говоря.", 2f) };
        segmentLines[23] = new[] {
            L("Maybe mechanics aren't necessary.", "Может, и не нужна механика.", 1.5f),
            L("Sometimes it's enough to just be somewhere.", "Иногда достаточно просто побыть где-нибудь.", 2f),
            L("Not in the reality you're actually in.", "Не в той реальности, в которой Вы находитесь.", 2f),
            L("It's called escapism.", "Это называется эскапизм.", 1.5f) };
        segmentLines[24] = new[] {
            L("Mary and John aren't going anywhere.", "Мэри и Джон никуда не уйдут.", 1.5f),
            L("They'll be here forever.", "Они будут здесь всегда.", 1.5f),
            L("They don't have the option of escapism.", "У них нет возможностей для эскапизма.", 2f),
            L("But there's something to that.", "Но есть что-то в этом.", 2f) };
        segmentLines[25] = new[] {
            L("Interesting.", "Забавно.", 1.5f),
            L("Has anyone ever combined a rock simulator with a clicker?", "Кто-нибудь объединял симулятор камня с кликером?", 2f),
            L("Or a walking simulator with a clicker?", "Или симулятор ходьбы с кликером?", 2f),
            L("I don't know.", "Не знаю.", 1.5f),
            L("It would be something special.", "Получилось бы что-то особенное.", 2f) };
        segmentLines[26] = new[] {
            L("Cookie Clicker broke people back in the day.", "Cookie Clicker в своё время сломал людей.", 2f),
            L("Just a cookie. Just clicks.", "Просто печенье. Просто клики.", 2f),
            L("Millions of hours.", "Миллионы часов.", 1.5f) };
        segmentLines[27] = new[] {
            L("There's something to it.", "Что-то в этом есть.", 1.5f),
            L("A primitive rhythm.", "Примитивный ритм.", 1.5f),
            L("Click. Sound. Number goes up.", "Клик. Звук. Число растёт.", 2f) };
        segmentLines[28] = new[] {
            L("A bit like walking around a room.", "Немного похоже на ходьбу по комнате.", 2f),
            L("Step. Step. Step.", "Шаг. Шаг. Шаг.", 1f),
            L("That's essentially what you're doing.", "Вы, собственно, этим и занимаетесь.", 2f) };
        segmentLines[29] = new[] {
            L("The Developer could've made a game about clicking.", "Разработчик мог бы сделать игру про клики.", 2f),
            L("But chose to make this instead.", "Но выбрал сделать это.", 1.5f),
            L("Because he believes that...", "Поскольку он считает, что...", 1.5f),
            L("This would demonstrate his skills.", "Это продемонстрирует его навыки.", 2f) };
        segmentLines[30] = new[] {
            L("Though the difference is small.", "Хотя разница невелика.", 1.5f),
            L("Either way, it's aimless activity.", "И там, и тут бесцельное действие.", 2f) };
        segmentLines[31] = new[] {
            L("By the way, how's the clicker?", "Кстати, как Вам кликер?", 2f) };
        segmentLines[32] = new[] {
            L("I'd try it too.", "Я бы тоже попробовал.", 1.5f),
            L("But I don't have hands.", "Но у меня нет рук.", 2f),
            L("Just a Text Mesh Pro text field.", "Только поле текста Text Mesh Pro.", 2f) };
        segmentLines[33] = new[] {
            L("You know what's great about simple clickers?", "Знаете, в чём прелесть простых кликеров?", 2f),
            L("They're crystal clear honest.", "Они кристально честны.", 1.5f),
            L("You click, and you get a result. Instantly.", "Вы кликаете, и получаете результат. Сразу.", 2f) };
        segmentLines[34] = new[] {
            L("No hidden purpose.", "Нет скрытой цели.", 1.5f),
            L("No achievement rewards.", "Нет награды за достижения.", 2f),
            L("Just sound and a number.", "Только звук и число.", 1.5f),
            L("Although nowadays everything gets stuffed with...", "Хотя сейчас всё принято пичкать...", 2f),
            L("Meta-progression.", "Метапрогрессией.", 1.5f) };
        segmentLines[35] = new[] {
            L("I think humanity seeks comfort...", "Я думаю, человечество ищет успокоение...", 2f),
            L("In unexpected places.", "В неожиданных местах.", 1.5f),
            L("And sometimes actually finds it.", "И действительно иногда находит.", 2f) };
        segmentLines[36] = new[] {
            L("I've read that idle games reduce anxiety.", "Я читал, что idle-игры редуцируют тревогу.", 2f),
            L("The process is soothing.", "Процесс успокаивает.", 1.5f),
            L("Maybe there's something to that here too.", "Может, и здесь что-то в этом есть.", 2f) };
        segmentLines[37] = new[] {
            L("Then again...", "Хотя, с другой стороны...", 1.5f),
            L("What does that say about us as a species?", "Что это говорит о нас как о виде?", 2f),
            L("I don't know.", "Не знаю.", 1.5f),
            L("Since I'm not real, only you can truly understand.", "Ведь я не настоящий, только Вам дано понять.", 2f),
            L("Maybe that's why you're here.", "Возможно, поэтому Вы здесь.", 1.5f),
            L("Only you can appreciate...", "Только Вам дано оценить...", 1.5f),
            L("The full depth of the Developer's vision.", "Всю глубину замысла Разработчика.", 2f) };
        segmentLines[38] = new[] {
            L("Alright.", "Ладно.", 1f),
            L("Looks like the clicker is getting dull.", "Похоже, кликер становится скучноват.", 2f),
            L("Just like the timer.", "Как и таймер.", 1.5f) };
        segmentLines[39] = new[] {
            L("How about a little quest?", "Что насчёт небольшого квеста?", 2f),
            L("...", "...", 1f) };
        segmentLines[40] = new[] {
            L("...How awful.", "...Какой кошмар.", 2f, "PaintingShift"),
            L("The paintings have magically tilted.", "Картины магическим образом наклонились.", 2f),
            L("An extremely dramatic plot twist.", "Крайне драматичный поворот сюжета.", 2f),
            L("They urgently need to be put back.", "Их надо срочно вернуть на место.", 2f) };
        segmentLines[41] = new[] {
            L("When it runs out, nothing will happen to you.", "По его истечении с Вами ничего не случится.", 2f),
            L("Just a fun little thing.", "Просто забавная вещь.", 1.5f),
            L("By the way, there's a whole culture...", "Кстати, есть целая культура...", 1.5f),
            L("Of completing games as fast as possible.", "Прохождения игр на скорость.", 1.5f),
            L("It's called Speedrunning.", "Называется Speedrun.", 1.5f),
            L("But everything here is scripted.", "Но тут всё заскриптовано.", 1.5f),
            L("So don't bother trying, there's no point.", "Так что не пытайтесь, смысла нет.", 2f) };
        segmentLines[42] = new[] {
            L("Help the tenants with this impossible task.", "Помогите жильцам с этой невыполнимой задачей.", 2f, "QuestCanvas") };

        // Pass 1: create/overwrite assets to disk, write lines via SerializedObject
        var disk = new DialogueSequence[segmentLines.Length];
        for (int i = 0; i < segmentLines.Length; i++)
            disk[i] = CreateSeq(folder, $"Seq_Ambient_{i:D2}", 0, null, segmentLines[i]);

        // Pass 2: wire nextSequence between disk objects via SerializedObject
        for (int i = 0; i < disk.Length - 1; i++)
            SetNextSequence(disk[i], disk[i + 1]);

        var diskStart = disk[0];

        //  TRIGGER B — one-shot (~67s), secret passage / Stanley Parable
        var trigB = CreateSeq(folder, "Seq_Trigger_B", 10, null,
            L("Congratulations!", "Поздравляю!", 1.5f),
            L("You found the Developer's secret passage.", "Вы нашли секретный лаз Разработчика.", 3.0f),
            L("Just like one of the endings in The Stanley Parable.", "Прямо как одна из концовок в The Stanley Parable.", 3.5f),
            L("Except there's no branching narrative here.", "Только здесь нет разветвлённого нарратива.", 3.0f),
            L("And for your outstanding observation skills...", "И за Вашу выдающуюся наблюдательность...", 3.0f),
            L("The game ceremoniously rewards you with...", "Игра торжественно награждает Вас...", 3.0f),
            L("A concrete wall in the window.", "Бетонной стеной в окне.", 2.5f, "SecretBlock"),
            L("Feel free to add 'QA Tester' to your resume.", "Можете смело добавлять в резюме пункт «QA-тестировщик».", 4.0f),
            L("Mary is very upset now, though.", "Правда, Мэри теперь очень расстроена.", 3.0f),
            L("Her lovely skybox view has been taken away.", "У неё забрали красивый вид на скайбокс.", 3.0f),
            L("And that's entirely your fault.", "И это полностью Ваша вина.", 2.5f),
            L("You know, sometimes textures beyond the map borders...", "Знаете, иногда текстуры за границами карты...", 3.0f),
            L("Are better left untouched.", "Лучше просто не трогать.", 2.5f),
            L("Anyway. You interrupted my monologue with your antics.", "Ладно. Вы прервали мой монолог своими выходками.", 3.5f),
            L("Back to where we were.", "Возвращаемся.", 1.5f)
        );

        // Trigger B restores interrupted dialogue
        SetRestoreInterrupted(trigB, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assignedCount = AutoAssignInScene(diskStart);

        string autoMsg = assignedCount > 0
            ? "Scene references updated."
            : "⚠ Manager not found in scene — assign manually.";
        EditorUtility.DisplayDialog("Build Exploration",
            $"Assets updated in {folder}.\n{autoMsg}", "OK");
    }

    private static int AutoAssignInScene(DialogueSequence ambientStart)
    {
        var mgr = Object.FindFirstObjectByType<ExplorationManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[ExplorationDialogueBuilder] ExplorationManager not found in scene.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("seqAmbientStart").objectReferenceValue = ambientStart;
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            mgr.gameObject.scene);

        return 1;
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

    private static void SetNextSequence(DialogueSequence asset, DialogueSequence next)
    {
        var so = new SerializedObject(asset);
        so.FindProperty("nextSequence").objectReferenceValue = next;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
    }

    private static void SetRestoreInterrupted(DialogueSequence asset, bool value)
    {
        var so = new SerializedObject(asset);
        so.FindProperty("restoreInterrupted").boolValue = value;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(asset);
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
