#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates/overwrites all SO assets for the "The Smith Family" visual novel.
/// Menu: Game → Dialogue → Build Visual Novel
///
/// On re-run, assets are overwritten (GUID preserved).
/// VisualNovelManager in scene is updated automatically.
/// </summary>
public static class NovelDialogueBuilder
{
    [MenuItem("Game/Dialogue/Build Visual Novel")]
    public static void Build()
    {
        const string folder = "Assets/_Project/SO/Dialogue/VisualNovel";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/SO/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project/SO", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/SO/Dialogue", "VisualNovel");

        // Narrator sequences
        var seqIntroJohn = CreateSeq(folder, "Narr_IntroJohn", 0, null,
            L("", "Это Джон Смит.", 1.0f),
            L("", "Он стоит у двери.", 1.2f),
            L("", "Точнее, он думает, что стоит по своей воле.", 1.8f),
            L("", "Но на самом деле это Разработчик...", 1.8f),
            L("", "Написал, что он стоит у двери.", 1.5f));

        var seqNarrDisagrees = CreateSeq(folder, "Narr_Disagrees", 0, null,
            L("", "Это неправда.", 1.5f));

        var seqIntroMary = CreateSeq(folder, "Narr_IntroMary", 0, null,
            L("", "Понятно.", 0.8f),
            L("", "Это Мэри Смит. Жена Джона.", 1.2f),
            L("", "Она на балконе. Смотрит вдаль.", 1.2f),
            L("", "Разработчик не прописал, что там.", 1.8f),
            L("", "Поэтому вдаль. Это просто HDRI-карта.", 1.5f),
            L("", "Хотя, по мнению Мэри, красивая HDRI-карта.", 1.5f),
            L("", "Камера, кстати, плавно переместилась.", 1.2f),
            L("", "Это Cinemachine. Или просто Lerp. Кто знает.", 1.5f));

        var seqIntroYou = CreateSeq(folder, "Narr_IntroYou", 0, null,
            L("", "А это Вы.", 1.0f),
            L("", "Появились в гостиной. Без объяснений.", 1.2f),
            L("", "Я тоже не понимаю, как Вы сюда попали.", 2.0f),
            L("", "У Разработчика явно была какая-то идея.", 1.5f));

        var seqAfterNotRemember = CreateSeq(folder, "Narr_AfterNotRemember", 0, null,
            L("", "Она не вызывала.", 1.0f),
            L("", "Воспоминаний до этой сцены у неё нет.", 1.8f),
            L("", "Откуда взялись Вы, тоже не вполне ясно.", 1.8f));

        var seqAfterPlayerSilence = CreateSeq(folder, "Narr_AfterPlayerSilence", 0, null,
            L("", "Игрок молчит.", 0.8f),
            L("", "Джон работает с тем, что есть.", 1.5f));

        var seqJohnNarr1 = CreateSeq(folder, "Narr_JohnExchange1", 0, null,
            L("", "Это не так работает, Джон.", 1.2f));
        var seqJohnNarr2 = CreateSeq(folder, "Narr_JohnExchange2", 0, null,
            L("", "Ты должен был просто стоять у двери.", 1.8f));
        var seqJohnNarr3 = CreateSeq(folder, "Narr_JohnExchange3", 0, null,
            L("", "Ты разговариваешь.", 1.5f));

        var seqEnding = CreateSeq(folder, "Narr_Ending", 0, null,
            L("", "Похоже...", 0.8f),
            L("", "Они так и не разобрались, кто Вы.", 1.8f),
            L("", "Вы, впрочем, скорее всего, тоже.", 1.2f),
            L("", "В любом случае.", 1.0f),
            L("", "Вы только что поиграли в визуальную новеллу.", 1.8f),
            L("", "Разработчик считает, что это, как правило, скучно.", 1.8f),
            L("", "Сидеть. Читать. Нажимать «Далее».", 1.2f),
            L("", "Не очень-то захватывает.", 1.0f),
            L("", "Поэтому... следующим шагом...", 1.0f),
            L("", "В буквальном смысле шагом.", 1.0f),
            L("", "Он разблокировал для Вас...", 1.0f),
            L("", "Ходьбу.", 1.1f),
            L("", "WASD. Любая кнопка на Ваш выбор.", 1.5f),
            L("", "Перед Вами комната.", 1.0f),
            L("", "Ходите. Смотрите на вещи.", 1.0f));

        // Novel sequence — lineData: [Speaker, textEn, textRu, CameraAnchor, narratorSeq]
        string novelPath = $"{folder}/NovelSeq_SmithOpening.asset";
        var novelSeq = AssetDatabase.LoadAssetAtPath<NovelSequence>(novelPath);
        if (novelSeq == null)
        {
            novelSeq = ScriptableObject.CreateInstance<NovelSequence>();
            AssetDatabase.CreateAsset(novelSeq, novelPath);
        }

        var so = new SerializedObject(novelSeq);
        var linesProp = so.FindProperty("lines");

        object[][] lineData = new object[][]
        {
            new object[] { Speaker.John, "", "Я стою у двери, потому что...", CameraAnchor.John, seqIntroJohn },
            new object[] { Speaker.John, "", "...потому что мне здесь хорошо.", CameraAnchor.John, null },
            new object[] { Speaker.John, "", "Слышу скепсис.", CameraAnchor.John, seqNarrDisagrees },
            new object[] { Speaker.Mary, "", "Красиво здесь.", CameraAnchor.Mary, seqIntroMary },
            new object[] { Speaker.John, "", "Мэри.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Что.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "В гостиной кто-то есть.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Ты уверен?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "Я... чувствую. Там кто-то есть.", CameraAnchor.John, null },
            new object[] { Speaker.Player, "", "...", CameraAnchor.Player, seqIntroYou },
            new object[] { Speaker.Mary, "", "Может, это мастер по телевизору?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "Я не вызывал мастера.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Я вызывала.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "Когда?", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Не помню.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "Эй. Ты кто?", CameraAnchor.JohnClose, seqAfterNotRemember },
            new object[] { Speaker.Player, "", "...", CameraAnchor.Player, null },
            new object[] { Speaker.John, "", "Ты умеешь чинить телевизоры?", CameraAnchor.JohnClose, seqAfterPlayerSilence },
            new object[] { Speaker.Player, "", "...", CameraAnchor.Player, null },
            new object[] { Speaker.John, "", "Возможно, это знак согласия.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.John, "", "И как это работает?", CameraAnchor.John, seqJohnNarr1 },
            new object[] { Speaker.John, "", "Я стою.", CameraAnchor.John, seqJohnNarr2 },
            new object[] { Speaker.John, "", "Я стою и разговариваю.", CameraAnchor.John, seqJohnNarr3 },
            new object[] { Speaker.John, "", "Я невероятно многофункциональный.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Джон.", CameraAnchor.Mary, null },
            new object[] { Speaker.Mary, "", "Он вообще уйдёт?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "", "Не знаю.", CameraAnchor.John, null },
            new object[] { Speaker.John, "", "Спрашивать бесполезно, он молчит.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "", "Понятно.", CameraAnchor.Mary, null },
            new object[] { Speaker.Mary, "", "Подождём, пока сам разберётся.", CameraAnchor.Mary, null },
            new object[] { Speaker.Player, "", "...", CameraAnchor.Player, seqEnding },
        };

        linesProp.arraySize = lineData.Length;
        for (int i = 0; i < lineData.Length; i++)
        {
            var elem = linesProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("speaker").enumValueIndex = (int)(Speaker)lineData[i][0];
            elem.FindPropertyRelative("textEn").stringValue = (string)lineData[i][1];
            elem.FindPropertyRelative("text").stringValue = (string)lineData[i][2];
            elem.FindPropertyRelative("cameraAnchor").enumValueIndex = (int)(CameraAnchor)lineData[i][3];
            elem.FindPropertyRelative("narratorSequenceBefore").objectReferenceValue = (Object)lineData[i][4];
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = AutoAssignInScene(novelSeq);
        string autoMsg = assigned > 0
            ? "Scene references updated."
            : "⚠ Manager not found in scene — assign manually.";
        EditorUtility.DisplayDialog("Build Visual Novel",
            $"Assets updated in {folder}.\n{autoMsg}", "OK");
    }

    private static int AutoAssignInScene(NovelSequence novelSeq)
    {
        var mgr = Object.FindFirstObjectByType<VisualNovelManager>();
        if (mgr == null)
        {
            Debug.LogWarning("[NovelDialogueBuilder] VisualNovelManager not found in scene.");
            return 0;
        }

        var so = new SerializedObject(mgr);
        so.FindProperty("sequence").objectReferenceValue = novelSeq;
        so.ApplyModifiedProperties();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);

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
