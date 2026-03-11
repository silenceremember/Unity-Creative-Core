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
}
