using UnityEngine;
using TMPro;

/// <summary>
/// Attach to any buy-button in the equipment shop.
/// Drag the item's ScriptableObject into "item" and the price is read from
/// the item's own "cost" field automatically.
///
/// Pricing reference (based on fish selling for 5–30 g each):
///   WoodenRod   100 g   |  RustyHook     0 g  |  Rowboat       0 g
///   IronRod    1500 g   |  IronHook    500 g  |  Motorboat   400 g
///   SilverRod  3000 g   |  GoldHook   2000 g  |  Trawler     800 g
///   MythicRod  6000 g   |  SpiritHook 4000 g  |  SpiritVessel 1200 g
///
/// Usage:
///   1. Add this component to a Button GameObject.
///   2. Drag the desired item ScriptableObject into the "Item" field.
///   3. Wire Button.OnClick → this.Buy().
/// </summary>
public class ItemShopButton : MonoBehaviour
{
    [Header("Item")]
    [Tooltip("The equipment ScriptableObject this button sells.")]
    [SerializeField] private EquippableData item;

    [Header("Optional UI")]
    [Tooltip("Text label on or near the button that shows the item price. Leave empty to skip.")]
    [SerializeField] private TMP_Text priceLabel;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        RefreshPriceLabel();
    }

    // ── Public API (wire to Button.OnClick in the Inspector) ──────────────────

    /// <summary>
    /// Attempt to purchase the item.
    /// - If the player cannot afford it, shows a notification and returns.
    /// - If the inventory is full, shows a notification and returns.
    /// - Otherwise spends gold and adds the item to the inventory.
    /// </summary>
    public void Buy()
    {
        if (item == null)
        {
            Debug.LogWarning($"[ItemShopButton] No item assigned on {gameObject.name}.");
            return;
        }

        int price = GetPrice();

        // ── Affordability check ────────────────────────────────────────────────
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.GetCurrency() < price)
        {
            int have = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetCurrency() : 0;
            NotificationManager.Instance?.ShowNotification(
                $"Not enough gold! Need {price} g, you have {have} g.");
            return;
        }

        // ── Inventory space check ──────────────────────────────────────────────
        if (InventoryController.Instance == null)
        {
            Debug.LogWarning("[ItemShopButton] InventoryController not found.");
            return;
        }

        if (InventoryController.Instance.IsFull)
        {
            NotificationManager.Instance?.ShowNotification("Inventory is full! Sell some items first.");
            return;
        }

        // ── Purchase ───────────────────────────────────────────────────────────
        CurrencyManager.Instance.SpendCurrency(price);
        InventoryController.Instance.AddItem(item, 1);

        // =======================================================
        // --- NEW: Tell the Mission Manager we got an item! ---
        // =======================================================
        if (MissionManager.Instance != null)
        {
            // Replace 'MissionType.Acquire' with whatever enum you use for buying/getting items!
            // Depending on your setup, item.name might need to be item.displayName or your ID string.
            MissionManager.Instance.ProgressMission(MissionType.Buy, item.name, 1);
        }

        NotificationManager.Instance?.ShowNotification($"Bought {item.displayName} for {price} g!");
        RefreshPriceLabel();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns the buy price from the item's cost field.</summary>
    public int GetPrice()
    {
        if (item is RodItemData  rod)  return rod.cost;
        if (item is HookItemData hook) return hook.cost;
        if (item is BoatItemData boat) return boat.cost;
        return 0;
    }

    private void RefreshPriceLabel()
    {
        if (priceLabel == null) return;
        int price = GetPrice();
        priceLabel.text = price > 0 ? $"{price} g" : "Free";
    }
}
