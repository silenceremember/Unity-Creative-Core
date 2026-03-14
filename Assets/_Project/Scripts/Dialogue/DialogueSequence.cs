using UnityEngine;

/// <summary>
/// Последовательность реплик. Один asset = одна сцена/момент.
/// Создай через ПКМ → Create → Dialogue → Sequence
/// </summary>
[CreateAssetMenu(menuName = "Dialogue/Sequence", fileName = "Seq_New")]
public class DialogueSequence : ScriptableObject
{
    public const int MAX_CHARS = 54;

    [Header("Реплики")]
    public DialogueLine[] lines;

    [Header("Автопереход")]
    [Tooltip("Следующая последовательность после завершения этой (null = стоп)")]
    public DialogueSequence nextSequence;

    [Header("Настройки")]
    [Tooltip("Приоритет. >= текущего — прерывает, < текущего — не прерывает.")]
    public int priority = 0;

    [Tooltip("Если true — при запуске сохраняет прерванный диалог и восстанавливает его после завершения этой последовательности.")]
    public bool restoreInterrupted = false;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lines == null) return;
        for (int i = 0; i < lines.Length; i++)
        {
            var text = lines[i]?.text;
            if (!string.IsNullOrEmpty(text) && text.Length > MAX_CHARS)
                Debug.LogWarning(
                    $"<color=orange>[DialogueSequence]</color> <b>{name}</b> · Line [{i}] " +
                    $"<b>{text.Length} chars</b> (max {MAX_CHARS}): \"{text.Substring(0, Mathf.Min(30, text.Length))}...\"",
                    this);
        }
    }
#endif
}
