using UnityEngine;

/// <summary>
/// Single visual novel line.
/// cameraIndex — index in the camera anchors array (0-3).
/// narratorSequenceBefore — if not null, narrator will play this sequence
/// AFTER hiding NovelCanvas (i.e., between two novel lines).
/// </summary>
[System.Serializable]
public class NovelLine
{
    [Tooltip("Speaker name (displayed in NovelCanvas)")]
    [SerializeField] private string speaker;
    public string Speaker => speaker;

    [TextArea(2, 5)]
    [Tooltip("Line text")]
    [SerializeField] private string text;
    public string Text => text;

    [Tooltip("Camera anchor index (0-3). -1 = don't change.")]
    [SerializeField] private int cameraIndex;
    public int CameraIndex => cameraIndex;

    [Tooltip("If set — narrator will play this sequence before showing this line (NovelCanvas is hidden).")]
    [SerializeField] private DialogueSequence narratorSequenceBefore;
    public DialogueSequence NarratorSequenceBefore => narratorSequenceBefore;
}
