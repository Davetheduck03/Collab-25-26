using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One row in the Fish Shop scroll list.
/// Shows the fish icon, name, and sell price.
/// Buttons call back into FishShopUI.
///
/// Prefab layout (suggested):
///   Row (FishShopRowUI)
///   ├── FishIcon  (Image)
///   ├── FishName  (TMP_Text)
///   ├── PriceLabel (TMP_Text)   e.g. "12g"
///   └── SellButton (Button)     → "Sell"
/// </summary>
public class FishShopRowUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fishIcon;
    public TMP_Text fishNameText;
    public TMP_Text priceText;
    public Button sellButton;

    // Cached for SellAll total calculation
    public int SellPrice { get; private set; }

    private InventoryItem item;
    private FishShopUI shop;

    /// <summary>Called by FishShopUI after instantiation.</summary>
    public void Populate(InventoryItem invItem, FishShopUI fishShopUI)
    {
        item = invItem;
        shop = fishShopUI;
        SellPrice = invItem.sellPrice;

        if (fishNameText != null)
            fishNameText.text = invItem.data.displayName;

        if (priceText != null)
            priceText.text = $"{invItem.sellPrice}g";

        if (fishIcon != null && invItem.data.Sprite != null)
            fishIcon.sprite = invItem.data.Sprite;

        if (sellButton != null)
        {
            var label = sellButton.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = "Sell";
            sellButton.onClick.AddListener(OnSellClicked);
        }
    }

    private void OnSellClicked()
    {
        shop?.SellOne(item);
    }
}
