using UnityEngine;

/// <summary>
/// Configuration for final sequence timing and camera easing.
/// </summary>
[CreateAssetMenu(menuName = "Game/Config/Final Config", fileName = "FinalConfig")]
public class FinalConfig : ScriptableObject
{
    [Tooltip("HDRI darkening duration")]
    [SerializeField] private float hdriDarkDuration = 4f;
    public float HdriDarkDuration => hdriDarkDuration;

    [Tooltip("Camera travel duration")]
    [SerializeField] private float cameraTravelDuration = 2.5f;
    public float CameraTravelDuration => cameraTravelDuration;

    [Tooltip("Camera far shot duration")]
    [SerializeField] private float cameraFarDuration = 4f;
    public float CameraFarDuration => cameraFarDuration;

    [Tooltip("Delay after camera before narrator")]
    [SerializeField] private float narratorDelayAfterCamera = 0.5f;
    public float NarratorDelayAfterCamera => narratorDelayAfterCamera;

    [Tooltip("Delay before quit")]
    [SerializeField] private float quitDelay = 1.5f;
    public float QuitDelay => quitDelay;

    [Tooltip("Camera easing curve")]
    [SerializeField] private AnimationCurve cameraCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve CameraCurve => cameraCurve;

    [Tooltip("Exposure property name in skybox material")]
    [SerializeField] private string hdriExposureProperty = "_Exposure";
    public string HdriExposureProperty => hdriExposureProperty;
}
