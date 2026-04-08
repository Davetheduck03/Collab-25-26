using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeUISlot : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Image progressBar;
    public Button upgradeButton;
    public ShowDetailBtn showDetailBtn;
    public TMP_Text costText;

    [Header("Divider Setup")]
    public Transform dividerParent;
    public GameObject dividerPrefab;
    private List<GameObject> dividers = new();

    private UpgradeType type;

    // --- NEW: Auto-refresh the UI when an upgrade happens! ---
    private void OnEnable()
    {
        UpgradeManager.OnUpgradeSuccessful += HandleUpgradeEvent;
    }

    private void OnDisable()
    {
        UpgradeManager.OnUpgradeSuccessful -= HandleUpgradeEvent;
    }

    private void HandleUpgradeEvent(UpgradeType upgradedType)
    {
        // Only refresh if THIS specific slot was the one upgraded
        if (upgradedType == type)
        {
            UpdateDisplay();
        }
    }

    public void Setup(UpgradeType upgradeType, Sprite icon = null)
    {
        type = upgradeType;
        var entry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(type);

        if (entry != null && entry.icon != null)
            iconImage.sprite = entry.icon;

        if (showDetailBtn != null)
            showDetailBtn.upgradeType = type;

        UpdateDisplay();

        // Remove old listeners to prevent double-clicking bugs, then add the new one
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    public void UpdateDisplay()
    {
        var manager = UpgradeManager.Instance;
        int currentLevel = manager.GetCurrentLevel(type);
        int maxLevel = manager.GetMaxLevel(type);
        int cost = manager.GetNextCost(type);

        progressBar.fillAmount = (float)currentLevel / maxLevel;

        // Build dividers once
        if (dividers.Count == 0)
            SpawnDividers(maxLevel);

        // Button logic
        if (currentLevel >= maxLevel)
        {
            upgradeButton.interactable = false;
            costText.text = "MAX";
        }
        else
        {
            upgradeButton.interactable = true;
            costText.text = $"{cost}";
        }
    }

    private void SpawnDividers(int maxLevel)
    {
        // Clean old ones
        foreach (Transform child in dividerParent)
        {
            Destroy(child.gameObject);
        }
        dividers.Clear(); // Ensure the list is actually clear

        for (int i = 1; i < maxLevel; i++)
        {
            GameObject divider = Instantiate(dividerPrefab, dividerParent);
            RectTransform dividerRect = divider.GetComponent<RectTransform>();

            // Fractional position along the bar (0 to 1)
            float t = (float)i / maxLevel;

            // Set divider anchored position
            dividerRect.anchorMin = new Vector2(t, 0);
            dividerRect.anchorMax = new Vector2(t, 1);
            dividerRect.anchoredPosition = Vector2.zero;

            // Optional: set divider width
            dividerRect.sizeDelta = new Vector2(10f, 0f);

            // --- NEW: Add the divider to the list so it doesn't respawn forever! ---
            dividers.Add(divider);
        }
    }

    private void OnUpgradeClicked()
    {
        if (UpgradeManager.Instance.TryUpgrade(type, out string message))
        {
            Debug.Log(message);
            // We no longer need to call UpdateDisplay() here, because the 
            // HandleUpgradeEvent will automatically do it for us!
        }
        else
        {
            Debug.LogWarning(message);
        }
    }
}