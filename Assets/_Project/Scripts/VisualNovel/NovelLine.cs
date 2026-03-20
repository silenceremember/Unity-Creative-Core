using UnityEngine;

/// <summary>
/// Single visual novel line.
/// cameraAnchor — enum value matching CameraAnchorRegistrar in the scene.
/// narratorSequenceBefore — if not null, narrator will play this sequence
/// AFTER hiding NovelCanvas (i.e., between two novel lines).
/// </summary>
[System.Serializable]
public class NovelLine
{
    [Tooltip("Speaker (dropdown)")]
    [SerializeField] private Speaker speaker;
    public Speaker Speaker => speaker;

    [TextArea(2, 5)]
    [Tooltip("Line text")]
    [SerializeField] private string text;
    public string Text => text;

    [Tooltip("Camera anchor (dropdown). None = don't change camera.")]
    [SerializeField] private CameraAnchor cameraAnchor;
    public CameraAnchor CameraAnchor => cameraAnchor;

    [Tooltip("If set — narrator will play this sequence before showing this line (NovelCanvas is hidden).")]
    [SerializeField] private DialogueSequence narratorSequenceBefore;
    public DialogueSequence NarratorSequenceBefore => narratorSequenceBefore;

    /// <summary>Display name for UI. Returns enum name; localization can be added later.</summary>
    public string SpeakerDisplayName => speaker.ToString();
}
