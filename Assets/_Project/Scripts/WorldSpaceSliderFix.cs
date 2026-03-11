using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Обходит EventSystem для World Space Canvas.
/// Во время drag — отключает EventSystem, блокируя hover/клики на кнопках.
/// </summary>
[RequireComponent(typeof(Slider))]
public class WorldSpaceSliderFix : MonoBehaviour
{
    public Camera uiCamera;

    private Slider _slider;
    private RectTransform _sliderRect;
    private bool _isDragging;
    private EventSystem _eventSystem; // кэшируем — current возвращает null когда disabled

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

        _eventSystem = EventSystem.current; // запоминаем пока он ещё включён
    }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (!_slider.interactable) return; // заблокирован — не обрабатываем

        Vector2 screenPos = mouse.position.ReadValue();

        // Начинаем drag только если нажали НАД слайдером
        if (mouse.leftButton.wasPressedThisFrame &&
            RectTransformUtility.RectangleContainsScreenPoint(_sliderRect, screenPos, uiCamera))
        {
            _isDragging = true;
            SetEventSystemEnabled(false); // блокируем все кнопки
        }

        // Отпустили — восстанавливаем
        if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
        {
            _isDragging = false;
            SetEventSystemEnabled(true);
        }

        // Пока drag активен — обновляем даже за пределами rect
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
        // Страховка: если объект отключили во время drag — восстанавливаем EventSystem
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
