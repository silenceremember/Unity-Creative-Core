using UnityEngine;

/// <summary>
/// SO asset: list of visual novel lines.
/// </summary>
[CreateAssetMenu(menuName = "Game/VisualNovel/Novel Sequence", fileName = "NovelSeq_New")]
public class NovelSequence : ScriptableObject
{
    [Header("Novel Lines")]
    [SerializeField] private NovelLine[] lines;
    public NovelLine[] Lines => lines;
}
