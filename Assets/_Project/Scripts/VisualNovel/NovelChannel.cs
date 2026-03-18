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

    [Tooltip("Набор pitch-значений для жены. 1.0 = оригинал.")]
    public float[] wifePitches = { 1.0f, 1.05f, 1.12f, 1.19f, 0.94f };

    [Tooltip("6 голосовых блипов мужа")]
    public AudioClip[] husbandBlips = new AudioClip[6];

    [Tooltip("Набор pitch-значений для мужа. 1.0 = оригинал.")]
    public float[] husbandPitches = { 1.0f, 0.94f, 0.89f, 0.84f, 1.05f };

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

    /// <summary>Возвращает случайный pitch для указанного speaker ("Wife" / "Husband").</summary>
    public float GetRandomPitch(string speaker)
    {
        float[] arr = speaker switch
        {
            "Wife"    => wifePitches,
            "Husband" => husbandPitches,
            _         => null
        };
        if (arr == null || arr.Length == 0) return 1f;
        return arr[UnityEngine.Random.Range(0, arr.Length)];
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
