using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Configuration for visual novel typewriter, camera blending, and character voices.
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

    [Tooltip("Camera blend easing curve")]
    [SerializeField] private AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve BlendCurve => blendCurve;

    [Tooltip("AudioMixerGroup for novel voice")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    public AudioMixerGroup MixerGroup => mixerGroup;

    [Header("Character Voices")]
    [SerializeField] private SpeakerVoice[] voices;

    /// <summary>Returns voice settings for the given speaker (null if not configured).</summary>
    public SpeakerVoice GetVoice(Speaker speaker)
    {
        if (voices == null) return null;
        foreach (var v in voices)
            if (v.Speaker == speaker) return v;
        return null;
    }

    [Serializable]
    public class SpeakerVoice
    {
        [SerializeField] private Speaker speaker;
        [SerializeField] private AudioClip[] blips;
        [SerializeField] private float pitchMin = 0.9f;
        [SerializeField] private float pitchMax = 1.1f;
        [Range(1, 10)]
        [SerializeField] private int blipInterval = 3;

        public Speaker Speaker => speaker;
        public int BlipInterval => blipInterval;

        public AudioClip GetRandomBlip()
        {
            if (blips == null || blips.Length == 0) return null;
            int attempts = 0;
            AudioClip clip = null;
            while (clip == null && attempts < blips.Length * 2)
            {
                clip = blips[UnityEngine.Random.Range(0, blips.Length)];
                attempts++;
            }
            return clip;
        }

        public float GetRandomPitch() => UnityEngine.Random.Range(pitchMin, pitchMax);
    }
}
