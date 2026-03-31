using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

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
        EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        RefreshUI();
    }

    private void OnDisable()
    {
        InventoryController.RefreshUIEvent -= RefreshUI;
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
    }

    private void OnEquipmentChanged()
    {
        // A new boat may have been equipped — rebuild the slot grid to match new capacity
        RegenerateSlots();
    }

    private int GetCapacity()
    {
        if (InventoryController.Instance != null)
            return InventoryController.Instance.MaxCapacity;
        return 8;
    }

    void GenerateSlots()
    {
        int count = GetCapacity();
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(slotPrefab, itemGrid);
            var slot = obj.GetComponent<ItemSlotUI>();
            slot.SetEmpty();
            slots.Add(slot);
        }
    }

    public void RegenerateSlots()
    {
        // Destroy existing slot GameObjects
        foreach (var slot in slots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        slots.Clear();

        GenerateSlots();
        RefreshUI();
    }

    public void RefreshUI()
    {
        // Failsafe in case the controller hasn't loaded yet
        if (InventoryController.Instance == null) return;

        foreach (var slot in slots)
            slot.SetEmpty();

        // Pull directly from the persistent Singleton instance
        List<InventoryItem> filtered = InventoryController.Instance.items;

        if (currentFilter.HasValue)
            filtered = filtered.Where(i => i.data.type == currentFilter.Value).ToList();

        for (int i = 0; i < filtered.Count && i < slots.Count; i++)
        {
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