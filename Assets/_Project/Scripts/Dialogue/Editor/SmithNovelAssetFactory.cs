using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Создаёт/перезаписывает все SO-ассеты визуальной новеллы «The Smith Family».
/// Window → Visual Novel → Create Smith Family Assets
///
/// При повторном запуске ассеты перезаписываются (GUID сохраняется).
/// VisualNovelManager в сцене обновляется автоматически.
/// </summary>
public class SmithNovelAssetFactory : EditorWindow
{
    [MenuItem("Window/Visual Novel/Create Smith Family Assets")]
    public static void CreateAssets()
    {
        const string folder = "Assets/_Project/Dialogue/VisualNovel";

        // ── NovelChannel ─────────────────────────────────────────────────────────
        SaveSO<NovelChannel>(folder, "NovelChannel");

        // ── NARRATOR SEQUENCES ────────────────────────────────────────────────────

        var seqIntroJohn = MakeSeq(folder, "Narr_IntroJohn",
            ("Рассказчик", "Это Джон Смит.",                                   1.0f),
            ("Рассказчик", "Он стоит у двери.",                                1.2f),
            ("Рассказчик", "Точнее, думает, что стоит по своей воле.",         1.8f),
            ("Рассказчик", "Но на самом деле — это Разработчик...",            1.8f),
            ("Рассказчик", "Написал что он стоит у двери.",                    1.5f)
        );

        var seqNarrDisagrees = MakeSeq(folder, "Narr_Disagrees",
            ("Рассказчик", "Это неправда.", 1.0f)
        );

        var seqIntroMary = MakeSeq(folder, "Narr_IntroMary",
            ("Рассказчик", "Понятно.",                                               0.8f),
            ("Рассказчик", "Это Мэри Смит. Жена Джона.",                            1.2f),
            ("Рассказчик", "Она на балконе. Смотрит вдаль.",                        1.2f),
            ("Рассказчик", "Разработчик не прописал что там во дворе.",              1.8f),
            ("Рассказчик", "Поэтому вдаль — это просто HDRI карта.",                1.5f),
            ("Рассказчик", "Хотя, по мнению Мэри, красивая HDRI карта.",            1.5f)
        );

        var seqIntroYou = MakeSeq(folder, "Narr_IntroYou",
            ("Рассказчик", "А это вы.",                                             1.0f),
            ("Рассказчик", "Появились в гостиной. Без объяснений.",                1.2f),
            ("Рассказчик", "Я тоже не понимаю как вы сюда попали.",                2.0f),
            ("Рассказчик", "У Разработчика явно была какая-то идея.",              1.5f)
        );

        var seqAfterNotRemember = MakeSeq(folder, "Narr_AfterNotRemember",
            ("Рассказчик", "Она не вызывала.",                                      1.0f),
            ("Рассказчик", "Воспоминаний до этой сцены у неё нет.",                1.8f),
            ("Рассказчик", "Откуда взялись вы, тоже не вполне ясно.",              1.8f)
        );

        var seqAfterPlayerSilence = MakeSeq(folder, "Narr_AfterPlayerSilence",
            ("Рассказчик", "Игрок молчит.",                        0.8f),
            ("Рассказчик", "Джон работает с тем что есть.",        1.5f)
        );

        var seqJohnNarr1 = MakeSeq(folder, "Narr_JohnExchange1",
            ("Рассказчик", "Это не так работает, Джон.", 1.2f)
        );

        var seqJohnNarr2 = MakeSeq(folder, "Narr_JohnExchange2",
            ("Рассказчик", "Ты должен был просто стоять у двери.", 1.8f)
        );

        var seqJohnNarr3 = MakeSeq(folder, "Narr_JohnExchange3",
            ("Рассказчик", "Ты разговариваешь.", 1.0f)
        );

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
        var novelSeq = MakeOrOverwrite<NovelSequence>(folder, "NovelSeq_SmithOpening");
        novelSeq.lines = new NovelLine[]
        {
            new NovelLine { speaker = "Джон",     text = "Я стою у двери потому что...",       cameraIndex = 0, narratorSequenceBefore = seqIntroJohn },
            new NovelLine { speaker = "Джон",     text = "...потому что мне здесь хорошо.",    cameraIndex = 0 },
            new NovelLine { speaker = "Джон",     text = "Слышу скепсис.",                     cameraIndex = 0, narratorSequenceBefore = seqNarrDisagrees },
            new NovelLine { speaker = "Мэри",     text = "Красиво здесь.",                     cameraIndex = 1, narratorSequenceBefore = seqIntroMary },
            new NovelLine { speaker = "Джон",     text = "Мэри.",                              cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Что.",                               cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "В гостиной кто-то есть.",            cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Ты уверен?",                        cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Я... чувствую. Там кто-то есть.",   cameraIndex = 0 },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3, narratorSequenceBefore = seqIntroYou },
            new NovelLine { speaker = "Мэри",     text = "Может, это мастер по телевизору?",   cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Я не вызывал мастера.",              cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Я вызывала.",                        cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Когда?",                             cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Не помню.",                          cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Эй. Ты кто?",                       cameraIndex = 2, narratorSequenceBefore = seqAfterNotRemember },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3 },
            new NovelLine { speaker = "Джон",     text = "Ты умеешь чинить телевизоры?",       cameraIndex = 2, narratorSequenceBefore = seqAfterPlayerSilence },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3 },
            new NovelLine { speaker = "Джон",     text = "Возможно это знак согласия.",          cameraIndex = 2 },
            new NovelLine { speaker = "Джон",     text = "А как работает?",                    cameraIndex = 0, narratorSequenceBefore = seqJohnNarr1 },
            new NovelLine { speaker = "Джон",     text = "Я стою.",                            cameraIndex = 0, narratorSequenceBefore = seqJohnNarr2 },
            new NovelLine { speaker = "Джон",     text = "Я стою и разговариваю.",             cameraIndex = 0, narratorSequenceBefore = seqJohnNarr3 },
            new NovelLine { speaker = "Джон",     text = "Я многофункциональный.",             cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Джон.",                              cameraIndex = 1 },
            new NovelLine { speaker = "Мэри",     text = "Он вообще уйдёт?",                  cameraIndex = 1 },
            new NovelLine { speaker = "Джон",     text = "Не знаю.",                           cameraIndex = 0 },
            new NovelLine { speaker = "Джон",     text = "Спрашивать бесполезно, он молчит.", cameraIndex = 0 },
            new NovelLine { speaker = "Мэри",     text = "Понятно.",                           cameraIndex = 1 },
            new NovelLine { speaker = "Мэри",     text = "Подождём пока сам разберётся.",      cameraIndex = 1 },
            new NovelLine { speaker = "Player 1", text = "...",                                 cameraIndex = 3, narratorSequenceBefore = seqEnding },
        };
        EditorUtility.SetDirty(novelSeq);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Авто-назначение в VisualNovelManager ─────────────────────────────────
        int assigned = AutoAssignInScene(novelSeq);

        string autoMsg = assigned > 0
            ? $"VisualNovelManager в сцене обновлён автоматически."
            : "⚠ VisualNovelManager не найден в открытой сцене — назначь вручную.";

        Debug.Log("[SmithNovelAssetFactory] Done — assets in " + folder);
        EditorUtility.DisplayDialog("Готово!",
            $"Ассеты новеллы обновлены.\n\n{autoMsg}",
            "OK");
    }

    // ── Авто-назначение ───────────────────────────────────────────────────────────

    private static int AutoAssignInScene(NovelSequence novelSeq)
    {
        var mgr = Object.FindFirstObjectByType<VisualNovelManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[SmithNovelAssetFactory] VisualNovelManager не найден в сцене.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("sequence").objectReferenceValue = novelSeq;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);

        Debug.Log("[SmithNovelAssetFactory] VisualNovelManager.sequence обновлён.");
        return 1;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    /// <summary>Находит существующий ассет или создаёт новый. GUID сохраняется.</summary>
    private static T MakeOrOverwrite<T>(string folder, string name) where T : ScriptableObject
    {
        string path = $"{folder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
            return existing; // данные обновятся через SetDirty снаружи
        var fresh = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(fresh, path);
        return fresh;
    }

    /// <summary>Создаёт или перезаписывает ScriptableObject-ассет без DialogueLine-данных.</summary>
    private static T SaveSO<T>(string folder, string name) where T : ScriptableObject
    {
        string path = $"{folder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            // Уже есть — GUID сохраняется, просто возвращаем
            return existing;
        }
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    /// <summary>Создаёт/перезаписывает DialogueSequence-ассет. GUID сохраняется.</summary>
    private static DialogueSequence MakeSeq(string folder, string name,
        params (string speaker, string text, float pauseAfter)[] lines)
    {
        string path = $"{folder}/{name}.asset";

        DialogueSequence seq;
        var existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
        if (existing != null)
        {
            seq = existing;
        }
        else
        {
            seq = ScriptableObject.CreateInstance<DialogueSequence>();
            AssetDatabase.CreateAsset(seq, path);
        }

        seq.lines = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            seq.lines[i] = new DialogueLine
            {
                speaker    = lines[i].speaker,
                text       = lines[i].text,
                pauseAfter = lines[i].pauseAfter,
                duration   = 0f
            };

        EditorUtility.SetDirty(seq);
        return seq;
    }
}
