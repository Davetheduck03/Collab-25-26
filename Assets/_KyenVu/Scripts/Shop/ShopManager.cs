using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Panels")]
    public GameObject ShopCanvas;
    public GameObject TavernPanel;
    public GameObject FishShopPanel;
    public GameObject UpgradeStallPanel;

    void Start()
    {
        // Ensure all shop panels are hidden when the game starts
        if (ShopCanvas != null) ShopCanvas.SetActive(false);
        if (TavernPanel != null) TavernPanel.SetActive(false);
        if (FishShopPanel != null) FishShopPanel.SetActive(false);
        if (UpgradeStallPanel != null) UpgradeStallPanel.SetActive(false);
    }

    // --- OPEN METHODS (Trigger these from Dialogue Choices) ---

    public void OpenTavern()
    {
        ShopCanvas.SetActive(true);
        TavernPanel.SetActive(true);
    }

    public void OpenFishShop()
    {
        ShopCanvas.SetActive(true);
        FishShopPanel.SetActive(true);
    }

    public void OpenUpgradeStall()
    {
        ShopCanvas.SetActive(true);
        UpgradeStallPanel.SetActive(true);
    }

    // --- CLOSE METHODS (Trigger these from UI 'X' Buttons) ---

    public void CloseTavern()
    {
        ShopCanvas.SetActive(false);
        TavernPanel.SetActive(false);
        FreePlayer();
    }

    public void CloseFishShop()
    {
        ShopCanvas.SetActive(false);
        FishShopPanel.SetActive(false);
        FreePlayer();
    }

    public void CloseUpgradeStall()
    {
        ShopCanvas.SetActive(false);
        UpgradeStallPanel.SetActive(false);
        FreePlayer();
    }

    // Helper method to let the player walk again
    private void FreePlayer()
    {
        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            player.EndInteraction();
        }
    }
}