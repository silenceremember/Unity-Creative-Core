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
/// Концепция:
///   Джон и Мэри знают что они в игре — действуют по скрипту, профессионально.
///   Рассказчик знает всё: и про персонажей, и про игрока.
///   Единственный, кто не понимает что происходит — сам игрок.
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

        // ── АКТ 1: УСТАНОВКА. ВСЁ ШТАТНО. ───────────────────────────────────────

        // Рассказчик представляет Джона — с наблюдениями, без лишней патетики
        var seqBeforeJohn1 = MakeSeq(folder, "Narr_BeforeJohn1",
            ("Рассказчик", "Это Джон Смит.", 0.4f),
            ("Рассказчик", "Он стоит у двери.", 0.3f),
            ("Рассказчик", "Так написано в скрипте.", 0.3f),
            ("Рассказчик", "Джон с этим согласился.", 0.3f),
            ("Рассказчик", "Выбора у него, впрочем, не было.", 0.7f)
        );

        // Рассказчик переходит к Мэри — наблюдает, а не перечисляет
        var seqBeforeMary1 = MakeSeq(folder, "Narr_BeforeMary1",
            ("Рассказчик", "Мэри Смит. Жена Джона.", 0.3f),
            ("Рассказчик", "Она на балконе — по сценарию.", 0.3f),
            ("Рассказчик", "Смотрит куда-то туда, в перспективу.", 0.4f),
            ("Рассказчик", "Там HDRI-карта. Мэри об этом знает.", 0.4f),
            ("Рассказчик", "Всё равно считает, что красиво.", 0.3f),
            ("Рассказчик", "Это либо характер, либо профессионализм.", 0.7f)
        );

        // Реакция на «Красиво здесь.» Мэри — рассказчик не спорит
        var seqBeforeDialogue = MakeSeq(folder, "Narr_BeforeDialogue",
            ("Рассказчик", "Возможно.", 0.4f),
            ("Рассказчик", "Джон согласен, хотя и не сказал этого.", 0.7f)
        );

        // ── АКТ 2: ИГРОК В СЦЕНЕ ──────────────────────────────────────────────────

        // Игрок появился. Рассказчик не в панике — спокойный профессионал
        var seqPlayerRevealed = MakeSeq(folder, "Narr_PlayerRevealed",
            ("Рассказчик", "А.", 0.5f),
            ("Рассказчик", "Вот и вы.", 0.4f),
            ("Рассказчик", "Чуть иначе чем ожидалось.", 0.3f),
            ("Рассказчик", "Никаких предупреждений — разумеется.", 0.4f),
            ("Рассказчик", "Типично.", 0.7f)
        );

        // Рассказчик объясняет игроку кто тот такой — пока Джон тактично ждёт
        var seqNarrAnswers = MakeSeq(folder, "Narr_Answers",
            ("Рассказчик", "По скрипту — да. Частично.", 0.4f),
            ("Рассказчик", "Вы — тот, кому выдадут управление в конце.", 0.4f),
            ("Рассказчик", "Джон и Мэри знали что кто-то придёт.", 0.3f),
            ("Рассказчик", "Что вы будете молчать — тоже знали.", 0.3f),
            ("Рассказчик", "Что именно вы — не знали.", 0.3f),
            ("Рассказчик", "Судя по вашему виду — вы тоже.", 0.4f),
            ("Рассказчик", "Ничего. Разберётесь.", 0.7f)
        );

        // Реакция на первое «...» игрока — рассказчик наблюдает, не даёт дирекций
        var seqBeforeTV = MakeSeq(folder, "Narr_BeforeTV",
            ("Рассказчик", "Молчащий протагонист.", 0.3f),
            ("Рассказчик", "Классика жанра.", 0.4f),
            ("Рассказчик", "Джон уже с этим работает.", 0.6f)
        );

        // Реакция на «Молчит.» Джона — рассказчик констатирует
        var seqBeforeMaryInput = MakeSeq(folder, "Narr_BeforeMaryInput",
            ("Рассказчик", "Исчерпывающий отчёт.", 0.7f)
        );

        // Рассказчик реагирует на телевизорную импровизацию Джона
        var seqNarrJohnTV = MakeSeq(folder, "Narr_JohnTV",
            ("Рассказчик", "Джон.", 0.3f),
            ("Рассказчик", "Телевизор.", 0.4f),
            ("Рассказчик", "Это не было в сценарии.", 0.3f),
            ("Рассказчик", "Мне нравится порыв.", 0.7f)
        );

        // ── АКТ 3: ЗАКРЫТИЕ СЦЕНЫ ────────────────────────────────────────────────

        // Реакция на «Я импровизировал.» — рассказчик просто принимает к сведению
        var seqNarrReact = MakeSeq(folder, "Narr_React",
            ("Рассказчик", "Охотно верю.", 0.7f)
        );

        // Рассказчик констатирует очевидное
        var seqYouTalk = MakeSeq(folder, "Narr_YouTalk",
            ("Рассказчик", "Ты разговариваешь, Джон.", 0.4f),
            ("Рассказчик", "Это поверх плана, но ладно.", 0.7f)
        );

        // Реакция на «Я многофункциональный.» — рассказчик не спорит
        var seqBeforeMary2 = MakeSeq(folder, "Narr_BeforeMary2",
            ("Рассказчик", "С этим не поспоришь.", 0.7f)
        );

        // Финальный монолог — личный, с иронией, тёплый итог
        var seqEnding = MakeSeq(folder, "Narr_Ending",
            ("Рассказчик", "Итак.", 0.5f),
            ("Рассказчик", "Вы только что наблюдали за тем, как Джон Смит стоит у двери.", 0.4f),
            ("Рассказчик", "Разговаривает с рассказчиком.", 0.3f),
            ("Рассказчик", "Встречает незнакомца в гостиной.", 0.3f),
            ("Рассказчик", "И спрашивает про телевизор.", 0.5f),
            ("Рассказчик", "Всё это — почти по плану.", 0.5f),
            ("Рассказчик", "Кроме вас.", 0.6f),
            ("Рассказчик", "Вы — сюрприз.", 0.4f),
            ("Рассказчик", "Приятный, будем надеяться.", 0.4f),
            ("Рассказчик", "Разработчик так и не уточнил.", 0.7f),
            ("Рассказчик", "В качестве следующего шага —", 0.3f),
            ("Рассказчик", "в буквальном смысле шага —", 0.3f),
            ("Рассказчик", "он разблокировал для вас:", 0.5f),
            ("Рассказчик", "Ходьбу.", 1.1f),
            ("Рассказчик", "WASD. Стики. На ваш выбор.", 0.4f),
            ("Рассказчик", "Квартира — ваша.", 0.4f),
            ("Рассказчик", "Джон и Мэри будут здесь.", 0.3f),
            ("Рассказчик", "Они уже всё знают.", 0.3f),
            ("Рассказчик", "Вам — только предстоит.", 0.9f)
        );

        // ── NOVEL SEQUENCE ────────────────────────────────────────────────────────
        var novelSeq = ScriptableObject.CreateInstance<NovelSequence>();
        novelSeq.lines = new NovelLine[]
        {
            // ── АКТ 1: ВСЁ ШТАТНО ──────────────────────────────────────────────
            new NovelLine { speaker = "Джон",     text = "Я стою у двери потому что...",       cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn1 },
            new NovelLine { speaker = "Джон",     text = "...потому что мне здесь хорошо.",    cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Красиво здесь.",                     cameraIndex = 1, narratorSequenceBefore = seqBeforeMary1 },

            // Диалог по сценарию — Джон чувствует присутствие в гостиной
            new NovelLine { speaker = "Джон",     text = "Мэри.",                              cameraIndex = 0, narratorSequenceBefore = seqBeforeDialogue },
            new NovelLine { speaker = "Мэри",     text = "Что.",                               cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "В гостиной кто-то есть.",            cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Ты уверен?",                        cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Я чувствую.",                        cameraIndex = 0 },

            // ── АКТ 2: ИГРОК В СЦЕНЕ ───────────────────────────────────────────
            // Рассказчик подтверждает — Джон смотрит на игрока
            new NovelLine { speaker = "Джон",     text = "Я тебя вижу.",                       cameraIndex = 2, narratorSequenceBefore = seqPlayerRevealed },
            // Джон спрашивает рассказчика — тот отвечает игроку
            new NovelLine { speaker = "Джон",     text = "Это по скрипту?",                    cameraIndex = 2 },
            // Рассказчик объясняет всё игроку. Один-на-один. Джон тактично ждёт.
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3, narratorSequenceBefore = seqNarrAnswers },

            // Рассказчик разрешает Джону поговорить. Джон делает лучшее что может.
            new NovelLine { speaker = "Джон",     text = "Ты умеешь чинить телевизоры?",       cameraIndex = 0, narratorSequenceBefore = seqBeforeTV },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3 },
            // Джон докладывает результат рассказчику.
            new NovelLine { speaker = "Джон",     text = "Молчит.",                            cameraIndex = 0 },

            // Мэри предлагает рабочую гипотезу. Джон сверяется со сценарием.
            new NovelLine { speaker = "Мэри",     text = "Может, мастер?",                     cameraIndex = 1, narratorSequenceBefore = seqBeforeMaryInput },
            new NovelLine { speaker = "Джон",     text = "Мастеров в плане не было.",           cameraIndex = 0 },
            // Лучшая реплика сцены. Мэри принимает хаос как рабочую норму.
            new NovelLine { speaker = "Мэри",     text = "В плане много чего не было сегодня.", cameraIndex = 1 },

            // ── АКТ 3: ЗАКРЫТИЕ ────────────────────────────────────────────────
            // Рассказчик реагирует на телевизорную импровизацию. Джон признаётся.
            new NovelLine { speaker = "Джон",     text = "Я импровизировал.",                  cameraIndex = 0, narratorSequenceBefore = seqNarrJohnTV },
            // Рассказчик пытается вернуть Джона к исходной позиции.
            new NovelLine { speaker = "Джон",     text = "Я стою.",                            cameraIndex = 0, narratorSequenceBefore = seqNarrReact },
            new NovelLine { speaker = "Джон",     text = "Я стою и разговариваю.",             cameraIndex = 0, narratorSequenceBefore = seqYouTalk },
            new NovelLine { speaker = "Джон",     text = "Я многофункциональный.",             cameraIndex = 0 },

            // Рассказчик передаёт Мэри. Она закрывает сцену достойно.
            new NovelLine { speaker = "Мэри",     text = "Джон.",                              cameraIndex = 1, narratorSequenceBefore = seqBeforeMary2 },
            new NovelLine { speaker = "Мэри",     text = "Впусти человека нормально.",         cameraIndex = 1 },
            new NovelLine { speaker = "Мэри",     text = "Или выпусти. Определись.",           cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Пусть пока побудет.",                cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Здесь не так много всего.",          cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Но что-то есть.",                    cameraIndex = 0 },

            // Финальный монолог рассказчика → конец катсцены
            new NovelLine { speaker = "",         text = "",                                    cameraIndex = 3, narratorSequenceBefore = seqEnding },
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
