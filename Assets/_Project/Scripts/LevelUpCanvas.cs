using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Канвас "выбора апгрейда". Все три кнопки закрывают канвас — выбора нет.
/// Часть юмора.
/// </summary>
public class LevelUpCanvas : MonoBehaviour
{
    public static LevelUpCanvas Instance { get; private set; }

    /// <summary>Срабатывает когда игрок нажал любую кнопку (т.е. «выбрал» способность).</summary>
    public static event Action OnAbilityChosen;

    [Tooltip("Корневой объект этого канваса")]
    public GameObject root;

    [Tooltip("Три кнопки апгрейда (все закрывают канвас)")]
    public Button[] buttons = new Button[3];

    void Awake()
    {
        Instance = this;
        if (root != null) root.SetActive(false);

        foreach (var btn in buttons)
            if (btn != null)
                btn.onClick.AddListener(Hide);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (root != null) root.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        // Блокируем движение и поворот игрока
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = false;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        gameObject.SetActive(false);
        // Возвращаем управление игроку
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;

        // Уведомляем XPLevelManager о том, что способность выбрана
        OnAbilityChosen?.Invoke();
    }
}
