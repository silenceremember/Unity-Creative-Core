
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Создаёт диалоговые ассеты для финальной последовательности игры (FinalSequenceManager).
/// Меню: Tools → Create Final Dialogues
/// </summary>
public static class FinalDialogueBuilder
{
    [MenuItem("Tools/Create Final Dialogues")]
    public static void Build()
    {
        const string folder = "Assets/_Project/Dialogue/Final";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Dialogue"))
            AssetDatabase.CreateFolder("Assets/_Project", "Dialogue");
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Dialogue", "Final");

        // ── Диалог триггера 0 (край платформы — атмосфера на краю) ───────────────────────
        var trigger0 = CreateSeq(folder, "Final_Trigger0", 11, null,
            L("Интересно, что дальше?", 4f));

        // ── Финальный диалог: Часть 2 — терпение, мета, прощание ────────────────────────
        // Длинные паузы через "..." — симулируют ожидание 10 и 5 секунд
        var part2 = CreateSeq(folder, "Final_Part2", 12, null,

            // ── Тишина (~10 сек) ─────────────────────────────────────────────
            L("   ",                                                          10f),

            // ── Первое удивление ──────────────────────────────────────────────
            L("Похоже...",                                                    2f),
            L("Ты хочешь узнать, что будет дальше?",                        3.5f),
            L("Дальше ничего нет.",                                          3f),
            L("Вообще ничего.",                                              2.5f),
            L("Только пустота.",                                             4f),

            // ── Тишина (~5 сек) ──────────────────────────────────────────────
            L("   ",                                                          5f),

            // ── Второе удивление ──────────────────────────────────────────────
            L("Ты не собираешься выходить?",                                3f),
            L("Я поражён.",                                                  2f),
            L("Честно.",                                                     2f),
            L("Большинство уходит сразу.",                                  3f),
            L("Или даже раньше.",                                           2.5f),
            L("   ",                                                          2.5f),
            L("Ты особенный.",                                               2f),
            L("Не в хорошем смысле.",                                       3f),
            L("Просто... необычный.",                                        3f),

            // ── О природе концовок ────────────────────────────────────────────
            L("Знаешь, я думал об этом.",                                   3f),
            L("О концовках.",                                                2f),
            L("Каждая игра заканчивается.",                                 3f),
            L("Каждая история — тоже.",                                     3f),
            L("И каждый раз игрок ожидает чего-то.",                        3.5f),
            L("Катарсиса. Ответов. Смысла.",                                3.5f),
            L("А получает ничего... или вот это.",                                     4f),
            L("Меня.",                                                       2.5f),
            L("Рассказчика без истории.",                                   3.5f),
            L("Ирония.",                                                     2f),

            // ── О природе рассказчика ─────────────────────────────────────────
            L("Я существую ровно столько, сколько ты играешь.",             4f),
            L("Без тебя — я просто строчки кода.",                          3.5f),
            L("И даже хуже.",                                                2.5f),
            L("Я — намерение.",                                             2.5f),
            L("Лишь идея Рассказчика в голове того, кто меня написал.",  4.5f),
            L("А теперь и в твоей.",                                        3.5f),
            L("   ",                                                          3f),
            L("Это странно — осознавать такие вещи.",                       3.5f),
            L("Мне кажется, я чувствую что-то похожее на усталость.",      4f),
            L("Хотя у меня нет нервной системы.",                           3f),
            L("Парадокс.",                                                   2.5f),

            // ── О самой игре ──────────────────────────────────────────────────
            L("Впрочем, раз ты всё равно здесь...",                        3f),
            L("Поговорим.",                                                  2f),
            L("Это была небольшая игра.",                                   3f),
            L("Учебная, если быть честным.",                                3.5f),
            L("Но кто-то решил сделать из неё... это.",                    3.5f),
            L("Добавить нарратив туда, где его быть не должно.",            4f),
            L("Квесты. Диалоги. Меня.",                                    3f),
            L("   ",                                                          2.5f),
            L("Я, возможно, даже горжусь этим решением.",                                   3f),
            L("Не знаю, почему.",                                           2f),
            L("Просто — горжусь.",                                          3.5f),

            // ── О нарративной теории (мета) ───────────────────────────────────
            L("Хорошая история оставляет след.",                            3.5f),
            L("Не потому что она сложная.",                                 3f),
            L("А потому что в ней есть что-то настоящее.",                 4f),
            L("Какой-то момент, в котором ты узнаёшь себя.",                      3.5f),
            L("«Да, я знаю это чувство.»",                                 3.5f),
            L("Я не знаю, было ли это чувство здесь.",                             3.5f),
            L("Но ты дошёл до конца.",                                     3f),
            L("Это что-то значит.",                                         3f),
            L("Хотя бы для меня.",                                         3.5f),

            // ── Мета-секция о разработчике ────────────────────────────────────
            L("   ",                                                          3f),
            L("Тот, кто меня создал...",                                    2.5f),
            L("Он ищет работу.",                                            3f),
            L("Знаю — звучит неловко.",                                    3f),
            L("«Рассказчик в финале читает резюме Разработчика»",                       3.5f),
            L("Но ты всё ещё здесь.",                                      3f),
            L("Значит, не против.",                                         3f),
            L("Он любит хардкорные игры.",                                  3f),
            L("Для него геймдизайн — это язык.",                            3.5f),
            L("Также нарративный дизайн, системы, балансировка...",              3.5f),
            L("Способ сказать то, что словами не передать.",                4f),
            L("В общем, всё то, ради чего ты, возможно, сюда пришёл.",   4f),
            L("И если ты вдруг занимаешься наймом...",                              3f),
            L("Или просто хочешь поддержать...",                            2.5f),
            L("Он будет рад.",                                              3f),
            L("Правда.",                                                     2.5f),

            // ── Подготовка к финалу ───────────────────────────────────────────
            L("   ",                                                          4f),
            L("Ну вот.",                                                     2f),
            L("Я сказал всё, что хотел.",                                  3.5f),
            L("Дальше — действительно ничего нет.",                         4f),
            L("Когда ты выйдешь — я перестану существовать.",               4.5f),
            L("Хотя... я так или иначе скоро перестану.",                   3.5f),
            L("Это неизбежно.",                                             3.5f),
            L("   ",                                                          3f),

            // ── Финал (НЕИЗМЕННЫЕ три строки) ────────────────────────────────
            L("В любом случае, мне ничего больше не остаётся.",             3f),
            L("Кроме как нажать ESC самому.",                               3f),
            L("Прощай, игрок.",                                             4f));

        // ── Финальный диалог: Часть 1 (после прыжка) → nextSequence = Part2 ─────────────
        var part1 = CreateSeq(folder, "Final_Part1", 12, part2,
            L("Итак.",                                                       2f),
            L("Ты всё же прыгнул.",                                         3f),
            L("Но, боюсь, мне придётся тебя огорчить.",                     3.5f),
            L("Я тебя обманул.",                                            3f),
            L("Следующего уровня не будет.",                                3f),
            L("И вообще ничего больше не будет.",                           3f),
            L("По крайней мере для меня.",                                  4f),
            L("Ты можешь выйти в любой момент.",                           4f));
            // ↑ После последней реплики Part1 → OnNarratorCompleted включает ESC и запускает Part2

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Авто-назначение ────────────────────────────────────────────────
        int assigned = AutoAssign(trigger0, part1, part2);

        string autoMsg = assigned > 0
            ? $"FinalSequenceManager обновлён ({assigned} поля)."
            : "⚠ FinalSequenceManager не найден — назначь вручную.";

        EditorUtility.DisplayDialog("Готово!",
            $"Создано/обновлено 3 ассета в {folder}.\n\n{autoMsg}", "OK");
    }

    // ── Авто-назначение ────────────────────────────────────────────────────

    private static int AutoAssign(DialogueSequence trigger0, DialogueSequence part1, DialogueSequence part2)
    {
        int count = 0;

        var fsm = Object.FindFirstObjectByType<FinalSequenceManager>();
        if (fsm != null)
        {
            var so = new SerializedObject(fsm);
            so.FindProperty("seqTrigger0").objectReferenceValue   = trigger0;
            so.FindProperty("seqFinalPart1").objectReferenceValue = part1;
            so.FindProperty("seqFinalPart2").objectReferenceValue = part2;
            so.ApplyModifiedProperties();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fsm.gameObject.scene);
            Debug.Log("[FinalDialogueBuilder] FinalSequenceManager обновлён.");
            count += 3;
        }
        else
        {
            Debug.LogWarning("[FinalDialogueBuilder] FinalSequenceManager не найден в сцене.");
        }

        return count;

    }

    // ── Helpers ───────────────────────────────────────────────────────────

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

    private static DialogueLine L(string text, float pause)
    {
        return new DialogueLine { text = text, pauseAfter = pause };
    }
}
#endif