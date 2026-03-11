using UnityEngine;

/// <summary>
/// Одна реплика рассказчика. Сериализуется инлайн внутри DialogueSequence.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Имя говорящего (Narrator, Husband, Wife...)")]
    public string speaker = "Narrator";

    [TextArea(2, 6)]
    [Tooltip("Текст реплики")]
    public string text;

    [Tooltip("Длительность показа (0 = авто: длина текста / 50 символов в сек)")]
    public float duration = 0f;

    [Tooltip("Пауза перед следующей репликой")]
    public float pauseAfter = 0.3f;

    [Tooltip("Аудиоклип (опционально)")]
    public AudioClip audioClip;

    public float GetDuration()
    {
        if (duration > 0f) return duration;
        return Mathf.Max(1.5f, text.Length / 50f);
    }
}
