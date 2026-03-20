
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
        const string folder = "Assets/_Project/Dialogue/Exploration";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Dialogue", "Exploration");

        // ══════════════════════════════════════════════════════════════
        //  AMBIENT — linear chain.
        //  Quest starts on last segment completion
        //  (nextSequence == null → ExplorationManager starts quest).
        //  Chain: A[0] → A[1] → … → A[19] → null
        // ══════════════════════════════════════════════════════════════
        var a = new DialogueSequence[45];
        for (int i = 0; i < a.Length; i++)
            a[i] = ScriptableObject.CreateInstance<DialogueSequence>();

        // Segment 0
        a[0].lines = new[]
        {
            L("Итак.", 1.5f),
            L("Вы находитесь в чужом доме.", 2.5f),
            L("Никто Вас не звал.", 2f),
        };

        // Segment 1
        a[1].lines = new[]
        {
            L("Впрочем, в таких играх всегда так.", 3f),
            L("Просто оказываетесь и смотрите.", 2f),
            L("Ничего не трогаете.", 2f),
        };

        // Segment 2
        a[2].lines = new[]
        {
            L("Хотя трогать и нечего.", 2f),
            L("Всё заблокировано.", 1.5f),
            L("Разработчик не придумал, что с этим делать.", 3f),
        };

        // Segment 3
        a[3].lines = new[]
        {
            L("Он думал, атмосферы будет достаточно.", 3f),
            L("Может, и будет.", 1.5f),
            L("Я не судья.", 1.5f),
        };

        // Segment 4
        a[4].lines = new[]
        {
            L("На балконе Мэри.", 2f),
            L("По задумке сильная, со своим мнением.", 3f),
        };

        // Segment 5
        a[5].lines = new[]
        {
            L("Пока что она просто стоит.", 2.5f),
            L("Мнение, видимо, ещё в разработке.", 3f),
        };

        // Segment 6
        a[6].lines = new[]
        {
            L("Джон в дверном проёме. Как всегда.", 2.5f),
            L("Он бы и рад уйти, но нет анимации.", 3.5f),
        };

        // Segment 7
        a[7].lines = new[]
        {
            L("Они пара с историей.", 2.5f),
            L("Что за история, узнаете позже.", 2.5f),
            L("Обещает Разработчик.", 1.5f),
        };

        // Segment 8
        a[8].lines = new[]
        {
            L("«Позже» всегда стандартный ответ.", 2f),
            L("Как «в планах» и «баг скоро починят».", 2.5f),
        };

        // Segment 9
        a[9].lines = new[]
        {
            L("Впрочем, тут есть что смотреть.", 2f),
            L("Картины, например.", 1.5f),
        };

        // Segment 10
        a[10].lines = new[]
        {
            L("Вообще, моя роль...", 3f),
            L("Просто говорить, пока Вы ходите.", 2f),
            L("Объяснять очевидное.", 1.5f),
            L("Да, тяжёлая работа.", 3f),
        };

        // Segment 11
        a[11].lines = new[]
        {
            L("Жанр таких игр называется Walking Simulator.", 3f),
            L("Вы ходите. Я говорю.", 1.5f),
            L("Получается атмосферно.", 2.5f),
        };

        // Segment 12
        a[12].lines = new[]
        {
            L("Тишина, кстати, тоже нарратив.", 2f),
            L("Но если я замолчу...", 1.5f),
            L("Станет слишком скучно.", 5f),
        };

        // Segment 13
        a[13].lines = new[]
        {
            L("Также...", 1f),
            L("Если Вы поиграете во второй раз...", 1.5f),
            L("Я скажу всё то же самое.", 2.5f),
            L("Слово в слово.", 1.5f),
            L("Такова природа Рассказчика.", 2.5f),
        };

        // Segment 14
        a[14].lines = new[]
        {
            L("Хотите знать, зачем Вы здесь?", 2f),
            L("Нормальный вопрос.", 1.5f),
            L("Я сам не очень понимаю.", 2.5f),
        };

        // Segment 15
        a[15].lines = new[]
        {
            L("Дом, в общем, хороший.", 2.5f),
            L("Видно, что Разработчик старался.", 2f),
            L("Есть детали.", 1.5f),
        };

        // Segment 16
        a[16].lines = new[]
        {
            L("Посмотрите на стены.", 1.5f),
            L("Разработчик разобрался в шейдерах.", 2.0f),
            L("Не идеально. Но и не стоковый материал.", 2.5f),
        };

        // Segment 17
        a[17].lines = new[]
        {
            L("Свет, кстати, запечённый.", 1.5f),
            L("Lightmap. Глобальное освещение.", 2.0f),
            L("Звучит проще, чем выглядит в настройках.", 2.5f),
        };

        // Segment 18
        a[18].lines = new[]
        {
            L("А звуки?", 1.5f),
            L("Разработчик потратил на них больше времени...", 2.0f),
            L("Чем на геймплей, по правде говоря.", 2.5f),
        };

        // Segment 19
        a[19].lines = new[]
        {
            L("Может, и не нужна механика.", 1.5f),
            L("Иногда достаточно просто побыть где-нибудь.", 3.5f),
            L("Не в той реальности, в которой находитесь.", 3.5f),
            L("Это называется эскапизм.", 1.5f),
        };

        // Segment 20
        a[20].lines = new[]
        {
            L("Мэри и Джон никуда не уйдут.", 2f),
            L("Они будут здесь всегда.", 2f),
            L("У них нет возможностей для эскапизма.", 2f),
            L("Но есть что-то в этом.", 2f),
        };

        // Segment 21
        a[21].lines = new[]
        {
            L("Похоже, становится скучновато.", 2f),
            L("Не знаю, смогу ли я Вас ещё долго развлекать.", 3f),
            L("Но Разработчик выделил мне несколько инструментов.", 2.5f),
        };

        // Segment 22 — TIMER (assign to seqTimerTrigger in ExplorationManager)
        a[22].lines = new[]
        {
            L("Смотрите...", 1.5f),
            L("Таймер!", 2.5f),
        };

        // Segment 23
        a[23].lines = new[]
        {
            L("Это среднее время прохождения игры.", 2f),
            L("Точнее, её оставшейся части.", 2f),
            L("Это время среднего игрока.", 2f),
            L("Не Ваше.", 2f),
        };

        // Segment 24
        a[24].lines = new[]
        {
            L("По его истечению с Вами ничего не случится.", 3.5f),
            L("Просто забавная вещь.", 2f),
            L("Кстати, есть целая культура.", 2f),
            L("Прохождения игр на скорость.", 1.5f),
            L("Называется Speedrun.", 2f),
            L("Но тут всё заскриптовано.", 1.5f),
            L("Так что не пытайтесь, смысла нет.", 2f),
        };

        // Segment 25 — timer boring, clicker appears
        a[25].lines = new[]
        {
            L("Ладно. Похоже, таймер Вас не впечатлил.", 3f),
            L("А что насчёт кликера?", 2f),
        };

        // Segment 26
        a[26].lines = new[]
        {
            L("Это обычный счётчик.", 2f),
            L("При нажатии ЛКМ издаёт приятный звук.", 2f),
            L("Это уже действительно интересно.", 2f),
        };

        // Segment 27
        a[27].lines = new[]
        {
            L("Забавно.", 1.5f),
            L("Кто-нибудь объединял симулятор камня с кликером?", 3.5f),
            L("Или симулятор ходьбы с кликером?", 2f),
            L("Не знаю.", 1.5f),
            L("Получилось бы что-то особенное.", 2f),
        };

        // Segment 28
        a[28].lines = new[]
        {
            L("Cookie Clicker в своё время сломал людей.", 3f),
            L("Просто печенье. Просто клики.", 2.5f),
            L("Миллионы часов.", 2f),
        };

        // Segment 29
        a[29].lines = new[]
        {
            L("Что-то в этом есть.", 2f),
            L("Примитивный ритм.", 2f),
            L("Клик. Звук. Число растёт.", 2.5f),
        };

        // Segment 30
        a[30].lines = new[]
        {
            L("Немного похоже на ходьбу по комнате.", 2f),
            L("Шаг. Шаг. Шаг.", 1f),
            L("Вы, собственно, этим и занимаетесь.", 2f),
        };

        // Segment 31
        a[31].lines = new[]
        {
            L("Разработчик мог бы сделать игру про клики.", 3f),
            L("Но выбрал сделать это.", 2f),
            L("Поскольку, считает он...", 1.5f),
            L("Это продемонстрирует его навыки.", 2f),
            L("Уважаю.", 1.5f),
        };

        // Segment 32
        a[32].lines = new[]
        {
            L("Хотя разница невелика.", 1.5f),
            L("И там, и тут бесцельное действие.", 2f),
            L("Просто одно немного интереснее другого.", 2f),
        };

        // Segment 33
        a[33].lines = new[]
        {
            L("Кстати, как Вам кликер?", 2.5f),
        };

        // Segment 34
        a[34].lines = new[]
        {
            L("Я бы тоже попробовал.", 1.5f),
            L("Но у меня нет рук.", 2.5f),
            L("Только поле текста Text Mesh Pro.", 3f),
        };

        // Segment 35
        a[35].lines = new[]
        {
            L("Знаете, в чём прелесть простых кликеров?", 3f),
            L("Они честны.", 1.5f),
            L("Вот Вы кликаете, и видите, что происходит.", 3f),
        };

        // Segment 36
        a[36].lines = new[]
        {
            L("Нет скрытой цели.", 2f),
            L("Нет награды за достижения.", 2.5f),
            L("Только звук и число.", 2f),
            L("Хотя сейчас всё принято пичкать...", 2f),
            L("Метапрогрессией.", 2f),
        };

        // Segment 37
        a[37].lines = new[]
        {
            L("Но раньше...", 2f),
            L("Всё равно кто-то доходил до миллиарда.", 2f),
            L("И что?", 1.5f),
            L("И продолжал кликать.", 2f),
        };

        // Segment 38
        a[38].lines = new[]
        {
            L("Я думаю, человечество ищет успокоение...", 3f),
            L("В неожиданных местах.", 2f),
            L("И действительно иногда находит.", 2f),
        };

        // Segment 39
        a[39].lines = new[]
        {
            L("Я читал, что idle-игры редуцируют тревогу.", 3.5f),
            L("Процесс успокаивает.", 2f),
            L("Может, и здесь что-то в этом есть.", 3f),
        };

        // Segment 40
        a[40].lines = new[]
        {
            L("Хотя с другой стороны...", 2f),
            L("Что это говорит о нас как виде?", 3.5f),
            L("Не знаю.", 2f),
            L("Ведь я не настоящий, только Вам дано понять.", 3f),
            L("Возможно, поэтому Вы здесь.", 2f),
            L("Только Вам дано оценить...", 2.0f),
            L("Всю глубину замысла Разработчика.", 2f),
        };

        // Segment 41 — clicker gets boring
        a[41].lines = new[]
        {
            L("Ладно.", 1.5f),
            L("Похоже, кликер становится скучноват.", 3f),
            L("Как и таймер.", 2f),
        };

        // Segment 42 — transition to quest
        a[42].lines = new[]
        {
            L("Что насчёт небольшого квеста?", 3f),
            L("...", 1f),
        };

        // Segment 43 — paintings shift on first line
        a[43].lines = new[]
        {
            L("...Какой ужас.", 2f, "PaintingShift"),
            L("Картины магическим образом наклонились.", 3f),
            L("Их надо поместить на прежнее место.", 3f),
        };

        // Segment 44 — final chain segment, quest starts (nextSequence = null)
        a[44].lines = new[]
        {
            L("Помогите жильцам разобраться с этим.", 4f, "QuestCanvas"),
        };

        // Key indices for auto-assignment in ExplorationManager
        const int idxTimerTrigger   = 22;
        const int idxClickerTrigger = 25;

        foreach (var seg in a)
            seg.priority = 0;

        // Pass 1: save/overwrite assets to disk
        var disk = new DialogueSequence[a.Length];
        for (int i = 0; i < a.Length; i++)
            disk[i] = SaveAsset(a[i], folder, $"Seq_Ambient_{i:D2}");

        // Pass 2: wire nextSequence between disk objects
        for (int i = 0; i < disk.Length - 1; i++)
        {
            disk[i].nextSequence = disk[i + 1];
            EditorUtility.SetDirty(disk[i]);
        }
        disk[disk.Length - 1].nextSequence = null;
        EditorUtility.SetDirty(disk[disk.Length - 1]);

        var diskStart          = disk[0];
        var diskTimerTrigger   = disk[idxTimerTrigger];
        var diskClickerTrigger = disk[idxClickerTrigger];

        // ══════════════════════════════════════════════════════════════
        //  TRIGGER A — one-shot (~48s)
        // ══════════════════════════════════════════════════════════════
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

        // ══════════════════════════════════════════════════════════════
        //  TRIGGER B — one-shot (~67s), secret passage / Stanley Parable
        // ══════════════════════════════════════════════════════════════
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
        trigA.restoreInterrupted = true;
        EditorUtility.SetDirty(trigA);
        trigB.restoreInterrupted = true;
        EditorUtility.SetDirty(trigB);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Auto-assign in ExplorationManager in scene
        int assignedCount = AutoAssignInScene(
            ambientStart:   diskStart,
            timerTrigger:   diskTimerTrigger,
            clickerTrigger: diskClickerTrigger,
            triggerA:       trigA,
            triggerB:       trigB);

        Debug.Log($"[ExplorationDialogueBuilder] ✓ Created/updated {a.Length + 2} assets in {folder}");
        string autoMsg = assignedCount > 0
            ? $"ExplorationManager in scene updated automatically ({assignedCount} fields)."
            : "⚠ ExplorationManager not found in open scene — assign manually.";
        EditorUtility.DisplayDialog("Done!",
            $"Updated {a.Length} ambient segments + 2 triggers.\n\n{autoMsg}",
            "OK");
    }

    /// <summary>
    /// Finds ExplorationManager in the open scene and wires all dialogue refs
    /// via SerializedObject. Returns number of updated fields.
    /// </summary>
    private static int AutoAssignInScene(
        DialogueSequence ambientStart,
        DialogueSequence timerTrigger,
        DialogueSequence clickerTrigger,
        DialogueSequence triggerA,
        DialogueSequence triggerB)
    {
        var mgr = Object.FindFirstObjectByType<ExplorationManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[ExplorationDialogueBuilder] ExplorationManager not found in scene.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("seqAmbientStart")  .objectReferenceValue = ambientStart;
        so.FindProperty("seqTimerTrigger")  .objectReferenceValue = timerTrigger;
        so.FindProperty("seqClickerTrigger").objectReferenceValue = clickerTrigger;
        so.FindProperty("seqTriggerA")      .objectReferenceValue = triggerA;
        so.FindProperty("seqTriggerB")      .objectReferenceValue = triggerB;
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            mgr.gameObject.scene);

        Debug.Log("[ExplorationDialogueBuilder] ExplorationManager updated: " +
                  "seqAmbientStart, seqTimerTrigger, seqClickerTrigger, seqTriggerA, seqTriggerB");
        return 5;
    }

    private static DialogueSequence CreateSeq(string folder, string name, int priority,
        DialogueSequence next, params DialogueLine[] lines)
    {
        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.priority = priority;
        seq.nextSequence = next;
        seq.lines = lines;
        var disk = SaveAsset(seq, folder, name);
        disk.nextSequence = next;
        EditorUtility.SetDirty(disk);
        return disk;
    }

    private static DialogueSequence SaveAsset(DialogueSequence seq, string folder, string name)
    {
        string path = $"{folder}/{name}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
        if (existing != null)
        {
            // Overwrite existing asset data — GUID preserved.
            existing.lines    = seq.lines;
            existing.priority = seq.priority;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        AssetDatabase.CreateAsset(seq, path);
        return seq;
    }

    private static DialogueLine L(string text, float pause, string activateObject = "")
    {
        return new DialogueLine { text = text, pauseAfter = pause, activateObject = activateObject };
    }
}
#endif
