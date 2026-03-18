using System;
using UnityEngine;

/// <summary>
/// SO-шина для визуальной новеллы.
/// Создай через ПКМ → Create → VisualNovel → Novel Channel
/// </summary>
[CreateAssetMenu(menuName = "VisualNovel/Novel Channel", fileName = "NovelChannel")]
public class NovelChannel : ScriptableObject
{
    [Header("Голоса персонажей")]
    [Tooltip("6 голосовых блипов жены")]
    public AudioClip[] wifeBlips = new AudioClip[6];

    [Tooltip("6 голосовых блипов мужа")]
    public AudioClip[] husbandBlips = new AudioClip[6];

    /// <summary>Возвращает случайный блип для указанного speaker ("Wife" / "Husband").</summary>
    public AudioClip GetBlip(string speaker)
    {
        AudioClip[] arr = speaker switch
        {
            "Wife"    => wifeBlips,
            "Husband" => husbandBlips,
            _         => null
        };
        if (arr == null || arr.Length == 0) return null;
        int attempts = 0;
        AudioClip clip = null;
        while (clip == null && attempts < arr.Length * 2)
        {
            clip = arr[UnityEngine.Random.Range(0, arr.Length)];
            attempts++;
        }
        return clip;
    }

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
