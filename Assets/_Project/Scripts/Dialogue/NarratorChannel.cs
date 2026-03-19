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
    [Tooltip("Набор голосовых блипов. На каждый символ берётся случайный клип.")]
    public AudioClip[] voiceBlips;

    [Tooltip("Минимальный pitch (нижняя граница диапазона). 1.0 = оригинал, 0.89 ≈ -2 полутона.")]
    public float pitchMin = 0.89f;

    [Tooltip("Максимальный pitch (верхняя граница диапазона). 1.19 ≈ +3 полутона.")]
    public float pitchMax = 1.19f;

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

    /// <summary>Возвращает случайный клип из массива voiceBlips (null если массив пуст).</summary>
    public AudioClip GetRandomBlip()
    {
        if (voiceBlips == null || voiceBlips.Length == 0) return null;
        AudioClip clip = null;
        int attempts = 0;
        while (clip == null && attempts < voiceBlips.Length * 2)
        {
            clip = voiceBlips[UnityEngine.Random.Range(0, voiceBlips.Length)];
            attempts++;
        }
        return clip;
    }

    /// <summary>Возвращает случайный pitch из диапазона [pitchMin, pitchMax].</summary>
    public float GetRandomPitch()
    {
        return UnityEngine.Random.Range(pitchMin, pitchMax);
    }
}
