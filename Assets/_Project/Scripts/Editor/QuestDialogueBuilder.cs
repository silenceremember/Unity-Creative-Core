
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
            CreateSeq(folder, "QuestClick_0", 5, null, L("", "Интересный выбор.", 2f)),
            CreateSeq(folder, "QuestClick_1", 5, null, L("", "Любопытно...", 2f)),
            CreateSeq(folder, "QuestClick_2", 5, null, L("", "Хм. Уверены?", 2f)),
            CreateSeq(folder, "QuestClick_3", 5, null, L("", "Ну что ж...", 2f)),
            CreateSeq(folder, "QuestClick_4", 5, null, L("", "Смелое решение.", 2f)),
            CreateSeq(folder, "QuestClick_5", 5, null, L("", "Допустим.", 2f)),
            CreateSeq(folder, "QuestClick_6", 5, null, L("", "Неожиданно.", 2f))
        };

        // Reject dialogues — escalating hints
        var rejects = new[] {
            CreateSeq(folder, "QuestReject_0", 5, null,
                L("", "Похоже, картины надо нажать в правильном порядке.", 3.5f)),
            CreateSeq(folder, "QuestReject_1", 5, null,
                L("", "Похоже, картины нужно нажать в порядке 1-2-3-4.", 3.5f)),
            CreateSeq(folder, "QuestReject_2", 5, null,
                L("", "Порядок 1-2-3-4. Просто. Нажмите. Их. По. Очереди.", 3.5f)),
            CreateSeq(folder, "QuestReject_3", 5, null,
                L("", "Я начинаю сомневаться в Ваших когнитивных способностях.", 3.5f)),
            CreateSeq(folder, "QuestReject_4", 5, null,
                L("", "Господи.", 1.5f),
                L("", "Вы ведь даже не стараетесь, да?", 2.5f),
                L("", "Ладно. Я выполню эту сложнейшую задачу за Вас.", 3.5f),
                L("", "Смотрите и учитесь.", 2.0f))
        };

        // Post-quest chain (transitions managed by XPLevelManager)
        var postQuest = CreateSeq(folder, "QuestDone_PostQuest", 5, null,
            L("", "Ну вот. Вы выполнили квест.", 2.5f),
            L("", "Правда, картины от этого сломались окончательно...", 3.5f),
            L("", "Зато Вы заработали 1000 очков опыта!", 3.0f),
            L("", "Которые Вам абсолютно некуда тратить.", 3.0f),
            L("", "Погодите-ка, я кое-что нашёл в интерфейсе.", 3.5f),
            L("", "Здесь есть полоска опыта. Какая классика.", 3.5f));

        var xpBar = CreateSeq(folder, "QuestDone_XPBar", 5, null,
            L("", "Сейчас мы торжественно начислим Вам опыт...", 3.5f));

        var levelUp = CreateSeq(folder, "QuestDone_LevelUp", 5, null,
            L("", "Ого! Вы повысили уровень!", 2.0f),
            L("", "Давайте посмотрим...", 1.5f),
            L("", "Какие же потрясающие навыки предложит Разработчик?", 3.5f));

        var abilityChosen = CreateSeq(folder, "QuestDone_AbilityChosen", 5, null,
            L("", "Боже, какой богатый выбор...", 2.5f),
            L("", "Новые способности. Прямо как в настоящих RPG.", 3.5f),
            L("", "Делайте свой выбор максимально вдумчиво.", 3.5f),
            L("", "Вы не сможете откатить эти изменения.", 3.0f));

        var doorEpilogue = CreateSeq(folder, "QuestDone_DoorEpilogue", 5, null,
            L("", "Смотрите, дверь пропала.", 2.5f),
            L("", "Теперь Вы можете идти.", 2.5f),
            L("", "Если спрыгнете вниз, Вы перейдёте дальше.", 3.5f),
            L("", "Но я Вас торопить не буду.", 2.5f),
            L("", "Можете находиться здесь, сколько захотите...", 3.0f),
            L("", "И наслаждаться результатами своих действий.", 3.5f));

        var doorUnlocked = CreateSeq(folder, "QuestDone_DoorUnlocked", 5, doorEpilogue,
            L("", "Кстати, Вам открылся переход...", 2.5f),
            L("", "На новый, неизведанный уровень.", 3.0f));

        var abilityTried = CreateSeq(folder, "QuestDone_AbilityTried", 5, doorUnlocked,
            L("", "Великолепный выбор!", 2.0f),
            L("", "Хм... Похоже...", 1.5f),
            L("", "Разработчик оставил навыки как UI-заглушку.", 3.5f),
            L("", "Они ничего не делают. Совсем.", 2.5f),
            L("", "Впрочем, иллюзия выбора — это база современного гейм-дизайна.", 4.0f),
            L("", "В любом случае поздравляю с повышением уровня.", 3.5f));

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
