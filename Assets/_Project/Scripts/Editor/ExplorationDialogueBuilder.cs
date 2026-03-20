
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

        //  AMBIENT — linear chain.
        //  Quest starts on last segment completion
        //  (nextSequence == null → ExplorationManager starts quest).
        //  Chain: A[0] → A[1] → … → A[44] → null
        var segmentLines = new DialogueLine[45][];

        segmentLines[0] = new[] {
            L("Итак.", 1.5f),
            L("Вы находитесь в чужом доме.", 2.5f),
            L("Никто Вас не звал.", 2f) };
        segmentLines[1] = new[] {
            L("Впрочем, в таких играх всегда так.", 3f),
            L("Просто оказываетесь и смотрите.", 2f),
            L("Ничего не трогаете.", 2f) };
        segmentLines[2] = new[] {
            L("Хотя трогать и нечего.", 2f),
            L("Всё заблокировано.", 1.5f),
            L("Разработчик не придумал, что с этим делать.", 3f) };
        segmentLines[3] = new[] {
            L("Он думал, атмосферы будет достаточно.", 3f),
            L("Может, и будет.", 1.5f),
            L("Я не судья.", 1.5f) };
        segmentLines[4] = new[] {
            L("На балконе Мэри.", 2f),
            L("По задумке сильная, со своим мнением.", 3f) };
        segmentLines[5] = new[] {
            L("Пока что она просто стоит.", 2.5f),
            L("Мнение, видимо, ещё в разработке.", 3f) };
        segmentLines[6] = new[] {
            L("Джон в дверном проёме. Как всегда.", 2.5f),
            L("Он бы и рад уйти, но нет анимации.", 3.5f) };
        segmentLines[7] = new[] {
            L("Они пара с историей.", 2.5f),
            L("Что за история, узнаете позже.", 2.5f),
            L("Обещает Разработчик.", 1.5f) };
        segmentLines[8] = new[] {
            L("«Позже» всегда стандартный ответ.", 2f),
            L("Как «в планах» и «баг скоро починят».", 2.5f) };
        segmentLines[9] = new[] {
            L("Впрочем, тут есть что смотреть.", 2f),
            L("Картины, например.", 1.5f) };
        segmentLines[10] = new[] {
            L("Вообще, моя роль...", 3f),
            L("Просто говорить, пока Вы ходите.", 2f),
            L("Объяснять очевидное.", 1.5f),
            L("Да, тяжёлая работа.", 3f) };
        segmentLines[11] = new[] {
            L("Жанр таких игр называется Walking Simulator.", 3f),
            L("Вы ходите. Я говорю.", 1.5f),
            L("Получается атмосферно.", 2.5f) };
        segmentLines[12] = new[] {
            L("Тишина, кстати, тоже нарратив.", 2f),
            L("Но если я замолчу...", 1.5f),
            L("Вам станет слишком скучно.", 5f) };
        segmentLines[13] = new[] {
            L("Также...", 1f),
            L("Если Вы поиграете во второй раз...", 1.5f),
            L("Я скажу всё то же самое.", 2.5f),
            L("Слово в слово.", 1.5f),
            L("Такова природа Рассказчика.", 2.5f) };
        segmentLines[14] = new[] {
            L("Хотите знать, зачем Вы здесь?", 2f),
            L("Нормальный вопрос.", 1.5f),
            L("Я сам не очень понимаю.", 2.5f) };
        segmentLines[15] = new[] {
            L("Дом, в общем, хороший.", 2.5f),
            L("Видно, что Разработчик старался.", 2f),
            L("Есть детали.", 1.5f) };
        segmentLines[16] = new[] {
            L("Посмотрите на стены.", 1.5f),
            L("Разработчик разобрался в шейдерах.", 2.0f),
            L("Не идеально. Но и не стоковый материал.", 2.5f) };
        segmentLines[17] = new[] {
            L("Свет, кстати, запечённый.", 1.5f),
            L("Lightmap. Глобальное освещение.", 2.0f),
            L("Звучит проще, чем выглядит в настройках.", 2.5f) };
        segmentLines[18] = new[] {
            L("А звуки?", 1.5f),
            L("Разработчик потратил на них больше времени...", 2.0f),
            L("Чем на геймплей, по правде говоря.", 2.5f) };
        segmentLines[19] = new[] {
            L("Может, и не нужна механика.", 1.5f),
            L("Иногда достаточно просто побыть где-нибудь.", 3.5f),
            L("Не в той реальности, в которой находитесь.", 3.5f),
            L("Это называется эскапизм.", 1.5f) };
        segmentLines[20] = new[] {
            L("Мэри и Джон никуда не уйдут.", 2f),
            L("Они будут здесь всегда.", 2f),
            L("У них нет возможностей для эскапизма.", 2f),
            L("Но есть что-то в этом.", 2f) };
        segmentLines[21] = new[] {
            L("Похоже, становится скучновато.", 2f),
            L("Не знаю, смогу ли я Вас ещё долго развлекать.", 3f),
            L("Но Разработчик выделил мне несколько инструментов.", 2.5f) };
        segmentLines[22] = new[] {
            L("Смотрите...", 1.5f),
            L("Таймер!", 2.5f, "Timer") };
        segmentLines[23] = new[] {
            L("Это среднее время прохождения игры.", 2f),
            L("Точнее, её оставшейся части.", 2f),
            L("Это время среднего игрока.", 2f),
            L("Не Ваше.", 2f) };
        segmentLines[24] = new[] {
            L("По его истечению с Вами ничего не случится.", 3.5f),
            L("Просто забавная вещь.", 2f),
            L("Кстати, есть целая культура.", 2f),
            L("Прохождения игр на скорость.", 1.5f),
            L("Называется Speedrun.", 2f),
            L("Но тут всё заскриптовано.", 1.5f),
            L("Так что не пытайтесь, смысла нет.", 2f) };
        segmentLines[25] = new[] {
            L("Ладно. Похоже, таймер Вас не впечатлил.", 3f),
            L("А что насчёт кликера?", 2f, "Clicker") };
        segmentLines[26] = new[] {
            L("Это обычный счётчик.", 2f),
            L("При нажатии ЛКМ издаёт приятный звук.", 2f),
            L("Это уже действительно интересно.", 2f) };
        segmentLines[27] = new[] {
            L("Забавно.", 1.5f),
            L("Кто-нибудь объединял симулятор камня с кликером?", 3.5f),
            L("Или симулятор ходьбы с кликером?", 2f),
            L("Не знаю.", 1.5f),
            L("Получилось бы что-то особенное.", 2f) };
        segmentLines[28] = new[] {
            L("Cookie Clicker в своё время сломал людей.", 3f),
            L("Просто печенье. Просто клики.", 2.5f),
            L("Миллионы часов.", 2f) };
        segmentLines[29] = new[] {
            L("Что-то в этом есть.", 2f),
            L("Примитивный ритм.", 2f),
            L("Клик. Звук. Число растёт.", 2.5f) };
        segmentLines[30] = new[] {
            L("Немного похоже на ходьбу по комнате.", 2f),
            L("Шаг. Шаг. Шаг.", 1f),
            L("Вы, собственно, этим и занимаетесь.", 2f) };
        segmentLines[31] = new[] {
            L("Разработчик мог бы сделать игру про клики.", 3f),
            L("Но выбрал сделать это.", 2f),
            L("Поскольку, считает он...", 1.5f),
            L("Это продемонстрирует его навыки.", 2f) };
        segmentLines[32] = new[] {
            L("Хотя разница невелика.", 1.5f),
            L("И там, и тут бесцельное действие.", 2f) };
        segmentLines[33] = new[] {
            L("Кстати, как Вам кликер?", 2.5f) };
        segmentLines[34] = new[] {
            L("Я бы тоже попробовал.", 1.5f),
            L("Но у меня нет рук.", 2.5f),
            L("Только поле текста Text Mesh Pro.", 3f) };
        segmentLines[35] = new[] {
            L("Знаете, в чём прелесть простых кликеров?", 3f),
            L("Они честны.", 1.5f),
            L("Вот Вы кликаете, и видите, что происходит.", 3f) };
        segmentLines[36] = new[] {
            L("Нет скрытой цели.", 2f),
            L("Нет награды за достижения.", 2.5f),
            L("Только звук и число.", 2f),
            L("Хотя сейчас всё принято пичкать...", 2f),
            L("Метапрогрессией.", 2f) };
        segmentLines[37] = new[] {
            L("Но раньше...", 2f),
            L("Всё равно кто-то доходил до миллиарда.", 2f),
            L("И что?", 1.5f),
            L("И продолжал дальше.", 2f) };
        segmentLines[38] = new[] {
            L("Я думаю, человечество ищет успокоение...", 3f),
            L("В неожиданных местах.", 2f),
            L("И действительно иногда находит.", 2f) };
        segmentLines[39] = new[] {
            L("Я читал, что idle-игры редуцируют тревогу.", 3.5f),
            L("Процесс успокаивает.", 2f),
            L("Может, и здесь что-то в этом есть.", 3f) };
        segmentLines[40] = new[] {
            L("Хотя с другой стороны...", 2f),
            L("Что это говорит о нас как виде?", 3.5f),
            L("Не знаю.", 2f),
            L("Ведь я не настоящий, только Вам дано понять.", 3f),
            L("Возможно, поэтому Вы здесь.", 2f),
            L("Только Вам дано оценить...", 2f),
            L("Всю глубину замысла Разработчика.", 2f) };
        segmentLines[41] = new[] {
            L("Ладно.", 1.5f),
            L("Похоже, кликер становится скучноват.", 3f),
            L("Как и таймер.", 2f) };
        segmentLines[42] = new[] {
            L("Что насчёт небольшого квеста?", 3f),
            L("...", 1f) };
        segmentLines[43] = new[] {
            L("...Какой ужас.", 2f, "PaintingShift"),
            L("Картины магическим образом наклонились.", 3f),
            L("Их надо поместить на прежнее место.", 3f) };
        segmentLines[44] = new[] {
            L("Помогите жильцам разобраться с этим.", 4f, "QuestCanvas") };

        // Pass 1: create/overwrite assets to disk, write lines via SerializedObject
        var disk = new DialogueSequence[segmentLines.Length];
        for (int i = 0; i < segmentLines.Length; i++)
            disk[i] = CreateSeq(folder, $"Seq_Ambient_{i:D2}", 0, null, segmentLines[i]);

        // Pass 2: wire nextSequence between disk objects via SerializedObject
        for (int i = 0; i < disk.Length - 1; i++)
            SetNextSequence(disk[i], disk[i + 1]);

        var diskStart = disk[0];

        //  TRIGGER A — one-shot (~48s)
        var trigA = CreateSeq(folder, "Seq_Trigger_A", 10, null,
            L("Стой!", 1.5f),
            L("Присмотритесь...", 2f),
            L("На лестничной клетке посмотрите вниз.", 2f),
            L("О. Это текстурный шов.", 2f),
            L("Очередное доказательство неумелости Разработчика.", 2f),
            L("Чего стоит сшить два меша в один...", 2f),
            L("Чтобы нормально запечь свет?", 2f),
            L("Пусть и многого, но всё же...", 2f),
            L("Разработчик явно не в теме.", 2f),
            L("Хотя, гоняясь за идеальностью...", 2f),
            L("Другие делают проекты годами.", 2f),
            L("А потом выгорают и бросают.", 2f),
            L("Этот Разработчик же проект не бросил.", 2f),
            L("Просто выложил как есть.", 2f),
            L("Это либо безрассудно...", 2f),
            L("Либо довольно смело.", 2f),
            L("Но Разработчик написал эту реплику.", 2f),
            L("Значит, явно перфекционист.", 2f),
            L("В любом случае это не так важно.", 2f),
            L("Вернёмся, пожалуй, к предыдущей реплике...", 2f)
        );

        //  TRIGGER B — one-shot (~67s), secret passage / Stanley Parable
        var trigB = CreateSeq(folder, "Seq_Trigger_B", 10, null,
            L("Поздравляю!", 1.5f),
            L("Вы нашли секретный лаз Разработчика.", 2f),
            L("Прямо как одна из концовок в The Stanley Parable.", 2f),
            L("Правда, это не The Stanley Parable.", 2f),
            L("Разработчик просто вдохновлялся им.", 2f),
            L("Там был выбор. Здесь – нет.", 2f),
            L("Ну, почти нет...", 2f),
            L("Вы же решаете, куда идти.", 2f),
            L("Но серенад и концовок всё равно не будет.", 2f),
            L("Вместо этого, за эту находку...", 2f),
            L("Разработчик награждает Вас...", 3f),
            L("Скучным видом из окна.", 2f, "SecretBlock"),
            L("А Вы можете называть себя тестировщиком игры.", 2f),
            L("Но, похоже, Мэри расстроена.", 2f),
            L("У неё больше нет красивого вида...", 2f),
            L("На HDRI-карту.", 2f),
            L("И это дело Ваших рук.", 2f),
            L("Знаете, иногда есть вещи, которые...", 2f),
            L("Лучше не трогать.", 2f),
            L("В любом случае, Ваша наблюдательность", 2f),
            L("Разработчика явно радует.", 2f),
            L("Ладно. Вы меня прервали своими выходками.", 2f),
            L("Возвращаемся.", 1f)
        );

        // Triggers A and B restore interrupted dialogue
        SetRestoreInterrupted(trigA, true);
        SetRestoreInterrupted(trigB, true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Auto-assign in ExplorationManager in scene
        int assignedCount = AutoAssignInScene(diskStart);


        string autoMsg = assignedCount > 0
            ? $"ExplorationManager in scene updated automatically ({assignedCount} fields)."
            : "⚠ ExplorationManager not found in open scene — assign manually.";
        EditorUtility.DisplayDialog("Done!",
            $"Updated {segmentLines.Length} ambient segments + 2 triggers.\n\n{autoMsg}",
            "OK");
    }

    /// <summary>
    /// Finds ExplorationManager in the open scene and wires ambient start ref.
    /// </summary>
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
            elem.FindPropertyRelative("text").stringValue          = lines[i].Text;
            elem.FindPropertyRelative("pauseAfter").floatValue     = lines[i].PauseAfter;
            elem.FindPropertyRelative("duration").floatValue       = lines[i].Duration;
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

    private static DialogueLine L(string text, float pause, string activateObject = "")
    {
        string json = JsonUtility.ToJson(new DialogueLineData
        {
            text = text,
            pauseAfter = pause,
            duration = 0f,
            activateObject = activateObject
        });
        return JsonUtility.FromJson<DialogueLine>(json);
    }
}
#endif
