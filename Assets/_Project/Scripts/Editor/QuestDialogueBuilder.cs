
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates all DialogueSequence assets for the painting quest and post-level-up narrative.
/// Menu: Game → Dialogue → Build Quest
/// </summary>
public static class QuestDialogueBuilder
{
    [MenuItem("Game/Dialogue/Build Quest")]
    public static void Build()
    {
        const string folder = "Assets/_Project/SO/Dialogue/Quest";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/SO/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project/SO", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/SO/Dialogue", "Quest");

        // Click dialogues
        var clicks = new[] {
            CreateSeq(folder, "QuestClick_0", 5, null, L("Interesting choice.", "Интересный выбор.", 2f)),
            CreateSeq(folder, "QuestClick_1", 5, null, L("Curious...", "Любопытно...", 2f)),
            CreateSeq(folder, "QuestClick_2", 5, null, L("Hmm. You sure?", "Хм. Уверены?", 2f)),
            CreateSeq(folder, "QuestClick_3", 5, null, L("Well then...", "Ну что ж...", 2f)),
            CreateSeq(folder, "QuestClick_4", 5, null, L("Bold decision.", "Смелое решение.", 2f)),
            CreateSeq(folder, "QuestClick_5", 5, null, L("Fair enough.", "Допустим.", 2f)),
            CreateSeq(folder, "QuestClick_6", 5, null, L("Unexpected.", "Неожиданно.", 2f))
        };

        // Reject dialogues — escalating hints
        var rejects = new[] {
            CreateSeq(folder, "QuestReject_0", 5, null,
                L("Looks like the paintings need to be clicked in the right order.", "Похоже, картины надо нажать в правильном порядке.", 3.5f)),
            CreateSeq(folder, "QuestReject_1", 5, null,
                L("You need to click the paintings in order: 1-2-3-4.", "Похоже, картины нужно нажать в порядке 1-2-3-4.", 3.5f)),
            CreateSeq(folder, "QuestReject_2", 5, null,
                L("Order 1-2-3-4. Just. Click. Them. In. Sequence.", "Порядок 1-2-3-4. Просто. Нажмите. Их. По. Очереди.", 3.5f)),
            CreateSeq(folder, "QuestReject_3", 5, null,
                L("I'm starting to question your cognitive abilities.", "Я начинаю сомневаться в Ваших когнитивных способностях.", 3.5f)),
            CreateSeq(folder, "QuestReject_4", 5, null,
                L("Good grief.", "Господи.", 1.5f),
                L("You're not even trying, are you?", "Вы ведь даже не стараетесь, да?", 2.5f),
                L("Fine. I'll handle this deeply complex task for you.", "Ладно. Я выполню эту сложнейшую задачу за Вас.", 3.5f),
                L("Watch and learn.", "Смотрите и учитесь.", 2.0f))
        };

        // Post-quest chain (transitions managed by XPLevelManager)
        var postQuest = CreateSeq(folder, "QuestDone_PostQuest", 5, null,
            L("And there you have it. Quest completed.", "Ну вот. Вы выполнили квест.", 2.5f),
            L("Though the paintings are now completely broken...", "Правда, картины от этого сломались окончательно...", 3.5f),
            L("But you earned 1000 experience points!", "Зато Вы заработали 1000 очков опыта!", 3.0f),
            L("Which you have absolutely nowhere to spend.", "Которые Вам абсолютно некуда тратить.", 3.0f),
            L("Hold on, I found something in the UI.", "Погодите-ка, я кое-что нашёл в интерфейсе.", 3.5f),
            L("There's an XP bar. How classic.", "Здесь есть полоска опыта. Какая классика.", 3.5f));

        var xpBar = CreateSeq(folder, "QuestDone_XPBar", 5, null,
            L("Now we shall ceremoniously award you the experience...", "Сейчас мы торжественно начислим Вам опыт...", 3.5f));

        var levelUp = CreateSeq(folder, "QuestDone_LevelUp", 5, null,
            L("Wow! You've leveled up!", "Ого! Вы повысили уровень!", 2.0f),
            L("Let's see...", "Давайте посмотрим...", 1.5f),
            L("What amazing skills has the Developer to offer?", "Какие же потрясающие навыки предложит Разработчик?", 3.5f));

        var abilityChosen = CreateSeq(folder, "QuestDone_AbilityChosen", 5, null,
            L("My, what a rich selection...", "Боже, какой богатый выбор...", 2.5f),
            L("New abilities. Just like in a real RPG.", "Новые способности. Прямо как в настоящих RPG.", 3.5f),
            L("Make your choice as thoughtfully as possible.", "Делайте свой выбор максимально вдумчиво.", 3.5f),
            L("You won't be able to undo this.", "Вы не сможете откатить эти изменения.", 3.0f));

        var doorEpilogue = CreateSeq(folder, "QuestDone_DoorEpilogue", 5, null,
            L("Look, the door is gone.", "Смотрите, дверь пропала.", 2.5f),
            L("Now you can go.", "Теперь Вы можете идти.", 2.5f),
            L("If you jump down, you'll move on.", "Если спрыгнете вниз, Вы перейдёте дальше.", 3.5f),
            L("But I won't rush you.", "Но я Вас торопить не буду.", 2.5f),
            L("You can stay here as long as you like...", "Можете находиться здесь, сколько захотите...", 3.0f),
            L("And enjoy the results of your actions.", "И наслаждаться результатами своих действий.", 3.5f));

        var doorUnlocked = CreateSeq(folder, "QuestDone_DoorUnlocked", 5, doorEpilogue,
            L("By the way, a passage has opened up...", "Кстати, Вам открылся переход...", 2.5f),
            L("To a new, uncharted level.", "На новый, неизведанный уровень.", 3.0f));

        var abilityTried = CreateSeq(folder, "QuestDone_AbilityTried", 5, doorUnlocked,
            L("Magnificent choice!", "Великолепный выбор!", 2.0f),
            L("Hmm... It seems...", "Хм... Похоже...", 1.5f),
            L("The Developer left the skills as UI placeholders.", "Разработчик оставил навыки как UI-заглушку.", 3.5f),
            L("They do absolutely nothing.", "Они ничего не делают. Совсем.", 2.5f),
            L("Then again, the illusion of choice...", "Впрочем, иллюзия выбора...", 2.0f),
            L("Is the foundation of modern game design.", "Это база современного гейм-дизайна.", 2.5f),
            L("In any case, congratulations on leveling up.", "В любом случае поздравляю с повышением уровня.", 3.5f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = AutoAssign(
            clicks, rejects,
            postQuest, xpBar, levelUp, abilityChosen, abilityTried, doorUnlocked);

        string autoMsg = assigned > 0
            ? "Scene references updated."
            : "⚠ Manager not found in scene — assign manually.";
        EditorUtility.DisplayDialog("Build Quest",
            $"Assets updated in {folder}.\n{autoMsg}", "OK");
    }

    private static int AutoAssign(
        DialogueSequence[] clicks, DialogueSequence[] rejects,
        DialogueSequence postQuest, DialogueSequence xpBar,
        DialogueSequence levelUp, DialogueSequence abilityChosen,
        DialogueSequence abilityTried, DialogueSequence doorUnlocked)
    {
        int count = 0;

        var pqm = Object.FindFirstObjectByType<PaintingQuestManager>();
        if (pqm != null)
        {
            var so = new SerializedObject(pqm);
            var clickArr = so.FindProperty("paintingClickDialogues");
            clickArr.arraySize = clicks.Length;
            for (int i = 0; i < clicks.Length; i++)
                clickArr.GetArrayElementAtIndex(i).objectReferenceValue = clicks[i];

            var rejArr = so.FindProperty("rejectDialogues");
            rejArr.arraySize = rejects.Length;
            for (int i = 0; i < rejects.Length; i++)
                rejArr.GetArrayElementAtIndex(i).objectReferenceValue = rejects[i];

            so.FindProperty("seqPostQuest").objectReferenceValue = postQuest;
            so.ApplyModifiedProperties();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(pqm.gameObject.scene);

            count += clicks.Length + rejects.Length + 1;
        }
        else
        {
            Debug.LogWarning("[QuestDialogueBuilder] PaintingQuestManager not found in scene.");
        }

        var xpm = Object.FindFirstObjectByType<XPLevelManager>();
        if (xpm != null)
        {
            var so = new SerializedObject(xpm);
            so.FindProperty("seqXPBarActivated").objectReferenceValue = xpBar;
            so.FindProperty("seqLevelUp")       .objectReferenceValue = levelUp;
            so.FindProperty("seqAbilityChosen") .objectReferenceValue = abilityChosen;
            so.FindProperty("seqAbilityTried")  .objectReferenceValue = abilityTried;
            so.FindProperty("seqDoorUnlocked")  .objectReferenceValue = doorUnlocked;
            so.ApplyModifiedProperties();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(xpm.gameObject.scene);

            count += 5;
        }
        else
        {
            Debug.LogWarning("[QuestDialogueBuilder] XPLevelManager not found in scene.");
        }

        return count;
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
