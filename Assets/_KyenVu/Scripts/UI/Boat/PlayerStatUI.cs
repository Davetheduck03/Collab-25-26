using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerStatUI : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("Drag your PlayerStats ScriptableObject here to get the icons and max levels.")]
    public PlayerStats playerStatsData;

    [System.Serializable]
    public class StatSlot
    {
        [Tooltip("Which stat should this slot display?")]
        public UpgradeType statType;

        [Tooltip("The GameObject that has an Image and TMP_Text as children")]
        public GameObject slotObject;
    }

    [Header("UI Slots")]
    [Tooltip("Set up your 4 game objects here")]
    public List<StatSlot> statSlots = new List<StatSlot>();

    private void OnEnable()
    {
        // Automatically update the UI every time this panel is turned on
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (playerStatsData == null)
        {
            Debug.LogWarning("[PlayerStatUI] PlayerStats ScriptableObject is missing!");
            return;
        }

        foreach (var slot in statSlots)
        {
            if (slot.slotObject == null) continue;

            // Automatically find the Image and Text components inside this GameObject
            Image iconImage = slot.slotObject.GetComponentInChildren<Image>();
            TMP_Text levelText = slot.slotObject.GetComponentInChildren<TMP_Text>();

            // Get the base data from your ScriptableObject (for the icon and max level)
            PlayerStats.UpgradeEntry statData = playerStatsData.GetUpgrade(slot.statType);

            if (statData != null)
            {
                // 1. Set the Icon
                if (iconImage != null && statData.icon != null)
                {
                    iconImage.sprite = statData.icon;
                }

                // 2. Set the Level Text
                if (levelText != null)
                {
                    int currentLevel = GetCurrentLevel(slot.statType);

                    // You can format this however you like! 
                    // Example: "Lv 3" or "3 / 10"
                    levelText.text = $"Lv. {currentLevel}";
                }
            }
        }
    }

    /// <summary>
    /// Grabs the player's current level for a specific stat.
    /// </summary>
    private int GetCurrentLevel(UpgradeType type)
    {
        // Ask your UpgradeManager for the REAL level!
        if (UpgradeManager.Instance != null)
        {
            return UpgradeManager.Instance.GetCurrentLevel(type);
        }

        return 1; // Fallback if the manager is missing
    }
}