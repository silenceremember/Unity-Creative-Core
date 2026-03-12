using UnityEngine;

/// <summary>
/// SO-ассет: список реплик визуальной новеллы.
/// Создай через ПКМ → Create → VisualNovel → Novel Sequence
/// </summary>
[CreateAssetMenu(menuName = "VisualNovel/Novel Sequence", fileName = "NovelSeq_New")]
public class NovelSequence : ScriptableObject
{
    [Header("Реплики новеллы")]
    public NovelLine[] lines;
}
