using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Creates/overwrites all SO assets for the "The Smith Family" visual novel.
/// Menu: Game → Dialogue → Build Visual Novel
///
/// On re-run, assets are overwritten (GUID preserved).
/// VisualNovelManager in scene is updated automatically.
/// </summary>
public class SmithNovelAssetFactory : EditorWindow
{
    [MenuItem("Game/Dialogue/Build Visual Novel")]
    public static void CreateAssets()
    {
        const string folder = "Assets/_Project/Dialogue/VisualNovel";

        SaveSO<NovelChannel>(folder, "NovelChannel");

        // Narrator sequences
        var seqIntroJohn = MakeSeq(folder, "Narr_IntroJohn",
            ("Это Джон Смит.",                                   1.0f),
            ("Он стоит у двери.",                                1.2f),
            ("Точнее, он думает, что стоит по своей воле.",      1.8f),
            ("Но на самом деле это Разработчик...",              1.8f),
            ("Написал, что он стоит у двери.",                   1.5f));

        var seqNarrDisagrees = MakeSeq(folder, "Narr_Disagrees",
            ("Это неправда.", 1.5f));

        var seqIntroMary = MakeSeq(folder, "Narr_IntroMary",
            ("Понятно.",                                               0.8f),
            ("Это Мэри Смит. Жена Джона.",                            1.2f),
            ("Она на балконе. Смотрит вдаль.",                        1.2f),
            ("Разработчик не прописал, что там.",                     1.8f),
            ("Поэтому вдаль. Это просто HDRI-карта.",                1.5f),
            ("Хотя, по мнению Мэри, красивая HDRI-карта.",           1.5f),
            ("Камера, кстати, плавно переместилась.",                 1.2f),
            ("Это Cinemachine. Или просто Lerp. Кто знает.",         1.5f));

        var seqIntroYou = MakeSeq(folder, "Narr_IntroYou",
            ("А это Вы.",                                              1.0f),
            ("Появились в гостиной. Без объяснений.",                 1.2f),
            ("Я тоже не понимаю, как Вы сюда попали.",               2.0f),
            ("У Разработчика явно была какая-то идея.",               1.5f));

        var seqAfterNotRemember = MakeSeq(folder, "Narr_AfterNotRemember",
            ("Она не вызывала.",                                      1.0f),
            ("Воспоминаний до этой сцены у неё нет.",                1.8f),
            ("Откуда взялись Вы, тоже не вполне ясно.",              1.8f));

        var seqAfterPlayerSilence = MakeSeq(folder, "Narr_AfterPlayerSilence",
            ("Игрок молчит.",                        0.8f),
            ("Джон работает с тем, что есть.",       1.5f));

        var seqJohnNarr1 = MakeSeq(folder, "Narr_JohnExchange1",
            ("Это не так работает, Джон.", 1.2f));
        var seqJohnNarr2 = MakeSeq(folder, "Narr_JohnExchange2",
            ("Ты должен был просто стоять у двери.", 1.8f));
        var seqJohnNarr3 = MakeSeq(folder, "Narr_JohnExchange3",
            ("Ты разговариваешь.", 1.5f));

        var seqEnding = MakeSeq(folder, "Narr_Ending",
            ("Похоже...",                                                   0.8f),
            ("Они так и не разобрались, кто Вы.",                          1.8f),
            ("Вы, впрочем, скорее всего, тоже.",                           1.2f),
            ("В любом случае.",                                             1.0f),
            ("Вы только что поиграли в визуальную новеллу.",               1.8f),
            ("Разработчик считает, что это, как правило, скучно.",         1.8f),
            ("Сидеть. Читать. Нажимать «Далее».",                          1.2f),
            ("Не очень-то захватывает.",                                   1.0f),
            ("Поэтому, следующим шагом...",                                1.0f),
            ("В буквальном смысле шагом.",                                 1.0f),
            ("Он разблокировал для Вас...",                                1.0f),
            ("Ходьбу.",                                                     1.1f),
            ("WASD. Любая кнопка на Ваш выбор.",                          1.5f),
            ("Перед Вами комната.",                                         1.0f),
            ("Ходите. Смотрите на вещи.",                                  1.0f));

        // Novel sequence
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

        int assigned = AutoAssignInScene(novelSeq);
        string autoMsg = assigned > 0
            ? "VisualNovelManager in scene updated automatically."
            : "⚠ VisualNovelManager not found in open scene — assign manually.";

        Debug.Log("[SmithNovelAssetFactory] Done — assets in " + folder);
        EditorUtility.DisplayDialog("Done!",
            $"Novel assets updated.\n\n{autoMsg}", "OK");
    }

    private static int AutoAssignInScene(NovelSequence novelSeq)
    {
        var mgr = Object.FindFirstObjectByType<VisualNovelManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[SmithNovelAssetFactory] VisualNovelManager not found in scene.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("sequence").objectReferenceValue = novelSeq;
        so.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);
        Debug.Log("[SmithNovelAssetFactory] VisualNovelManager.sequence updated.");
        return 1;
    }

    /// <summary>Finds existing asset or creates new one. GUID preserved.</summary>
    private static T MakeOrOverwrite<T>(string folder, string name) where T : ScriptableObject
    {
        string path = $"{folder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var fresh = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(fresh, path);
        return fresh;
    }

    /// <summary>Creates or overwrites a ScriptableObject asset without DialogueLine data.</summary>
    private static T SaveSO<T>(string folder, string name) where T : ScriptableObject
    {
        string path = $"{folder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    /// <summary>Creates/overwrites a DialogueSequence asset. GUID preserved.</summary>
    private static DialogueSequence MakeSeq(string folder, string name,
        params (string text, float pauseAfter)[] lines)
    {
        string path = $"{folder}/{name}.asset";
        DialogueSequence seq;
        var existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
        if (existing != null)
            seq = existing;
        else
        {
            seq = ScriptableObject.CreateInstance<DialogueSequence>();
            AssetDatabase.CreateAsset(seq, path);
        }

        seq.lines = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            seq.lines[i] = new DialogueLine
            {
                text       = lines[i].text,
                pauseAfter = lines[i].pauseAfter,
                duration   = 0f
            };

        EditorUtility.SetDirty(seq);
        return seq;
    }
}
