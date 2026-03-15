using System;
using UnityEngine;

/// <summary>
/// SO-шина для визуальной новеллы.
/// Создай через ПКМ → Create → VisualNovel → Novel Channel
/// </summary>
[CreateAssetMenu(menuName = "VisualNovel/Novel Channel", fileName = "NovelChannel")]
public class NovelChannel : ScriptableObject
{
    /// <summary>Вызови чтобы запустить новеллу</summary>
    public event Action OnNovelStartRequested;
    public void RaiseStart() => OnNovelStartRequested?.Invoke();

    /// <summary>Стреляет когда новелла полностью завершена</summary>
    public event Action OnNovelCompleted;
    public void NotifyCompleted() => OnNovelCompleted?.Invoke();

    /// <summary>Вызови чтобы принудительно прервать новеллу (например из меню паузы)</summary>
    public event Action OnNovelAbortRequested;
    public void RaiseAbort() => OnNovelAbortRequested?.Invoke();
}
