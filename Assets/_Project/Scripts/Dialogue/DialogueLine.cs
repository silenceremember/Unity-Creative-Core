using UnityEngine;

/// <summary>
/// Одна реплика диалога. Сериализуется инлайн внутри DialogueSequence.
/// </summary>
[System.Serializable]
public class DialogueLine
{

    [TextArea(2, 6)]
    [Tooltip("Текст реплики")]
    public string text;

    [Tooltip("Длительность показа (0 = авто: длина текста / 50 символов в сек)")]
    public float duration = 0f;

    [Tooltip("Пауза перед следующей репликой")]
    public float pauseAfter = 0.3f;

    [Tooltip("Аудиоклип (опционально)")]
    public AudioClip audioClip;

    [Tooltip("Имя GameObject для SetActive(true) в начале этой реплики (пусто = не трогать)")]
    public string activateObject = "";

    public float GetDuration()
    {
        if (duration > 0f) return duration;
        return Mathf.Max(1.5f, text.Length / 50f);
    }
}
