using UnityEngine;

/// <summary>
/// Single dialogue line. Serialized inline inside DialogueSequence.
/// </summary>
[System.Serializable]
public class DialogueLine
{

    [TextArea(2, 6)]
    [Tooltip("Line text")]
    public string text;

    [Tooltip("Display duration (0 = auto: text length / 50 chars per sec)")]
    public float duration = 0f;

    [Tooltip("Pause before next line")]
    public float pauseAfter = 0.3f;

    [Tooltip("GameObject name to SetActive(true) at the start of this line (empty = skip)")]
    public string activateObject = "";

    public float GetDuration()
    {
        if (duration > 0f) return duration;
        return Mathf.Max(1.5f, text.Length / 50f);
    }
}
