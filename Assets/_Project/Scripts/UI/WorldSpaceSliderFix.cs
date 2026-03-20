using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Workaround for EventSystem on World Space Canvas.
/// During drag — disables EventSystem, blocking hover/clicks on buttons.
/// </summary>
[RequireComponent(typeof(Slider))]
public class WorldSpaceSliderFix : MonoBehaviour
{
    [SerializeField] private Camera uiCamera;

    private Slider _slider;
    private RectTransform _sliderRect;
    private bool _isDragging;
    private EventSystem _eventSystem;

    void Awake()
    {
        _slider = GetComponent<Slider>();
        _sliderRect = GetComponent<RectTransform>();

        if (uiCamera == null)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) uiCamera = canvas.worldCamera;
        }
        if (uiCamera == null)
            uiCamera = Camera.main;

        _eventSystem = EventSystem.current;
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (!_slider.interactable) return;

        Vector2 screenPos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame &&
            RectTransformUtility.RectangleContainsScreenPoint(_sliderRect, screenPos, uiCamera))
        {
            _isDragging = true;
            SetEventSystemEnabled(false);
        }

        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            _isDragging = false;
            SetEventSystemEnabled(true);
        }

        if (_isDragging && mouse.leftButton.isPressed)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _sliderRect, screenPos, uiCamera, out Vector2 local))
            {
                Rect r = _sliderRect.rect;
                float norm = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
                _slider.value = Mathf.Lerp(_slider.minValue, _slider.maxValue, norm);
            }
        }
    }

    void OnDisable()
    {
        if (_isDragging)
        {
            _isDragging = false;
            SetEventSystemEnabled(true);
        }
    }

    private void SetEventSystemEnabled(bool enabled)
    {
        if (_eventSystem != null)
            _eventSystem.enabled = enabled;
    }
}
