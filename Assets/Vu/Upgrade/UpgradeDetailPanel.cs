using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text statText;

    private UpgradeType currentType;

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show(UpgradeType type)
    {
        currentType = type;
        var upgradeEntry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(type);
        if (upgradeEntry == null) return;

        int currentLevel = UpgradeManager.Instance.GetCurrentLevel(type);
        int maxLevel = UpgradeManager.Instance.GetMaxLevel(type);
        float currentValue = upgradeEntry.baseValue + (currentLevel - 1) * upgradeEntry.incrementPerLevel;
        float nextValue = currentLevel < maxLevel ? currentValue + upgradeEntry.incrementPerLevel : currentValue;

        // Fill UI
        titleText.text = type.ToString();
        descriptionText.text = $"Increase {type} by {upgradeEntry.incrementPerLevel} per level.";
        statText.text = $"<color=white>{currentValue:F1}</color> → <color=yellow>{nextValue:F1}</color>";

        iconImage.sprite = upgradeEntry.icon;

        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
