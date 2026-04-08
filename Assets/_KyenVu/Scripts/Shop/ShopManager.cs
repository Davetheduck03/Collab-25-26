using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Panels")]
    public GameObject ShopCanvas;
    public GameObject TavernPanel;
    public GameObject FishShopPanel;
    public GameObject UpgradeStallPanel;
    public Button[] closeButton;

    private void OnEnable()
    {
        // --- NEW: Listen for the shop closing event ---
        TimeManager.OnShopClosed += ForceCloseAllShops;
    }

    private void OnDisable()
    {
        TimeManager.OnShopClosed -= ForceCloseAllShops;
    }

    void Start()
    {
        if (ShopCanvas != null) ShopCanvas.SetActive(false);
        if (TavernPanel != null) TavernPanel.SetActive(false);
        if (FishShopPanel != null) FishShopPanel.SetActive(false);
        if (UpgradeStallPanel != null) UpgradeStallPanel.SetActive(false);
        //foreach (Button button in closeButton)
        //{
        //    button.onClick.AddListener(CheckAndCloseCanvas);
        //}
        
    }

    // --- OPEN METHODS (Trigger these from Dialogue Choices) ---

    // --- OPEN METHODS (Trigger these from Dialogue Choices) ---

    public void OpenTavern()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsShopOpen())
        {
            // Call your new Notification Manager here!
            NotificationManager.Instance.ShowNotification("The Tavern is closed, please come back at 5:00 AM.");
            FreePlayer();
            return;
        }
        ShopCanvas.SetActive(true);
        TavernPanel.SetActive(true);
        Invoke(nameof(LockPlayer), 0.05f);
    }

    public void OpenFishShop()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsShopOpen())
        {
            NotificationManager.Instance.ShowNotification("The Fish Shop is closed, please come back at 5:00 AM.");
            FreePlayer();
            return;
        }
        else
        {
            ShopCanvas.SetActive(true);
            FishShopPanel.SetActive(true);

            if (FishShopUI.Instance != null)
                FishShopUI.Instance.Refresh();

            Invoke(nameof(LockPlayer), 0.05f);
        }
        
    }

    public void OpenUpgradeStall()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsShopOpen())
        {
            NotificationManager.Instance.ShowNotification("The Upgrade Stall is closed, please come back at 5:00 AM.");
            FreePlayer();
            return;
        }
        ShopCanvas.SetActive(true);
        UpgradeStallPanel.SetActive(true);
        Invoke(nameof(LockPlayer), 0.05f);
    }
    // --- CLOSE METHODS (Trigger these from UI 'X' Buttons) ---

    public void CloseTavern()
    {
        if (TavernPanel.activeSelf) TavernPanel.SetActive(false);
        CheckAndCloseCanvas();
    }

    public void CloseFishShop()
    {
        if (FishShopPanel.activeSelf) FishShopPanel.SetActive(false);
        CheckAndCloseCanvas();
    }

    public void CloseUpgradeStall()
    {
        if (UpgradeStallPanel.activeSelf) UpgradeStallPanel.SetActive(false);
        CheckAndCloseCanvas();
    }

    // --- NEW: KICK OUT LOGIC ---
    private void ForceCloseAllShops()
    {
        // If the canvas is open when 7 PM hits, kick them out!
        if (ShopCanvas != null && ShopCanvas.activeSelf)
        {
            Debug.Log("7 PM hit! Kicking player out of the shop menus.");

            if (TavernPanel != null) TavernPanel.SetActive(false);
            if (FishShopPanel != null) FishShopPanel.SetActive(false);
            if (UpgradeStallPanel != null) UpgradeStallPanel.SetActive(false);

            CheckAndCloseCanvas();
        }
    }

    private void CheckAndCloseCanvas()
    {
        ShopCanvas.SetActive(false);
        FreePlayer();
    }

    // --- PLAYER STATE CONTROLS ---

    private void LockPlayer()
    {
        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            player.SwitchState(player.InteractState);
        }
    }

    private void FreePlayer()
    {
        PlayerStateManager player = Object.FindFirstObjectByType<PlayerStateManager>();
        if (player != null)
        {
            player.EndInteraction();
        }
    }
}