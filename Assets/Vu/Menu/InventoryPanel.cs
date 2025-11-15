using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class InventoryPanel : MonoBehaviour
{
   public Button switchToStatsButton; // Add a UI button in your inventory panel
    private PlayerPanel menuManager;

    void Awake()
    {
        menuManager = FindAnyObjectByType<PlayerPanel>();
        if (switchToStatsButton)
        {
            switchToStatsButton.onClick.AddListener(() => menuManager.SwitchPanel(menuManager.statsPanel));
        }
    }
}
