using UnityEngine;

/// <summary>
/// Dialogue sequence. One asset = one scene/moment.
/// </summary>
[CreateAssetMenu(menuName = "Game/Dialogue/Sequence", fileName = "Seq_New")]
public class DialogueSequence : ScriptableObject
{
    public const int MAX_CHARS = 58;

    [Header("Lines")]
    [SerializeField] private DialogueLine[] lines;

    [Header("Auto-transition")]
    [Tooltip("Next sequence after this one finishes (null = stop)")]
    [SerializeField] private DialogueSequence nextSequence;

    [Header("Settings")]
    [Tooltip("Priority. >= current — interrupts, < current — does not.")]
    [SerializeField] private int priority;

    [Tooltip("If true — saves the interrupted dialogue and restores it after this sequence finishes.")]
    [SerializeField] private bool restoreInterrupted;

    public DialogueLine[] Lines => lines;
    public DialogueSequence NextSequence => nextSequence;
    public int Priority => priority;
    public bool RestoreInterrupted => restoreInterrupted;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (lines == null) return;
        for (int i = 0; i < lines.Length; i++)
        {
            var t = lines[i]?.Text;
            if (!string.IsNullOrEmpty(t) && t.Length > MAX_CHARS)
                Debug.LogWarning(
                    $"<color=orange>[DialogueSequence]</color> <b>{name}</b> · Line [{i}] " +
                    $"<b>{t.Length} chars</b> (max {MAX_CHARS}): \"{t.Substring(0, Mathf.Min(30, t.Length))}...\"",
                    this);
        }
    }
#endif
}
