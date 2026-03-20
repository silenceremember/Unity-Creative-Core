using UnityEngine;

/// <summary>
/// Dialogue sequence. One asset = one scene/moment.
/// </summary>
[CreateAssetMenu(menuName = "Game/Dialogue/Sequence", fileName = "Seq_New")]
public class DialogueSequence : ScriptableObject
{
    public const int MAX_CHARS = 58;

    [Header("Lines")]
    public DialogueLine[] lines;

    [Header("Auto-transition")]
    [Tooltip("Next sequence after this one finishes (null = stop)")]
    public DialogueSequence nextSequence;

    [Header("Settings")]
    [Tooltip("Priority. >= current — interrupts, < current — does not.")]
    public int priority = 0;

    [Tooltip("If true — saves the interrupted dialogue and restores it after this sequence finishes.")]
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
