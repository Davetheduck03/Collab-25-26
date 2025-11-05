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
    public TMP_Text costText;

    [Header("Divider Setup")]
    public Transform dividerParent;
    public GameObject dividerPrefab;
    private List<GameObject> dividers = new();

    private UpgradeType type;

    public void Setup(UpgradeType upgradeType, Sprite icon = null)
    {
        type = upgradeType;
        var entry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(type);

        if (entry != null && entry.icon != null)
            iconImage.sprite = entry.icon;

        UpdateDisplay();
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
            costText.text = $"Cost: {cost}";
        }
    }

    private void SpawnDividers(int maxLevel)
    {
        // Clean old ones
        foreach (Transform child in dividerParent)
            Destroy(child.gameObject);

        RectTransform barRect = progressBar.GetComponent<RectTransform>();
        float barWidth = barRect.rect.width;

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
        }
    }

    private void OnUpgradeClicked()
    {
        if (UpgradeManager.Instance.TryUpgrade(type, out string message))
        {
            Debug.Log(message);
            UpdateDisplay();
        }
        else
        {
            Debug.LogWarning(message);
        }
    }
}
