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
            L("This is John Smith.", "Это Джон Смит.", 1.0f),
            L("He's standing by the door.", "Он стоит у двери.", 1.2f),
            L("More precisely, he thinks he's standing there by choice.", "Точнее, он думает, что стоит по своей воле.", 1.8f),
            L("But in reality, the Developer...", "Но на самом деле это Разработчик...", 1.8f),
            L("Wrote that he stands by the door.", "Написал, что он стоит у двери.", 1.5f));

        var seqNarrDisagrees = CreateSeq(folder, "Narr_Disagrees", 0, null,
            L("That's not true.", "Это неправда.", 1.5f));

        var seqIntroMary = CreateSeq(folder, "Narr_IntroMary", 0, null,
            L("I see.", "Понятно.", 0.8f),
            L("This is Mary Smith. John's wife.", "Это Мэри Смит. Жена Джона.", 1.2f),
            L("She's on the balcony. Gazing into the distance.", "Она на балконе. Смотрит вдаль.", 1.2f),
            L("The Developer didn't specify what's there.", "Разработчик не прописал, что там.", 1.8f),
            L("Hence 'the distance'. It's just an HDRI map.", "Поэтому вдаль. Это просто HDRI-карта.", 1.5f),
            L("Though, in Mary's opinion, a beautiful HDRI map.", "Хотя, по мнению Мэри, красивая HDRI-карта.", 1.5f));

        var seqIntroYou = CreateSeq(folder, "Narr_IntroYou", 0, null,
            L("And this is you.", "А это Вы.", 1.0f),
            L("Appeared in the living room. No explanation.", "Появились в гостиной. Без объяснений.", 1.2f),
            L("I don't understand how you got here either.", "Я тоже не понимаю, как Вы сюда попали.", 2.0f),
            L("The Developer clearly had some idea.", "У Разработчика явно была какая-то идея.", 1.5f));

        var seqAfterNotRemember = CreateSeq(folder, "Narr_AfterNotRemember", 0, null,
            L("She didn't call anyone.", "Она не вызывала.", 1.0f),
            L("She has no memories before this scene.", "Воспоминаний до этой сцены у неё нет.", 1.8f),
            L("Where you came from isn't entirely clear either.", "Откуда взялись Вы, тоже не вполне ясно.", 1.8f));

        var seqAfterPlayerSilence = CreateSeq(folder, "Narr_AfterPlayerSilence", 0, null,
            L("The player is silent.", "Игрок молчит.", 0.8f),
            L("John works with what he's got.", "Джон работает с тем, что есть.", 1.5f));

        var seqJohnNarr1 = CreateSeq(folder, "Narr_JohnExchange1", 0, null,
            L("That's not how it works, John.", "Это не так работает, Джон.", 1.2f));
        var seqJohnNarr2 = CreateSeq(folder, "Narr_JohnExchange2", 0, null,
            L("You were supposed to just stand by the door.", "Ты должен был просто стоять у двери.", 1.8f));
        var seqJohnNarr3 = CreateSeq(folder, "Narr_JohnExchange3", 0, null,
            L("You're talking.", "Ты разговариваешь.", 1.5f));

        var seqEnding = CreateSeq(folder, "Narr_Ending", 0, null,
            L("It seems...", "Похоже...", 0.8f),
            L("They never figured out who you are.", "Они так и не разобрались, кто Вы.", 1.8f),
            L("Neither did you, most likely.", "Вы, впрочем, скорее всего, тоже.", 1.2f),
            L("In any case.", "В любом случае.", 1.0f),
            L("You just played a visual novel.", "Вы только что поиграли в визуальную новеллу.", 1.8f),
            L("The Developer thinks these are generally boring.", "Разработчик считает, что это, как правило, скучно.", 1.8f),
            L("Sit. Read. Press 'Next'.", "Сидеть. Читать. Нажимать «Далее».", 1.2f),
            L("Not exactly thrilling.", "Не очень-то захватывает.", 1.0f),
            L("So... the next step...", "Поэтому... следующим шагом...", 1.0f),
            L("Literally a step.", "В буквальном смысле шагом.", 1.0f),
            L("He unlocked for you...", "Он разблокировал для Вас...", 1.0f),
            L("Walking.", "Ходьбу.", 1.1f),
            L("WASD. Any key of your choice.", "WASD. Любая кнопка на Ваш выбор.", 1.5f),
            L("Before you lies a room.", "Перед Вами комната.", 1.0f),
            L("Walk around. Look at things.", "Ходите. Смотрите на вещи.", 1.0f));

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
            new object[] { Speaker.John, "I'm standing by the door because...", "Я стою у двери, потому что...", CameraAnchor.JohnClose, seqIntroJohn },
            new object[] { Speaker.John, "...because it feels right here.", "...потому что мне здесь хорошо.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.John, "I sense skepticism.", "Слышу скепсис.", CameraAnchor.JohnClose, seqNarrDisagrees },
            new object[] { Speaker.Mary, "It's beautiful here.", "Красиво здесь.", CameraAnchor.Mary, seqIntroMary },
            new object[] { Speaker.John, "Mary.", "Мэри.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Mary, "What.", "Что.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "There's someone in the living room.", "В гостиной кто-то есть.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Mary, "Are you sure?", "Ты уверен?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "I... I can feel it. Someone's there.", "Я... чувствую. Там кто-то есть.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Player, "...", "...", CameraAnchor.Player, seqIntroYou },
            new object[] { Speaker.Mary, "Maybe it's the TV repairman?", "Может, это мастер по телевизору?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "I didn't call a repairman.", "Я не вызывал мастера.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Mary, "I did.", "Я вызывала.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "When?", "Когда?", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Mary, "I don't remember.", "Не помню.", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "Hey. Who are you?", "Эй. Ты кто?", CameraAnchor.John, seqAfterNotRemember },
            new object[] { Speaker.Player, "...", "...", CameraAnchor.Player, null },
            new object[] { Speaker.John, "Do you know how to fix TVs?", "Ты умеешь чинить телевизоры?", CameraAnchor.John, seqAfterPlayerSilence },
            new object[] { Speaker.Player, "...", "...", CameraAnchor.Player, null },
            new object[] { Speaker.John, "Perhaps that's a sign of agreement.", "Возможно, это знак согласия.", CameraAnchor.John, null },
            new object[] { Speaker.John, "And how does this work?", "И как это работает?", CameraAnchor.JohnClose, seqJohnNarr1 },
            new object[] { Speaker.John, "I'm standing.", "Я стою.", CameraAnchor.JohnClose, seqJohnNarr2 },
            new object[] { Speaker.John, "I'm standing and talking.", "Я стою и разговариваю.", CameraAnchor.JohnClose, seqJohnNarr3 },
            new object[] { Speaker.John, "I'm incredibly multifunctional.", "Я невероятно многофункциональный.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.Mary, "John.", "Джон.", CameraAnchor.Mary, null },
            new object[] { Speaker.Mary, "Is he ever going to leave?", "Он вообще уйдёт?", CameraAnchor.Mary, null },
            new object[] { Speaker.John, "I don't know.", "Не знаю.", CameraAnchor.JohnClose, null },
            new object[] { Speaker.John, "No point asking, he's silent.", "Спрашивать бесполезно, он молчит.", CameraAnchor.John, null },
            new object[] { Speaker.Mary, "I see.", "Понятно.", CameraAnchor.Mary, null },
            new object[] { Speaker.Mary, "Let's wait until he figures it out.", "Подождём, пока сам разберётся.", CameraAnchor.Mary, null },
            new object[] { Speaker.Player, "...", "...", CameraAnchor.Player, seqEnding },
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
