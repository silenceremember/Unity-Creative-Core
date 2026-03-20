using UnityEngine;

/// <summary>
/// Single dialogue line. Serialized inline inside DialogueSequence.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 6)]
    [Tooltip("Line text")]
    [SerializeField] private string text;

    [Tooltip("Display duration (0 = auto: text length / 50 chars per sec)")]
    [SerializeField] private float duration;

    [Tooltip("Pause before next line")]
    [SerializeField] private float pauseAfter = 0.3f;

    [Tooltip("GameObject name to SetActive(true) at the start of this line (empty = skip)")]
    [SerializeField] private string activateObject = "";

    public string Text => text;
    public float Duration => duration;
    public float PauseAfter => pauseAfter;
    public string ActivateObject => activateObject;

    public float GetDuration()
    {
        if (duration > 0f) return duration;
        return Mathf.Max(1.5f, text.Length / 50f);
    }
}
