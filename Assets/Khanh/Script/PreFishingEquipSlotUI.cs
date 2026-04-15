using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One of the three equipment category tabs (Rod / Hook / Boat) on the left column
/// of the Pre-Fishing Menu.  Shows the currently equipped item's icon and key stats.
/// Clicking the button switches the right column to show items of that category.
/// </summary>
public class PreFishingEquipSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image  slotIcon;
    public TMP_Text nameText;
    public TMP_Text statsText;
    public Button   button;
    /// <summary>Border / background image that turns bright when this tab is active.</summary>
    public Image    highlightBorder;

    [Header("Colors")]
    public Color activeColor   = new Color(0.30f, 0.50f, 0.22f, 0.28f); // subtle green tint overlay
    public Color inactiveColor = new Color(0.00f, 0.00f, 0.00f, 0.00f); // transparent — parchment shows through

    // ── Internal ───────────────────────────────────────────────────────────
    private PreFishingMenuUI.EquipSlotType slotType;
    private PreFishingMenuUI               menu;

    // ── Init ───────────────────────────────────────────────────────────────

    public void Init(PreFishingMenuUI.EquipSlotType type, PreFishingMenuUI menuRef)
    {
        slotType = type;
        menu     = menuRef;
        button?.onClick.AddListener(() => menu.ShowSlotList(slotType));
    }

    // ── Refresh ────────────────────────────────────────────────────────────

    /// <summary>Update displayed name and stats for the currently selected zone.</summary>
    public void RefreshZone(ZoneSO zone)
    {
        if (slotIcon != null) slotIcon.enabled = false;

        if (zone == null)
        {
            if (nameText  != null) nameText.text  = "— No Zone —";
            if (statsText != null) statsText.text = "";
            return;
        }

        if (nameText  != null) nameText.text  = zone.zoneName;
        if (statsText != null) statsText.text =
            $"×{zone.currencyMultiplier:0.0} Gold  ×{zone.expMultiplier:0.0} XP";
    }

    public void Refresh(EquippableData equipped)
    {
        string label = slotType == PreFishingMenuUI.EquipSlotType.Rod  ? "Rod"
                     : slotType == PreFishingMenuUI.EquipSlotType.Hook ? "Hook"
                     : slotType == PreFishingMenuUI.EquipSlotType.Zone ? "Zone"
                     :                                                    "Boat";

        if (equipped == null)
        {
            if (slotIcon != null) { slotIcon.sprite = null; slotIcon.enabled = false; }
            if (nameText != null) nameText.text = $"— No {label} —";
            if (statsText != null) statsText.text = "";
            return;
        }

        if (slotIcon != null)
        {
            slotIcon.enabled = equipped.Sprite != null;
            if (equipped.Sprite != null) slotIcon.sprite = equipped.Sprite;
        }

        if (nameText != null) nameText.text = equipped.displayName;

        // Stat summary line
        string stats = "";
        if (equipped is RodItemData rod)
            stats = $"Line {rod.lineLength}m  Reel ×{rod.reelSpeed:0.0}";
        else if (equipped is HookItemData hook)
            stats = $"Chance {hook.hookChance:0}%  Rare +{hook.rareBoost:0.0}";
        else if (equipped is BoatItemData boat)
            stats = $"Spd {boat.speed}  Cap {boat.capacity}  HP +{boat.hp}";

        if (statsText != null) statsText.text = stats;
    }

    /// <summary>Highlight this tab when it is the active category.</summary>
    public void SetHighlight(bool active)
    {
        if (highlightBorder != null)
            highlightBorder.color = active ? activeColor : inactiveColor;
    }
}
