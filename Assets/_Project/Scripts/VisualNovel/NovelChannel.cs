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
    [Tooltip("Голосовые блипы Мэри (Mary)")]
    public AudioClip[] maryBlips = new AudioClip[6];

    [Tooltip("Минимальный pitch для Мэри. 1.0 = оригинал.")]
    public float maryPitchMin = 0.94f;

    [Tooltip("Максимальный pitch для Мэри.")]
    public float maryPitchMax = 1.19f;

    [Tooltip("Блип каждые N непробельных символов для Мэри (Undertale-стиль).")]
    [Range(1, 10)]
    public int maryBlipInterval = 3;

    [Tooltip("Голосовые блипы Джона (John)")]
    public AudioClip[] johnBlips = new AudioClip[6];

    [Tooltip("Минимальный pitch для Джона. 1.0 = оригинал.")]
    public float johnPitchMin = 0.84f;

    [Tooltip("Максимальный pitch для Джона.")]
    public float johnPitchMax = 1.05f;

    [Tooltip("Блип каждые N непробельных символов для Джона (Undertale-стиль).")]
    [Range(1, 10)]
    public int johnBlipInterval = 4;

    /// <summary>
    /// Возвращает случайный блип для указанного speaker.
    /// Распознаёт: "Mary"/"Мэри"/"Wife" и "John"/"Джон"/"Husband" (без учёта регистра).
    /// </summary>
    public AudioClip GetBlip(string speaker)
    {
        AudioClip[] arr = ResolveSpeaker(speaker);
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

    /// <summary>
    /// Возвращает случайный pitch для указанного speaker.
    /// </summary>
    public float GetRandomPitch(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key)) return UnityEngine.Random.Range(maryPitchMin, maryPitchMax);
        if (IsJohn(key)) return UnityEngine.Random.Range(johnPitchMin, johnPitchMax);
        return 1f;
    }

    /// <summary>Возвращает блип-интервал (каждые N символов) для speaker. Fallback = 4.</summary>
    public int GetBlipInterval(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key)) return maryBlipInterval;
        if (IsJohn(key)) return johnBlipInterval;
        return 4;
    }

    // ── Internal helpers ──────────────────────────────────────
    private bool IsMary(string key) =>
        string.Equals(key, "Mary",    StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Мэри",   StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Wife",    StringComparison.OrdinalIgnoreCase);

    private bool IsJohn(string key) =>
        string.Equals(key, "John",    StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Джон",   StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Husband", StringComparison.OrdinalIgnoreCase);

    private AudioClip[] ResolveSpeaker(string speaker)
    {
        var key = speaker?.Trim();
        if (IsMary(key))    return maryBlips;
        if (IsJohn(key))    return johnBlips;
        return null;
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
