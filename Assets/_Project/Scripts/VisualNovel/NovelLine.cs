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
    public string speaker;

    [TextArea(2, 5)]
    [Tooltip("Line text")]
    public string text;

    [Tooltip("Camera anchor index (0-3). -1 = don't change.")]
    public int cameraIndex = 0;

    [Tooltip("If set — narrator will play this sequence before showing this line (NovelCanvas is hidden).")]
    public DialogueSequence narratorSequenceBefore;
}
