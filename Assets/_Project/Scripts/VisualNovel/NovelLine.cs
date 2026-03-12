using UnityEngine;

/// <summary>
/// Одна строка визуальной новеллы.
/// cameraIndex — индекс в массиве якорей камеры (0-3).
/// narratorSequence — если не null, рассказчик озвучит эту последовательность
/// ПОСЛЕ скрытия NovelCanvas (то есть между двумя репликами новеллы).
/// </summary>
[System.Serializable]
public class NovelLine
{
    [Tooltip("Имя говорящего (отображается в NovelCanvas)")]
    public string speaker;

    [TextArea(2, 5)]
    [Tooltip("Текст реплики")]
    public string text;

    [Tooltip("Индекс якоря камеры (0-3). -1 = не менять.")]
    public int cameraIndex = 0;

    [Tooltip("Если задано — перед показом этой реплики рассказчик произнесёт эту последовательность (NovelCanvas будет скрыт).")]
    public DialogueSequence narratorSequenceBefore;
}
