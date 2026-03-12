using UnityEngine;
using UnityEditor;

/// <summary>
/// Создаёт все SO-ассеты визуальной новеллы «The Smith Family».
/// Window → Visual Novel → Create Smith Family Assets
///
/// Правило: в NovelLine.speaker НИКОГДА не бывает «Рассказчик».
/// Рассказчик говорит через NarratorCanvas (NarratorManager) 
/// — до того, как NovelCanvas открывается — через поле narratorSequenceBefore.
/// </summary>
public class SmithNovelAssetFactory : EditorWindow
{
    [MenuItem("Window/Visual Novel/Create Smith Family Assets")]
    public static void CreateAssets()
    {
        const string folder = "Assets/_Project/Dialogue/VisualNovel";

        // ── NovelChannel ──────────────────────────────────────────────────────────
        var channel = ScriptableObject.CreateInstance<NovelChannel>();
        AssetDatabase.CreateAsset(channel, $"{folder}/NovelChannel.asset");

        // ── Narrator sequences ────────────────────────────────────────────────────
        // Каждая — то что рассказчик говорит ПЕРЕД конкретной репликой персонажа.
        // NovelCanvas скрыт всё это время.

        // Перед «Джон: Я стою у двери потому что...»
        var seqBeforeJohn1 = MakeSeq(folder, "Narr_BeforeJohn1",
            ("Рассказчик", "Это Джон Смит.", 0.3f),
            ("Рассказчик", "Он стоит у двери.", 0.3f),
            ("Рассказчик", "Он не знает почему.", 0.3f),
            ("Рассказчик", "Он думает, что знает.", 0.3f),
            ("Рассказчик", "Но на самом деле — это Разработчик", 0.2f),
            ("Рассказчик", "Написал что он стоит у двери.", 0.5f)
        );

        // Перед «Мэри: (не оборачиваясь) Красиво здесь.»
        // Объединяем «Это неправда. Но пусть будет.» + вводную про Мэри
        var seqBeforeMary1 = MakeSeq(folder, "Narr_BeforeMary1",
            ("Рассказчик", "Это неправда. Но пусть будет.", 0.6f),
            ("Рассказчик", "Это Мэри Смит. Жена Джона.", 0.3f),
            ("Рассказчик", "Она на балконе. Смотрит вдаль.", 0.3f),
            ("Рассказчик", "Разработчик не прописал что там во дворе.", 0.3f),
            ("Рассказчик", "Поэтому вдаль — это просто HDRI карта.", 0.5f)
        );

        // Перед «Джон: Мэри.» — объединяем «Возможно.» + вводную про игрока
        var seqBeforeJohn2 = MakeSeq(folder, "Narr_BeforeJohn2",
            ("Рассказчик", "Возможно.", 0.7f),
            ("Рассказчик", "А это — вы.", 0.3f),
            ("Рассказчик", "Вы появились здесь без объяснений.", 0.3f),
            ("Рассказчик", "Я тоже не понимаю как вы сюда попали.", 0.3f),
            ("Рассказчик", "У Разработчика явно была какая-то идея.", 0.7f)
        );

        // Перед «Джон: Это было неприятно.»
        var seqBeforeJohn3 = MakeSeq(folder, "Narr_BeforeJohn3",
            ("Рассказчик", "Он не чувствует.", 0.3f),
            ("Рассказчик", "Разработчик добавил следующую сцену", 0.2f),
            ("Рассказчик", "с игроком — и Джон это увидел.", 0.5f)
        );

        // Перед «Джон: Эй. Ты кто?» (камера переходит к игроку)
        var seqBeforePlayer1 = MakeSeq(folder, "Narr_BeforePlayer1",
            ("Рассказчик", "Она не вызывала.", 0.3f),
            ("Рассказчик", "У неё нет воспоминаний до этой сцены.", 0.3f),
            ("Рассказчик", "Разработчик явно не подумал об этом.", 0.7f)
        );

        // Перед «Джон: Ты умеешь чинить телевизоры?»
        var seqBeforeJohn4 = MakeSeq(folder, "Narr_BeforeJohn4",
            ("Рассказчик", "Игрок молчит.", 0.3f),
            ("Рассказчик", "Как правило они молчат. Это нормально.", 0.3f),
            ("Рассказчик", "Разработчик мог бы дать вам реплику.", 0.3f),
            ("Рассказчик", "Но это требует времени и ресурсов.", 0.7f)
        );

        // Перед «Джон: А как работает?»
        var seqBeforeJohn5 = MakeSeq(folder, "Narr_BeforeJohn5",
            ("Рассказчик", "Это не так работает, Джон.", 0.8f)
        );

        // Перед «Джон: Я стою.»
        var seqBeforeJohn6 = MakeSeq(folder, "Narr_BeforeJohn6",
            ("Рассказчик", "Ты должен был просто стоять у двери.", 0.8f)
        );

        // Перед «Джон: Я стою и разговариваю.»
        var seqBeforeJohn7 = MakeSeq(folder, "Narr_BeforeJohn7",
            ("Рассказчик", "Ты разговариваешь.", 0.8f)
        );

        // Финальный нарраторский монолог — перед последней пустой строкой (конец катсцены)
        var seqEnding = MakeSeq(folder, "Narr_Ending",
            ("Рассказчик", "Да. Что-то есть.", 0.4f),
            ("Рассказчик", "Честно говоря, я думал это будет хуже.", 0.7f),
            ("Рассказчик", "Возможность ходить — это уже что-то.", 0.3f),
            ("Рассказчик", "Симулятор камня такого не предлагает.", 0.3f),
            ("Рассказчик", "Разработчик считает это победой.", 0.3f),
            ("Рассказчик", "Я склонен согласиться.", 0.8f),
            ("Рассказчик", "Удачи. Или нет. Посмотрим.", 1.0f)
        );

        // ── NovelSequence ─────────────────────────────────────────────────────────
        // Камеры:  0 = дверь (Джон)   1 = балкон (Мэри)   2 = гостиная (Игрок)   3 = крупный план
        // speaker НИКОГДА не "Рассказчик"
        // speaker "" = тихий кадр, NovelCanvas показывает только кнопку «Далее»

        var novelSeq = ScriptableObject.CreateInstance<NovelSequence>();
        novelSeq.lines = new NovelLine[]
        {
            // ── Блок 1: Дверь ───────────────────────────────────────────────────
            new NovelLine { speaker = "Джон", text = "Я стою у двери потому что...",       cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn1 },
            new NovelLine { speaker = "Джон", text = "...потому что мне здесь хорошо.",    cameraIndex = 0 },

            // ── Блок 2: Балкон ──────────────────────────────────────────────────
            new NovelLine { speaker = "Мэри", text = "Красиво здесь.",   cameraIndex = 1, narratorSequenceBefore = seqBeforeMary1 },

            // ── Блок 3: Гостиная и мэрино «Возможно» слито с вводной про игрока ─
            new NovelLine { speaker = "Джон", text = "Мэри.",                              cameraIndex = 3, narratorSequenceBefore = seqBeforeJohn2 },

            // ── Блок 4: Диалог ──────────────────────────────────────────────────
            new NovelLine { speaker = "Мэри", text = "Что.",                               cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "В гостиной кто-то есть.",            cameraIndex = 0 },
            new NovelLine { speaker = "Мэри", text = "Ты уверен?",                         cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Я... чувствую. Там кто-то есть.",   cameraIndex = 0 },
            new NovelLine { speaker = "Джон", text = "Это было неприятно.",                cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn3 },

            // ── Блок 5: Мастер ──────────────────────────────────────────────────
            new NovelLine { speaker = "Мэри", text = "Может, это мастер по телевизору?",   cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Я не вызывал мастера.",              cameraIndex = 0 },
            new NovelLine { speaker = "Мэри", text = "Я вызывала.",                        cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Когда?",                             cameraIndex = 0 },
            new NovelLine { speaker = "Мэри", text = "Не помню.",                          cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Эй. Ты кто?",                       cameraIndex = 2, narratorSequenceBefore = seqBeforePlayer1 },

            // ── Блок 6: Игрок ───────────────────────────────────────────────────
            new NovelLine { speaker = "Джон", text = "Ты умеешь чинить телевизоры?",      cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn4 },
            // Тихий кадр — игрок молчит
            new NovelLine { speaker = "",     text = "...",                                 cameraIndex = 2 },
            new NovelLine { speaker = "Джон", text = "Молчание — знак согласия.",          cameraIndex = 0 },
            new NovelLine { speaker = "Джон", text = "А как работает?",                    cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn5 },
            new NovelLine { speaker = "Джон", text = "Я стою.",                            cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn6 },
            new NovelLine { speaker = "Джон", text = "Я стою и разговариваю.",             cameraIndex = 0, narratorSequenceBefore = seqBeforeJohn7 },
            new NovelLine { speaker = "Джон", text = "Я многофункциональный.",             cameraIndex = 0 },

            // ── Блок 7: Финал ───────────────────────────────────────────────────
            new NovelLine { speaker = "Мэри", text = "Джон, впусти его нормально.",        cameraIndex = 1 },
            new NovelLine { speaker = "Мэри", text = "Или выпусти. Определись.",           cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Пусть пока побудет.",                cameraIndex = 0 },
            new NovelLine { speaker = "Мэри", text = "Здесь не так много всего.",          cameraIndex = 1 },
            new NovelLine { speaker = "Джон", text = "Но что-то есть.",                    cameraIndex = 0 },
            // Финальный монолог рассказчика, потом пустая строка = конец катсцены
            new NovelLine { speaker = "",     text = "",                                    cameraIndex = 2, narratorSequenceBefore = seqEnding },
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
