using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Fish Shop panel in the Top Down scene.
/// Reads all fish from InventoryController, shows them in a scrollable list,
/// and lets the player sell individual fish or everything at once.
/// </summary>
public class FishShopUI : MonoBehaviour
{
    public static FishShopUI Instance;

    [Header("Row Spawning")]
    [Tooltip("Parent Transform inside the ScrollView Content")]
    public Transform rowContainer;
    [Tooltip("Prefab with a FishShopRowUI component")]
    public GameObject rowPrefab;

    [Header("Header UI")]
    [Tooltip("'X' / Close button — wired to ShopManager.CloseFishShop()")]
    public Button closeButton;
    // --- NEW: Current Money Text ---
    [Tooltip("Label showing the player's current gold balance")]
    public TMP_Text currentMoneyText;

    [Header("Footer UI")]
    [Tooltip("Label showing the combined value of all fish in inventory")]
    public TMP_Text totalValueText;
    [Tooltip("'Sell All Fish' button")]
    public Button sellAllButton;
    [Tooltip("Optional: feedback label shown briefly after a sale")]
    public TMP_Text feedbackText;

    // Active row instances
    private readonly List<FishShopRowUI> activeRows = new();

    // ──────────────────────────────────────────────────────────────
    #region Unity lifecycle

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // Re-build list every time the panel opens
        Refresh();

        // Wire buttons (safe to do every time; listeners are cleared in OnDisable)
        if (sellAllButton != null)
            sellAllButton.onClick.AddListener(SellAllFish);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (sellAllButton != null)
            sellAllButton.onClick.RemoveListener(SellAllFish);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    #region Public API

    /// <summary>Rebuild the fish list from the current inventory.</summary>
    public void Refresh()
    {
        ClearRows();

        // --- NEW: Update the player's wallet display every time the shop refreshes! ---
        UpdateCurrentMoneyLabel();

        if (InventoryController.Instance == null) return;

        foreach (var invItem in InventoryController.Instance.items)
        {
            if (invItem.data is FishItemData)
            {
                var rowObj = Instantiate(rowPrefab, rowContainer);
                var row = rowObj.GetComponent<FishShopRowUI>();
                row.Populate(invItem, this);
                activeRows.Add(row);
            }
        }

        UpdateTotalLabel();

        // Disable Sell All if nothing to sell
        if (sellAllButton != null)
            sellAllButton.interactable = activeRows.Count > 0;
    }

    /// <summary>Sell a single fish slot.</summary>
    public void SellOne(InventoryItem item)
    {
        if (item == null) return;

        int price = item.sellPrice;
        InventoryController.Instance.RemoveItem(item);
        AddGold(price);

        ShowFeedback($"+{price}g");
        Refresh();
    }

    /// <summary>Sell every fish currently in the inventory.</summary>
    public void SellAllFish()
    {
        if (InventoryController.Instance == null) return;

        // Collect fish items first (avoid modifying list while iterating)
        var fishItems = new List<InventoryItem>();
        foreach (var invItem in InventoryController.Instance.items)
        {
            if (invItem.data is FishItemData)
                fishItems.Add(invItem);
        }

        if (fishItems.Count == 0) return;

        int total = 0;
        foreach (var fi in fishItems)
        {
            total += fi.sellPrice;
            InventoryController.Instance.RemoveItem(fi);
        }

        AddGold(total);
        ShowFeedback($"Sold all fish for +{total}g!");
        Refresh();
    }

    /// <summary>Close the shop panel (delegates to ShopManager).</summary>
    public void Close()
    {
        var mgr = Object.FindFirstObjectByType<ShopManager>();
        if (mgr != null)
            mgr.CloseFishShop();
        else
            gameObject.SetActive(false); // fallback
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    #region Private helpers

    // --- NEW: Helper method to sync the text with CurrencyManager ---
    private void UpdateCurrentMoneyLabel()
    {
        if (currentMoneyText == null) return;

        if (CurrencyManager.Instance != null)
        {
            currentMoneyText.text = $"Gold: {CurrencyManager.Instance.GetCurrency()}";
        }
        else
        {
            currentMoneyText.text = "Gold: 0";
        }
    }

    private void ClearRows()
    {
        foreach (var row in activeRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }
        activeRows.Clear();
    }

    private void UpdateTotalLabel()
    {
        if (totalValueText == null) return;

        int total = 0;
        foreach (var row in activeRows)
            total += row.SellPrice;

        totalValueText.text = total > 0 ? $"Total: {total}g" : "No fish to sell";
    }

    private void AddGold(int amount)
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCurrency(amount);
        else
            Debug.LogWarning("[FishShopUI] CurrencyManager.Instance is null — gold not added!");
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), 2f);
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    #endregion
}