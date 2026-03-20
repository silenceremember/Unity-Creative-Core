using UnityEngine;

/// <summary>
/// Configuration for visual novel typewriter and camera blending.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Novel Config", fileName = "NovelConfig")]
public class NovelConfig : ScriptableObject
{
    [Tooltip("Camera blend duration between anchors")]
    [SerializeField] private float blendDuration = 0.8f;
    public float BlendDuration => blendDuration;

    [Tooltip("Typewriter speed (characters per second)")]
    [SerializeField] private float charsPerSecond = 60f;
    public float CharsPerSecond => charsPerSecond;

    [Tooltip("Play blip sound every N characters")]
    [SerializeField] private int blipEveryNChars = 4;
    public int BlipEveryNChars => blipEveryNChars;
}
