using System;

/// <summary>
/// Shared data struct for Editor dialogue builders.
/// Maps to DialogueLine serialized fields via JsonUtility.
/// </summary>
[Serializable]
public struct DialogueLineData
{
    public string text;
    public string textEn;
    public float pauseAfter;
    public string activateObject;
}
