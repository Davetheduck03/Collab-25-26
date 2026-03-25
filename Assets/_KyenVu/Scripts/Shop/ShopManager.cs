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
        Invoke(nameof(LockPlayer), 0.05f); // Tiny delay to let the Dialogue Box finish closing
    }

    public void OpenFishShop()
    {
        ShopCanvas.SetActive(true);
        FishShopPanel.SetActive(true);
        Invoke(nameof(LockPlayer), 0.05f);
    }

    public void OpenUpgradeStall()
    {
        ShopCanvas.SetActive(true);
        UpgradeStallPanel.SetActive(true);
        Invoke(nameof(LockPlayer), 0.05f);
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

    // --- PLAYER STATE CONTROLS ---

    // NEW: Helper method to lock the player in place while shopping
    private void LockPlayer()
    {
        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            // Forces the player back into the frozen Interact State
            player.SwitchState(player.InteractState);
        }
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