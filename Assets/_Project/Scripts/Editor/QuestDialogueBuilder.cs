
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
        const string folder = "Assets/_Project/Dialogue/Quest";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Dialogue", "Quest");

        // Click dialogues
        var click0 = CreateSeq(folder, "QuestClick_0", 5, null, L("Интересный выбор.", 2f));
        var click1 = CreateSeq(folder, "QuestClick_1", 5, null, L("Любопытно...", 2f));
        var click2 = CreateSeq(folder, "QuestClick_2", 5, null, L("Хм. Уверены?", 2f));
        var click3 = CreateSeq(folder, "QuestClick_3", 5, null, L("Ну что ж.", 2f));

        // Reject dialogues — escalating hints
        var reject0 = CreateSeq(folder, "QuestReject_0", 5, null,
            L("Похоже, картины надо нажать в правильном порядке.", 3.5f));
        var reject1 = CreateSeq(folder, "QuestReject_1", 5, null,
            L("Похоже, картины нужно нажать в порядке 1234.", 1.5f));
        var reject2 = CreateSeq(folder, "QuestReject_2", 5, null,
            L("Порядок 1234. Просто попробуйте.", 2f));
        var reject3 = CreateSeq(folder, "QuestReject_3", 5, null,
            L("Я начинаю сомневаться в эффективности подсказок.", 1.5f));
        var reject4 = CreateSeq(folder, "QuestReject_4", 5, null,
            L("Ладно.", 1.5f),
            L("Я надеялся, что Ваши умственные способности выше...", 3f),
            L("Ладно. Я выполню задание за Вас.", 3f),
            L("Смотрите внимательно.", 2f));

        // Post-quest chain (transitions managed by XPLevelManager)
        var postQuest = CreateSeq(folder, "QuestDone_PostQuest", 5, null,
            L("Похоже, Вы выполнили квест.", 2.5f), L("И сломали картины...", 2.5f),
            L("Но Вы заработали 1000 опыта.", 2.5f), L("Однако Вам некуда его тратить...", 3f),
            L("Хм, я кое-что нашёл.", 1f), L("У меня есть полоска опыта.", 3f));

        var xpBar = CreateSeq(folder, "QuestDone_XPBar", 5, null,
            L("Теперь начислим Вам опыт...", 3f));

        var levelUp = CreateSeq(folder, "QuestDone_LevelUp", 5, null,
            L("Отлично, Вы повысили уровень!", 1.5f), L("Давайте посмотрим...", 1f),
            L("Что же предложил Разработчик.", 2.5f));

        var abilityChosen = CreateSeq(folder, "QuestDone_AbilityChosen", 5, null,
            L("Ого, какой выбор...", 2f), L("Новые способности.", 3f),
            L("Делайте выбор очень внимательно.", 3f),
            L("Вы не сможете откатить изменения.", 3f));

        var doorEpilogue = CreateSeq(folder, "QuestDone_DoorEpilogue", 5, null,
            L("Смотрите, дверь пропала.", 3.5f), L("Вы можете идти.", 2.5f),
            L("Если спрыгнете вниз, Вы перейдёте дальше.", 2.5f),
            L("Но я Вас торопить не буду.", 3f),
            L("Можете находиться здесь сколько хотите...", 3f),
            L("И смотреть на результаты своих действий.", 3.5f));

        var doorUnlocked = CreateSeq(folder, "QuestDone_DoorUnlocked", 5, doorEpilogue,
            L("Кстати, Вам открылся переход...", 3f), L("На новый уровень.", 2.5f));

        var abilityTried = CreateSeq(folder, "QuestDone_AbilityTried", 5, doorUnlocked,
            L("Отличный выбор!", 2f), L("Хм, но похоже...", 1.5f),
            L("Разработчик оставил это как заглушку.", 3.5f),
            L("Впрочем, слишком широкие возможности...", 2.5f),
            L("...могут привести к неожиданным последствиям.", 3f),
            L("В любом случае, поздравляю с повышением уровня.", 5f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int assigned = AutoAssign(
            click0, click1, click2, click3,
            reject0, reject1, reject2, reject3, reject4,
            postQuest, xpBar, levelUp, abilityChosen, abilityTried, doorUnlocked);

        string autoMsg = assigned > 0
            ? $"Managers updated automatically ({assigned} fields)."
            : "⚠ Managers not found — assign manually.";
        EditorUtility.DisplayDialog("Done!",
            $"Created 15 assets in {folder}.\n\n{autoMsg}", "OK");
    }

    private static int AutoAssign(
        DialogueSequence click0, DialogueSequence click1,
        DialogueSequence click2, DialogueSequence click3,
        DialogueSequence reject0, DialogueSequence reject1,
        DialogueSequence reject2, DialogueSequence reject3,
        DialogueSequence reject4,
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
            clickArr.arraySize = 4;
            clickArr.GetArrayElementAtIndex(0).objectReferenceValue = click0;
            clickArr.GetArrayElementAtIndex(1).objectReferenceValue = click1;
            clickArr.GetArrayElementAtIndex(2).objectReferenceValue = click2;
            clickArr.GetArrayElementAtIndex(3).objectReferenceValue = click3;

            var rejArr = so.FindProperty("rejectDialogues");
            rejArr.arraySize = 5;
            rejArr.GetArrayElementAtIndex(0).objectReferenceValue = reject0;
            rejArr.GetArrayElementAtIndex(1).objectReferenceValue = reject1;
            rejArr.GetArrayElementAtIndex(2).objectReferenceValue = reject2;
            rejArr.GetArrayElementAtIndex(3).objectReferenceValue = reject3;
            rejArr.GetArrayElementAtIndex(4).objectReferenceValue = reject4;

            so.FindProperty("seqPostQuest").objectReferenceValue = postQuest;
            so.ApplyModifiedProperties();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(pqm.gameObject.scene);
            Debug.Log("[QuestDialogueBuilder] PaintingQuestManager updated.");
            count += 8;
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
            Debug.Log("[QuestDialogueBuilder] XPLevelManager updated.");
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
        var seq = ScriptableObject.CreateInstance<DialogueSequence>();
        seq.priority     = priority;
        seq.nextSequence = next;
        seq.lines        = lines;
        var disk = SaveAsset(seq, folder, name);
        EditorUtility.SetDirty(disk);
        return disk;
    }

    private static DialogueSequence SaveAsset(DialogueSequence seq, string folder, string name)
    {
        string path = $"{folder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<DialogueSequence>(path);
        if (existing != null)
        {
            existing.lines        = seq.lines;
            existing.priority     = seq.priority;
            existing.nextSequence = seq.nextSequence;
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
