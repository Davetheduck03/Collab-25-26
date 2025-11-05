using System.Collections.Generic;
using UnityEngine;

public class UpgradeUIManager : MonoBehaviour
{
    [Header("References")]
    public GameObject upgradeSlotPrefab;
    public Transform upgradeListParent;

    private Dictionary<UpgradeType, UpgradeUISlot> uiSlots = new Dictionary<UpgradeType, UpgradeUISlot>();

    void Start()
    {
        GenerateUpgradeUI();
        UpgradeManager.OnUpgradeSuccessful += RefreshUI;
    }

    void OnDestroy()
    {
        UpgradeManager.OnUpgradeSuccessful -= RefreshUI;
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

    void RefreshUI(UpgradeType type)
    {
        if (uiSlots.TryGetValue(type, out var slot))
            slot.UpdateDisplay();
    }
}
