using UnityEngine;

/// <summary>
/// Configuration for camera transition between menu anchors.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Camera Transition Config", fileName = "CameraTransitionConfig")]
public class CameraTransitionConfig : ScriptableObject
{
    [Tooltip("Transition duration in seconds")]
    [Range(0.1f, 3f)]
    [SerializeField] private float duration = 0.8f;
    public float Duration => duration;

    [Tooltip("Transition easing curve")]
    [SerializeField] private AnimationCurve blendCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve BlendCurve => blendCurve;
}
