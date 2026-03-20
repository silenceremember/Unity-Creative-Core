
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Создаёт все DialogueSequence-ассеты для Exploration фазы (60 сек).
/// Меню: Tools → Create Exploration Dialogues
/// </summary>
public static class ExplorationDialogueBuilder
{
    [MenuItem("Tools/Create Exploration Dialogues")]
    public static void Build()
    {
        const string folder = "Assets/_Project/Dialogue/Exploration";

        // Убедимся, что папки существуют
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Dialogue", "Exploration");

        // ══════════════════════════════════════════════════════════════
        //  AMBIENT — линейная цепочка.
        //  Квест запускается по завершению последнего сегмента
        //  (nextSequence == null → ExplorationManager стартует квест).
        //  Chain: A[0] → A[1] → … → A[19] → null
        // ══════════════════════════════════════════════════════════════
        var a = new DialogueSequence[45];
        for (int i = 0; i < a.Length; i++)
            a[i] = ScriptableObject.CreateInstance<DialogueSequence>();

        // Сегмент 0 — первое впечатление
        a[0].lines = new[]
        {
            L("Итак.", 1.5f),
            L("Вы находитесь в чужом доме.", 2.5f),
            L("Никто Вас не звал.", 2f),
        };

        // Сегмент 1 — о природе присутствия
        a[1].lines = new[]
        {
            L("Впрочем, в таких играх всегда так.", 3f),
            L("Просто оказываетесь и смотрите.", 2f),
            L("Ничего не трогаете.", 2f),
        };

        // Сегмент 2 — о механиках
        a[2].lines = new[]
        {
            L("Хотя трогать и нечего.", 2f),
            L("Всё заблокировано.", 1.5f),
            L("Разработчик не придумал, что с этим делать.", 3f),
        };

        // Сегмент 3 — о разработчике
        a[3].lines = new[]
        {
            L("Он думал, атмосферы будет достаточно.", 3f),
            L("Может, и будет.", 1.5f),
            L("Я не судья.", 1.5f),
        };

        // Сегмент 4 — о Мэри
        a[4].lines = new[]
        {
            L("На балконе Мэри.", 2f),
            L("По задумке сильная, со своим мнением.", 3f),
        };

        // Сегмент 5 — Мэри глубже
        a[5].lines = new[]
        {
            L("Пока что она просто стоит.", 2.5f),
            L("Мнение, видимо, ещё в разработке.", 3f),
        };

        // Сегмент 6 — о Джоне
        a[6].lines = new[]
        {
            L("Джон в дверном проёме. Как всегда.", 2.5f),
            L("Он бы и рад уйти, но нет анимации.", 3.5f),
        };

        // Сегмент 7 — о паре
        a[7].lines = new[]
        {
            L("Они пара с историей.", 2.5f),
            L("Что за история, узнаете позже.", 2.5f),
            L("Обещает Разработчик.", 1.5f),
        };

        // Сегмент 8 — скептицизм
        a[8].lines = new[]
        {
            L("«Позже» всегда стандартный ответ.", 2f),
            L("Как «в планах» и «баг скоро починят».", 2.5f),
        };

        // Сегмент 9 — о конфликте
        a[9].lines = new[]
        {
            L("Впрочем, тут есть что смотреть.", 2f),
            L("Картины, например.", 1.5f),
        };

        // Сегмент 10 — о рассказчике
        a[10].lines = new[]
        {
            L("Вообще, моя роль...", 3f),
            L("Просто говорить, пока Вы ходите.", 2f),
            L("Объяснять очевидное.", 1.5f),
            L("Да, тяжёлая работа.", 3f),
        };

        // Сегмент 11 — о жанре
        a[11].lines = new[]
        {
            L("Жанр таких игр называется Walking Simulator.", 3f),
            L("Вы ходите. Я говорю.", 1.5f),
            L("Получается атмосферно.", 2.5f),
        };

        // Сегмент 12 — о тишине
        a[12].lines = new[]
        {
            L("Тишина, кстати, тоже нарратив.", 2f),
            L("Но если я замолчу...", 1.5f),
            L("Станет слишком скучно.", 5f),
        };

        // Сегмент 13 — о повторном прохождении
        a[13].lines = new[]
        {
            L("Также...", 1f),
            L("Если Вы поиграете во второй раз...", 1.5f),
            L("Я скажу всё то же самое.", 2.5f),
            L("Слово в слово.", 1.5f),
            L("Такова природа Рассказчика.", 2.5f),
        };

        // Сегмент 14 — о поиске смысла
        a[14].lines = new[]
        {
            L("Хотите знать, зачем Вы здесь?", 2f),
            L("Нормальный вопрос.", 1.5f),
            L("Я сам не очень понимаю.", 2.5f),
        };

        // Сегмент 15 — о доме
        a[15].lines = new[]
        {
            L("Дом, в общем, хороший.", 2.5f),
            L("Видно, что Разработчик старался.", 2f),
            L("Есть детали.", 1.5f),
        };

        // Сегмент 16 — о шейдерах и материалах
        a[16].lines = new[]
        {
            L("Посмотрите на стены.", 1.5f),
            L("Разработчик разобрался в шейдерах.", 2.0f),
            L("Не идеально. Но и не стоковый материал.", 2.5f),
        };

        // Сегмент 17 — о свете
        a[17].lines = new[]
        {
            L("Свет, кстати, запечённый.", 1.5f),
            L("Lightmap. Глобальное освещение.", 2.0f),
            L("Звучит проще, чем выглядит в настройках.", 2.5f),
        };

        // Сегмент 18 — об аудио
        a[18].lines = new[]
        {
            L("А звуки?", 1.5f),
            L("Разработчик потратил на них больше времени...", 2.0f),
            L("Чем на геймплей, по правде говоря.", 2.5f),
        };

        // Сегмент 19 — небольшое примирение с реальностью
        a[19].lines = new[]
        {
            L("Может, и не нужна механика.", 1.5f),
            L("Иногда достаточно просто побыть где-нибудь.", 3.5f),
            L("Не в той реальности, в которой находитесь.", 3.5f),
            L("Это называется эскапизм.", 1.5f),
        };

        // Сегмент 20 — лёгкая грусть
        a[20].lines = new[]
        {
            L("Мэри и Джон никуда не уйдут.", 2f),
            L("Они будут здесь всегда.", 2f),
            L("У них нет возможностей для эскапизма.", 2f),
            L("Но есть что-то в этом.", 2f),
        };

        // Сегмент 21 — рассказчик скучает
        a[21].lines = new[]
        {
            L("Похоже, становится скучновато.", 2f),
            L("Не знаю, смогу ли я Вас ещё долго развлекать.", 3f),
            L("Но Разработчик выделил мне несколько инструментов.", 2.5f),
        };

        // Сегмент 22 — ТАЙМЕР (назначь это поле в seqTimerTrigger в ExplorationManager)
        // После завершения этого сегмента ExplorationManager запустит декоративный таймер
        a[22].lines = new[]
        {
            L("Смотрите...", 1.5f),
            L("Таймер!", 2.5f),
        };

        // Сегмент 23 — объяснение таймера
        a[23].lines = new[]
        {
            L("Это среднее время прохождения игры.", 2f),
            L("Точнее, её оставшейся части.", 2f),
            L("Это время среднего игрока.", 2f),
            L("Не Ваше.", 2f),
        };

        // Сегмент 24 — успокоение
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

        // Сегмент 25 — таймер скучен, появляется кликер
        a[25].lines = new[]
        {
            L("Ладно. Похоже, таймер Вас не впечатлил.", 3f),
            L("А что насчёт кликера?", 2f),
        };

        // Сегмент 26 — объяснение кликера
        a[26].lines = new[]
        {
            L("Это обычный счётчик.", 2f),
            L("При нажатии ЛКМ издаёт приятный звук.", 2f),
            L("Это уже действительно интересно.", 2f),
        };

        // Сегмент 27 — рассуждение о жанре
        a[27].lines = new[]
        {
            L("Забавно.", 1.5f),
            L("Кто-нибудь объединял симулятор камня с кликером?", 3.5f),
            L("Или симулятор ходьбы с кликером?", 2f),
            L("Не знаю.", 1.5f),
            L("Получилось бы что-то особенное.", 2f),
        };

        // Сегмент 28 — болтовня про кликеры (~минута)
        a[28].lines = new[]
        {
            L("Cookie Clicker в своё время сломал людей.", 3f),
            L("Просто печенье. Просто клики.", 2.5f),
            L("Миллионы часов.", 2f),
        };

        // Сегмент 29 — болтовня
        a[29].lines = new[]
        {
            L("Что-то в этом есть.", 2f),
            L("Примитивный ритм.", 2f),
            L("Клик. Звук. Число растёт.", 2.5f),
        };

        // Сегмент 30 — болтовня
        a[30].lines = new[]
        {
            L("Немного похоже на ходьбу по комнате.", 2f),
            L("Шаг. Шаг. Шаг.", 1f),
            L("Вы, собственно, этим и занимаетесь.", 2f),
        };

        // Сегмент 31 — болтовня
        a[31].lines = new[]
        {
            L("Разработчик мог бы сделать игру про клики.", 3f),
            L("Но выбрал сделать это.", 2f),
            L("Поскольку, считает он...", 1.5f),
            L("Это продемонстрирует его навыки.", 2f),
            L("Уважаю.", 1.5f),
        };

        // Сегмент 32 — болтовня
        a[32].lines = new[]
        {
            L("Хотя разница невелика.", 1.5f),
            L("И там, и тут бесцельное действие.", 2f),
            L("Просто одно немного интереснее другого.", 2f),
        };

        // Сегмент 33 — болтовня
        a[33].lines = new[]
        {
            L("Кстати, как Вам кликер?", 2.5f),
        };

        // Сегмент 34 — болтовня
        a[34].lines = new[]
        {
            L("Я бы тоже попробовал.", 1.5f),
            L("Но у меня нет рук.", 2.5f),
            L("Только поле текста Text Mesh Pro.", 3f),
        };

        // Сегмент 35 — дополнительная минута кликера
        a[35].lines = new[]
        {
            L("Знаете, в чём прелесть простых кликеров?", 3f),
            L("Они честны.", 1.5f),
            L("Вот Вы кликаете, и видите, что происходит.", 3f),
        };

        // Сегмент 36 — болтовня
        a[36].lines = new[]
        {
            L("Нет скрытой цели.", 2f),
            L("Нет награды за достижения.", 2.5f),
            L("Только звук и число.", 2f),
            L("Хотя сейчас всё принято пичкать...", 2f),
            L("Метапрогрессией.", 2f),
        };

        // Сегмент 37 — болтовня
        a[37].lines = new[]
        {
            L("Но раньше...", 2f),
            L("Всё равно кто-то доходил до миллиарда.", 2f),
            L("И что?", 1.5f),
            L("И продолжал кликать.", 2f),
        };

        // Сегмент 38 — болтовня
        a[38].lines = new[]
        {
            L("Я думаю, человечество ищет успокоение...", 3f),
            L("В неожиданных местах.", 2f),
            L("И действительно иногда находит.", 2f),
        };

        // Сегмент 39 — болтовня
        a[39].lines = new[]
        {
            L("Я читал, что idle-игры редуцируют тревогу.", 3.5f),
            L("Процесс успокаивает.", 2f),
            L("Может, и здесь что-то в этом есть.", 3f),
        };

        // Сегмент 40 — болтовня
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

        // Сегмент 41 — кликер надоел
        a[41].lines = new[]
        {
            L("Ладно.", 1.5f),
            L("Похоже, кликер становится скучноват.", 3f),
            L("Как и таймер.", 2f),
        };

        // Сегмент 42 — переход к квесту
        a[42].lines = new[]
        {
            L("Что насчёт небольшого квеста?", 3f),
            L("...", 1f),
        };

        // Сегмент 43 — ужас картин (картины сдвигаются при первой реплике)
        a[43].lines = new[]
        {
            L("...Какой ужас.", 2f, "PaintingShift"),
            L("Картины магическим образом наклонились.", 3f),
            L("Их надо поместить на прежнее место.", 3f),
        };

        // Сегмент 44 — финал цепочки, квест стартует (nextSequence = null)
        // Квест-канвас появляется вместе с последней репликой
        a[44].lines = new[]
        {
            L("Помогите жильцам разобраться с этим.", 4f, "QuestCanvas"),
        };

        // Ключевые индексы для авто-назначения в ExplorationManager
        const int idxTimerTrigger   = 22;
        const int idxClickerTrigger = 25;

        foreach (var seg in a)
            seg.priority = 0;

        // ── Проход 1: сохраняем/перезаписываем данные на диск ──────────
        // Важно: nextSequence НЕ прошиваем пока — ссылки ещё in-memory.
        // SaveAsset возвращает реальный дисковый объект (с живым GUID).
        var disk = new DialogueSequence[a.Length];
        for (int i = 0; i < a.Length; i++)
            disk[i] = SaveAsset(a[i], folder, $"Seq_Ambient_{i:D2}");

        // ── Проход 2: прошиваем nextSequence между ДИСКОВЫМИ объектами ─
        for (int i = 0; i < disk.Length - 1; i++)
        {
            disk[i].nextSequence = disk[i + 1];
            EditorUtility.SetDirty(disk[i]);
        }
        disk[disk.Length - 1].nextSequence = null;
        EditorUtility.SetDirty(disk[disk.Length - 1]);

        // Переключаемся на дисковые объекты для авто-назначения
        // (AutoAssignInScene получит корректные ссылки)
        var diskStart          = disk[0];
        var diskTimerTrigger   = disk[idxTimerTrigger];
        var diskClickerTrigger = disk[idxClickerTrigger];

        // ══════════════════════════════════════════════════════════════
        //  TRIGGER A — замечание о перфекционизме (~48с), одноразовый
        //  nextSequence = null: ExplorationManager сам возобновит ambient
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
        //  TRIGGER B — одноразовый (~67с), секретный лаз / Stanley Parable
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

        // Триггеры A и B восстанавливают прерванный диалог (ambient / quest / XP / любой)
        trigA.restoreInterrupted = true;
        EditorUtility.SetDirty(trigA);
        trigB.restoreInterrupted = true;
        EditorUtility.SetDirty(trigB);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Авто-назначение в ExplorationManager в сцене ─────────────
        int assignedCount = AutoAssignInScene(
            ambientStart:   diskStart,
            timerTrigger:   diskTimerTrigger,
            clickerTrigger: diskClickerTrigger,
            triggerA:       trigA,
            triggerB:       trigB);

        Debug.Log($"[ExplorationDialogueBuilder] ✓ Создано/обновлено {a.Length + 2} ассетов в {folder}");
        string autoMsg = assignedCount > 0
            ? $"ExplorationManager в сцене обновлён автоматически ({assignedCount} поля)."
            : "⚠ ExplorationManager не найден в открытой сцене — назначь вручную.";
        EditorUtility.DisplayDialog("Готово!",
            $"Обновлено {a.Length} ambient-сегментов + 2 триггера.\n\n{autoMsg}",
            "OK");
    }

    // ── Авто-назначение ────────────────────────────────────────────────

    /// <summary>
    /// Находит ExplorationManager в открытой сцене и через SerializedObject
    /// прописывает все dialogue-ссылки. Возвращает количество обновлённых полей.
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
            Debug.LogWarning("[ExplorationDialogueBuilder] ExplorationManager не найден в сцене.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("seqAmbientStart")  .objectReferenceValue = ambientStart;
        so.FindProperty("seqTimerTrigger")  .objectReferenceValue = timerTrigger;
        so.FindProperty("seqClickerTrigger").objectReferenceValue = clickerTrigger;
        so.FindProperty("seqTriggerA")      .objectReferenceValue = triggerA;
        so.FindProperty("seqTriggerB")      .objectReferenceValue = triggerB;
        so.ApplyModifiedProperties();

        // Помечаем сцену как изменённую (чтобы Unity предложила сохранить)
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            mgr.gameObject.scene);

        Debug.Log("[ExplorationDialogueBuilder] ExplorationManager обновлён: " +
                  "seqAmbientStart, seqTimerTrigger, seqClickerTrigger, seqTriggerA, seqTriggerB");
        return 5;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static DialogueSequence CreateSeq(string folder, string name, int priority,
        DialogueSequence next, params DialogueLine[] lines)
    {
        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.priority = priority;
        seq.nextSequence = next; // для триггеров next = null, поэтому in-memory ссылка не нужна
        seq.lines = lines;
        var disk = SaveAsset(seq, folder, name);
        // Прошиваем nextSequence на дисковом объекте (для триггеров это null)
        disk.nextSequence = next;
        EditorUtility.SetDirty(disk);
        return disk; // возвращаем дисковый объект, а не временный
    }

    private static DialogueSequence SaveAsset(DialogueSequence seq, string folder, string name)
    {
        string path = $"{folder}/{name}.asset";

        var existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
        if (existing != null)
        {
            // Перезаписываем данные существующего ассета — GUID сохраняется.
            // nextSequence специально НЕ копируем здесь — он прошивается отдельно
            // в проходе 2 через дисковые объекты (иначе ссылки были бы на in-memory мусор).
            existing.lines    = seq.lines;
            existing.priority = seq.priority;
            // nextSequence — будет задан снаружи в проходе 2
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
