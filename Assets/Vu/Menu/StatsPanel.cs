using UnityEngine;
using UnityEngine.UI;

public class StatsPanel : MonoBehaviour
{
    public Button switchToInventoryButton; // Add a UI button in your stats panel
    private PlayerPanel menuManager; // Reference to manager

    void Awake()
    {
        menuManager = FindAnyObjectByType<PlayerPanel>(); // Or inject via Inspector
        if (switchToInventoryButton)
        {
            switchToInventoryButton.onClick.AddListener(() => menuManager.SwitchPanel(menuManager.inventoryPanel));
        }
    }
}

