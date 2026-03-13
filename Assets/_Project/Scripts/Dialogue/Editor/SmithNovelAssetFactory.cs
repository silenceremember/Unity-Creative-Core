using UnityEngine;
using UnityEditor;

/// <summary>
/// Создаёт все SO-ассеты визуальной новеллы «The Smith Family».
/// Window → Visual Novel → Create Smith Family Assets
///
/// Камеры:
///   0 = John Novel Camera   (Джон у двери)
///   1 = Jane Novel Camera   (Мэри на балконе)
///   2 = Player with John    (игрок + Джон в гостиной)
///   3 = Player Focus        (крупный план на игрока)
///
/// Поток:
///   1. Рассказчик представляет Джона → Джон говорит → иронизирует над рассказчиком.
///   2. Рассказчик кратко отвечает, затем вводит Мэри → Мэри говорит.
///   3. Динамика Джон–Мэри: в гостиной кто-то есть.
///   4. Рассказчик вводит игрока → Player 1: «...»
///   5. Персонажи пытаются разобраться кто это.
///   6. Джон общается с рассказчиком (серия реплик).
///   7. Финал → монолог → ходьба.
///
/// Правило: в NovelLine.speaker НИКОГДА не «Рассказчик».
/// </summary>
public class SmithNovelAssetFactory : EditorWindow
{
    [MenuItem("Window/Visual Novel/Create Smith Family Assets")]
    public static void CreateAssets()
    {
        const string folder = "Assets/_Project/Dialogue/VisualNovel";

        var channel = ScriptableObject.CreateInstance<NovelChannel>();
        AssetDatabase.CreateAsset(channel, $"{folder}/NovelChannel.asset");

        // ── NARRATOR SEQUENCES ────────────────────────────────────────────────────

        // Рассказчик представляет Джона
        var seqIntroJohn = MakeSeq(folder, "Narr_IntroJohn",
            ("Рассказчик", "Это Джон Смит.",                                   1.0f),
            ("Рассказчик", "Он стоит у двери.",                                1.2f),
            ("Рассказчик", "Точнее, думает, что стоит по своей воле.",         1.8f),
            ("Рассказчик", "Но на самом деле — это Разработчик...",            1.8f),
            ("Рассказчик", "Написал что он стоит у двери.",                    1.5f)
        );

        // Рассказчик не соглашается с «мне здесь хорошо» — играет перед иронией Джона
        var seqNarrDisagrees = MakeSeq(folder, "Narr_Disagrees",
            ("Рассказчик", "Это неправда.", 1.0f)
        );

        // Рассказчик кратко отвечает Джону, затем вводит Мэри
        var seqIntroMary = MakeSeq(folder, "Narr_IntroMary",
            ("Рассказчик", "Понятно.",                                               0.8f),
            ("Рассказчик", "Это Мэри Смит. Жена Джона.",                            1.2f),
            ("Рассказчик", "Она на балконе. Смотрит вдаль.",                        1.2f),
            ("Рассказчик", "Разработчик не прописал что там во дворе.",              1.8f),
            ("Рассказчик", "Поэтому вдаль — это просто HDRI карта.",                1.5f),
            ("Рассказчик", "Хотя, по мнению Мэри, красивая HDRI карта.",            1.5f)
        );

        // Реакция на «Я... чувствую.» + ввод игрока — одна непрерывная последовательность
        var seqIntroYou = MakeSeq(folder, "Narr_IntroYou",
            ("Рассказчик", "А это вы.",                                             1.0f),
            ("Рассказчик", "Появились в гостиной. Без объяснений.",                1.2f),
            ("Рассказчик", "Я тоже не понимаю как вы сюда попали.",                2.0f),
            ("Рассказчик", "У Разработчика явно была какая-то идея.",              1.5f)
        );

        // Реакция на «Не помню.» Мэри — перед «Эй. Ты кто?»
        var seqAfterNotRemember = MakeSeq(folder, "Narr_AfterNotRemember",
            ("Рассказчик", "Она не вызывала.",                                      1.0f),
            ("Рассказчик", "Воспоминаний до этой сцены у неё нет.",                1.8f),
            ("Рассказчик", "Откуда взялись вы, тоже не вполне ясно.",              1.8f)
        );

        // Реакция на молчание игрока — перед вопросом про телевизор
        var seqAfterPlayerSilence = MakeSeq(folder, "Narr_AfterPlayerSilence",
            ("Рассказчик", "Игрок молчит.",                        0.8f),
            ("Рассказчик", "Джон работает с тем что есть.",        1.5f)
        );

        // Диалог рассказчика и Джона — серия коротких обменов
        var seqJohnNarr1 = MakeSeq(folder, "Narr_JohnExchange1",
            ("Рассказчик", "Это не так работает, Джон.", 1.2f)
        );

        var seqJohnNarr2 = MakeSeq(folder, "Narr_JohnExchange2",
            ("Рассказчик", "Ты должен был просто стоять у двери.", 1.8f)
        );

        var seqJohnNarr3 = MakeSeq(folder, "Narr_JohnExchange3",
            ("Рассказчик", "Ты разговариваешь.", 1.0f)
        );

        // Финальный монолог
        var seqEnding = MakeSeq(folder, "Narr_Ending",
            ("Рассказчик", "Похоже...",                                                   0.8f),
            ("Рассказчик", "Они так и не разобрались кто вы.",                           1.8f),
            ("Рассказчик", "Вы, впрочем, скорее всего, тоже.",                           1.2f),
            ("Рассказчик", "В любом случае.",                                             1.0f),
            ("Рассказчик", "Вы только что поиграли в визуальную новеллу.",               1.8f),
            ("Рассказчик", "Разработчик считает, что это, как правило, скучно.",         1.8f),
            ("Рассказчик", "Сидеть. Читать. Нажимать «Далее».",                          1.2f),
            ("Рассказчик", "Не очень-то захватывает.",                                   1.0f),
            ("Рассказчик", "Поэтому, следующим шагом...",                                1.0f),
            ("Рассказчик", "В буквальном смысле шагом.",                                 1.0f),
            ("Рассказчик", "Он разблокировал для вас...",                                1.0f),
            ("Рассказчик", "Ходьбу.",                                                     1.1f),
            ("Рассказчик", "WASD. Любая кнопка на ваш выбор.",                          1.5f),
            ("Рассказчик", "Перед вами комната.",                                         1.0f),
            ("Рассказчик", "Ходите. Смотрите на вещи.",                                  1.0f)
        );

        // ── NOVEL SEQUENCE ────────────────────────────────────────────────────────
        var novelSeq = ScriptableObject.CreateInstance<NovelSequence>();
        novelSeq.lines = new NovelLine[]
        {
            // ── БЛОК 1: РАССКАЗЧИК ВВОДИТ ДЖОНА ────────────────────────────────
            new NovelLine { speaker = "Джон",     text = "Я стою у двери потому что...",       cameraIndex = 0, narratorSequenceBefore = seqIntroJohn },
            new NovelLine { speaker = "Джон",     text = "...потому что мне здесь хорошо.",    cameraIndex = 0 },
            // Рассказчик: «Это неправда.» → Джон иронизирует
            new NovelLine { speaker = "Джон",     text = "Слышу скепсис.",                     cameraIndex = 0, narratorSequenceBefore = seqNarrDisagrees },

            // ── БЛОК 2: РАССКАЗЧИК ОТВЕЧАЕТ И ВВОДИТ МЭРИ ──────────────────────
            // seqIntroMary начинается с «Понятно.» — ответ на иронию Джона
            new NovelLine { speaker = "Мэри",     text = "Красиво здесь.",                     cameraIndex = 1, narratorSequenceBefore = seqIntroMary },

            // ── БЛОК 3: ПЕРСОНАЖИ ЗАМЕЧАЮТ ПРИСУТСТВИЕ ──────────────────────────
            new NovelLine { speaker = "Джон",     text = "Мэри.",                              cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Что.",                               cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "В гостиной кто-то есть.",            cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Ты уверен?",                        cameraIndex = 1 },
            // Рассказчик: «Он не чувствует...» — и сразу Джон: «Я... чувствую.»
            new NovelLine { speaker = "Джон",     text = "Я... чувствую. Там кто-то есть.",   cameraIndex = 0 },

            // Рассказчик вводит игрока — прямо после слов Джона
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3, narratorSequenceBefore = seqIntroYou },

            // ── БЛОК 5: ВЕРСИЯ О МАСТЕРЕ ────────────────────────────────────────
            new NovelLine { speaker = "Мэри",     text = "Может, это мастер по телевизору?",   cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Я не вызывал мастера.",              cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Я вызывала.",                        cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Когда?",                             cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Не помню.",                          cameraIndex = 1 },
            // Рассказчик: «Она не вызывала. Воспоминаний до этой сцены нет.»
            new NovelLine { speaker = "Джон",     text = "Эй. Ты кто?",                       cameraIndex = 2, narratorSequenceBefore = seqAfterNotRemember },

            // ── БЛОК 6: ИГРОК МОЛЧИТ — ДЖОН ИМПРОВИЗИРУЕТ ──────────────────────
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3 },
            // Рассказчик: «Игрок молчит. Джон работает с тем что есть.»
            new NovelLine { speaker = "Джон",     text = "Ты умеешь чинить телевизоры?",       cameraIndex = 2, narratorSequenceBefore = seqAfterPlayerSilence },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3 },
            new NovelLine { speaker = "Джон",     text = "Возможно это знак согласия.",          cameraIndex = 2 },

            // ── БЛОК 7: ДИАЛОГ ДЖОНА С РАССКАЗЧИКОМ ─────────────────────────────
            new NovelLine { speaker = "Джон",     text = "А как работает?",                    cameraIndex = 0, narratorSequenceBefore = seqJohnNarr1 },
            new NovelLine { speaker = "Джон",     text = "Я стою.",                            cameraIndex = 0, narratorSequenceBefore = seqJohnNarr2 },
            new NovelLine { speaker = "Джон",     text = "Я стою и разговариваю.",             cameraIndex = 0, narratorSequenceBefore = seqJohnNarr3 },
            new NovelLine { speaker = "Джон",     text = "Я многофункциональный.",             cameraIndex = 0 },

            // ── ФИНАЛ ───────────────────────────────────────────────────────────
            new NovelLine { speaker = "Мэри",     text = "Джон.",                              cameraIndex = 1 },
            new NovelLine { speaker = "Мэри",     text = "Он вообще уйдёт?",                  cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Не знаю.",                           cameraIndex = 0 },
            new NovelLine { speaker = "Джон",     text = "Спрашивать бесполезно, он молчит.", cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Понятно.",                           cameraIndex = 1 },
            new NovelLine { speaker = "Мэри",     text = "Подождём пока сам разберётся.",      cameraIndex = 1 },

            // Финальный монолог рассказчика → конец катсцены
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3, narratorSequenceBefore = seqEnding },
        };

        AssetDatabase.CreateAsset(novelSeq, $"{folder}/NovelSeq_SmithOpening.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SmithNovelAssetFactory] Done — assets in " + folder);
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = novelSeq;
    }

    // ── Helper ────────────────────────────────────────────────────────────────────

    private static DialogueSequence MakeSeq(string folder, string name,
        params (string speaker, string text, float pauseAfter)[] lines)
    {
        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.lines = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            seq.lines[i] = new DialogueLine
            {
                speaker    = lines[i].speaker,
                text       = lines[i].text,
                pauseAfter = lines[i].pauseAfter,
                duration   = 0f
            };
        AssetDatabase.CreateAsset(seq, $"{folder}/{name}.asset");
        return seq;
    }
}
