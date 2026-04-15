using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One selectable row inside the equipment list on the Pre-Fishing Menu.
/// Shows the item icon, display name, and a short stat summary.
/// The row background turns blue when this item is currently equipped.
/// </summary>
public class PreFishingItemRowUI : MonoBehaviour
{
    [Header("UI References")]
    public Image    itemIcon;
    public TMP_Text nameText;
    public TMP_Text statsText;
    public Button   selectButton;
    /// <summary>Small checkmark / glow shown when this item is currently equipped.</summary>
    public Image    selectedBadge;

    [Header("Row Colors")]
    public Color selectedBgColor   = new Color(0.20f, 0.50f, 0.90f, 0.35f);
    public Color unselectedBgColor = new Color(1.00f, 1.00f, 1.00f, 0.05f);

    // ── Internal ───────────────────────────────────────────────────────────
    private InventoryItem   inventoryItem;
    private PreFishingMenuUI menu;

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Populate this row with item data.</summary>
    public void Populate(InventoryItem item, bool isSelected, PreFishingMenuUI menuRef)
    {
        inventoryItem = item;
        menu          = menuRef;

        // Icon
        if (itemIcon != null)
        {
            itemIcon.enabled = item.data.Sprite != null;
            if (item.data.Sprite != null) itemIcon.sprite = item.data.Sprite;
        }

        // Name
        if (nameText != null) nameText.text = item.data.displayName;

        // Stats by type
        if (statsText != null)
        {
            if (item.data is RodItemData rod)
                statsText.text = $"Line {rod.lineLength}m  Reel ×{rod.reelSpeed:0.0}  ATK ×{rod.attackMult:0.0}";
            else if (item.data is HookItemData hook)
                statsText.text = $"Chance {hook.hookChance:0}%  Rare +{hook.rareBoost:0.0}";
            else if (item.data is BoatItemData boat)
                statsText.text = $"Speed {boat.speed:0.0}  Cap {boat.capacity}  HP +{boat.hp}";
        }

        // Selection state
        ApplySelectionVisuals(isSelected);

        // Wire button
        if (selectButton != null)
            selectButton.onClick.AddListener(OnSelectClicked);
    }

    /// <summary>Show a "nothing here" placeholder row (no button interaction).</summary>
    public void SetEmpty(string message)
    {
        if (nameText    != null) nameText.text    = message;
        if (statsText   != null) statsText.text   = "";
        if (itemIcon    != null) itemIcon.enabled = false;
        if (selectedBadge != null) selectedBadge.enabled = false;
        if (selectButton != null) selectButton.interactable = false;

        var bg = GetComponent<Image>();
        if (bg != null) bg.color = new Color(1f, 1f, 1f, 0.03f);
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void OnSelectClicked()
    {
        menu?.EquipItem(inventoryItem);
    }

    private void ApplySelectionVisuals(bool isSelected)
    {
        if (selectedBadge != null) selectedBadge.enabled = isSelected;

        var bg = GetComponent<Image>();
        if (bg != null) bg.color = isSelected ? selectedBgColor : unselectedBgColor;
    }
}
