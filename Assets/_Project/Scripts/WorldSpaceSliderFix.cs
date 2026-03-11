using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Полностью обходит EventSystem для World Space Canvas.
/// Во время drag слайдера отключает EventSystem (блокирует hover/click на кнопках).
/// Восстанавливает EventSystem через один кадр после отпускания мыши.
/// </summary>
[RequireComponent(typeof(Slider))]
public class WorldSpaceSliderFix : MonoBehaviour
{
    public Camera uiCamera;

    private Slider _slider;
    private RectTransform _sliderRect;
    private bool _isDragging;
    private int _restoreOnFrame = -1; // кадр, в котором нужно восстановить EventSystem

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
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();

        // --- Начало drag ---
        if (mouse.leftButton.wasPressedThisFrame &&
            RectTransformUtility.RectangleContainsScreenPoint(_sliderRect, screenPos, uiCamera))
        {
            _isDragging = true;
            _restoreOnFrame = -1; // сбрасываем отложенное восстановление
            SetEventSystem(false);
        }

        // --- Конец drag: запоминаем кадр восстановления ---
        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            _isDragging = false;
            _restoreOnFrame = Time.frameCount + 1; // восстановить в следующем кадре
        }

        // --- Отложенное восстановление ---
        if (_restoreOnFrame >= 0 && Time.frameCount >= _restoreOnFrame)
        {
            _restoreOnFrame = -1;
            SetEventSystem(true);
        }

        // --- Страховочный фолбэк: мышь не нажата и нет drag — всегда включаем ---
        if (!_isDragging && _restoreOnFrame < 0 && !mouse.leftButton.isPressed)
        {
            if (EventSystem.current != null && !EventSystem.current.enabled)
                SetEventSystem(true);
        }

        // --- Обновление значения слайдера во время drag ---
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

    private void SetEventSystem(bool state)
    {
        if (EventSystem.current != null)
            EventSystem.current.enabled = state;
    }

    void OnDisable()
    {
        // Если объект выключился во время drag — гарантированно восстанавливаем
        _isDragging = false;
        _restoreOnFrame = -1;
        SetEventSystem(true);
    }
}



