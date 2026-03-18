using System;
using UnityEngine;

/// <summary>
/// Канал событий рассказчика. ScriptableObject-шина.
/// NarratorManager подписывается; кнопки/триггеры вызывают Raise().
/// Создай через ПКМ → Create → Dialogue → Narrator Channel
/// </summary>
[CreateAssetMenu(menuName = "Dialogue/Narrator Channel", fileName = "NarratorChannel")]
public class NarratorChannel : ScriptableObject
{
    [Header("Голос персонажа")]
    [Tooltip("Один голосовой блип. Pitch меняется случайно из массива Pitches.")]
    public AudioClip voiceBlip;

    [Tooltip("Набор значений pitch. На каждый символ берётся случайный.\n1.0 = оригинал, 1.12 = +2 полутона, 1.19 = +3 полутона, 0.94 = -1, 0.89 = -2.")]
    public float[] pitches = { 1.0f, 1.05f, 1.12f, 1.19f, 0.94f, 0.89f };

    /// <summary>Подпишись чтобы получать последовательности</summary>
    public event Action<DialogueSequence> OnSequenceRequested;

    /// <summary>Вызови чтобы запустить последовательность</summary>
    public void Raise(DialogueSequence sequence)
    {
        if (sequence == null) return;
        OnSequenceRequested?.Invoke(sequence);
    }

    /// <summary>Остановить текущую последовательность</summary>
    public event Action OnStopRequested;
    public void Stop() => OnStopRequested?.Invoke();

    /// <summary>Стреляет когда последовательность полностью завершена</summary>
    public event Action<DialogueSequence> OnSequenceCompleted;
    public void NotifyCompleted(DialogueSequence sequence) =>
        OnSequenceCompleted?.Invoke(sequence);

    /// <summary>Возвращает случайный pitch из массива pitches (1.0 если массив пуст).</summary>
    public float GetRandomPitch()
    {
        if (pitches == null || pitches.Length == 0) return 1f;
        return pitches[UnityEngine.Random.Range(0, pitches.Length)];
    }
}
