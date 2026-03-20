using UnityEngine;

/// <summary>
/// Configuration for intro crawl sequence.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Crawl Config", fileName = "CrawlConfig")]
public class CrawlConfig : ScriptableObject
{
    [Tooltip("Scroll speed (pixels/sec)")]
    [SerializeField] private float scrollSpeed = 60f;
    public float ScrollSpeed => scrollSpeed;

    [Tooltip("Speed multiplier when holding")]
    [SerializeField] private float holdMultiplier = 3f;
    public float HoldMultiplier => holdMultiplier;

    [Tooltip("Start Y position")]
    [SerializeField] private float startY = -800f;
    public float StartY => startY;

    [Tooltip("End Y position")]
    [SerializeField] private float endY = 3000f;
    public float EndY => endY;

    [Tooltip("Skip icon blink speed")]
    [SerializeField] private float blinkSpeed = 1.5f;
    public float BlinkSpeed => blinkSpeed;
}
