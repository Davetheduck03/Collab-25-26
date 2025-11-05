using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : GameSingleton<UpgradeUIManager>
{
    [Header("References")]
    public GameObject upgradeSlotPrefab;
    public Transform upgradeListParent;
    public UpgradeDetailPanel detailPanel;
    private Dictionary<UpgradeType, UpgradeUISlot> uiSlots = new Dictionary<UpgradeType, UpgradeUISlot>();

    void Start()
    {
        GenerateUpgradeUI();
        UpgradeManager.OnUpgradeSuccessful += RefreshUI;
    }

    protected override void OnDestroy()
    {
        UpgradeManager.OnUpgradeSuccessful -= RefreshUI;
        base.OnDestroy(); // ensures _instance is cleared
    }

    void GenerateUpgradeUI()
    {
        foreach (var entry in UpgradeManager.Instance.playerStatsConfig.upgrades)
        {
            GameObject slotObj = Instantiate(upgradeSlotPrefab, upgradeListParent);
            UpgradeUISlot slot = slotObj.GetComponent<UpgradeUISlot>();

            slot.Setup(entry.type);
            uiSlots.Add(entry.type, slot);
        }
    }
    public void ShowDetail(UpgradeType type)
    {
        detailPanel.Show(type);
    }
    void RefreshUI(UpgradeType type)
    {
        if (uiSlots.TryGetValue(type, out var slot))
            slot.UpdateDisplay();
    }
}
