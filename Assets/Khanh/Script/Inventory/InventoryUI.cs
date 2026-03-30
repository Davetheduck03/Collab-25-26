using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Inventory Settings")]
    public int slotCount = 40;

    [Header("References")]
    // We no longer need the inspector reference because we are using the Singleton!
    public Transform itemGrid;
    public GameObject slotPrefab;

    private List<ItemSlotUI> slots = new();
    private ItemType? currentFilter = null;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateSlots();
        RefreshUI();
    }

    private void OnEnable()
    {
        InventoryController.RefreshUIEvent += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        InventoryController.RefreshUIEvent -= RefreshUI;
    }

    void GenerateSlots()
    {
        for (int i = 0; i < slotCount; i++)
        {
            var obj = Instantiate(slotPrefab, itemGrid);
            var slot = obj.GetComponent<ItemSlotUI>();
            slot.SetEmpty();

            slots.Add(slot);
        }
    }

    public void RefreshUI()
    {
        // Failsafe in case the controller hasn't loaded yet
        if (InventoryController.Instance == null) return;

        foreach (var slot in slots)
            slot.SetEmpty();

        // NEW: Pull directly from the persistent Singleton instance!
        List<InventoryItem> filtered = InventoryController.Instance.items;

        if (currentFilter.HasValue)
            filtered = filtered.Where(i => i.data.type == currentFilter.Value).ToList();

        for (int i = 0; i < filtered.Count && i < slots.Count; i++)
        {
            // NEW: Pass the Singleton instance to the slot
            slots[i].SetItem(filtered[i], InventoryController.Instance);
        }
    }

    public void Filter_All() => SetFilter(null);
    public void Filter_Consumable() => SetFilter(ItemType.Consumable);
    public void Filter_Equippable() => SetFilter(ItemType.Equippable);
    public void Filter_Fish() => SetFilter(ItemType.Fish);
    public void Filter_Material() => SetFilter(ItemType.Material);
    public void Filter_Quest() => SetFilter(ItemType.Quest);

    private void SetFilter(ItemType? filter)
    {
        currentFilter = filter;
        RefreshUI();
    }
}