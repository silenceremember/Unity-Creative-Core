using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Upgrade selection" canvas. All three buttons close the canvas — no real choice.
/// Part of the humor.
/// </summary>
public class LevelUpCanvas : MonoBehaviour
{
    public static LevelUpCanvas Instance { get; private set; }

    /// <summary>Fires when the player presses any button (i.e., "chose" an ability).</summary>
    public static event Action OnAbilityChosen;

    [Tooltip("Root object of this canvas")]
    [SerializeField] private GameObject root;

    [Tooltip("Three upgrade buttons (all close the canvas)")]
    [SerializeField] private Button[] buttons = new Button[3];

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
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = false;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        gameObject.SetActive(false);
        if (PlayerController.Instance != null)
            PlayerController.Instance.enabled = true;

        OnAbilityChosen?.Invoke();
    }
}
