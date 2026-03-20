using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// "Upgrade selection" canvas. All three buttons close the canvas — no real choice.
/// Part of the humor.
/// </summary>
public class LevelUpCanvas : MonoBehaviour
{
    [Tooltip("Fires when the player presses any button")]
    [SerializeField] private VoidChannel abilityChosenChannel;

    [Tooltip("Player controller to disable/enable on show/hide")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("Root object of this canvas")]
    [SerializeField] private GameObject root;

    [Tooltip("Three upgrade buttons (all close the canvas)")]
    [SerializeField] private Button[] buttons = new Button[3];

    void Awake()
    {
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
        if (playerController != null)
            playerController.enabled = false;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        gameObject.SetActive(false);
        if (playerController != null)
            playerController.enabled = true;

        abilityChosenChannel?.Raise();
    }
}
