using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public int sellPrice;
    public string uniqueId;

    public InventoryItem(ItemData itemData, int amount)
    {
        data = itemData;
        quantity = amount;
        uniqueId = Guid.NewGuid().ToString();
    }

    public InventoryItem(ItemData itemData, int amount, int price)
    {
        data = itemData;
        quantity = amount;
        sellPrice = price;
        uniqueId = Guid.NewGuid().ToString();
    }
}


public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    public ItemDatabase database;
    public static event Action RefreshUIEvent;
    public static event Action OnInventoryFull;
    public List<InventoryItem> items = new();
    private Dictionary<int, InventoryItem> map = new();

    /// <summary>
    /// Max number of item slots. Driven by the equipped boat via EquipmentManager
    /// (Rowboat=8, Motorboat=15, Trawler=20, Spirit Vessel=30). Falls back to 8.
    /// </summary>
    public int MaxCapacity =>
        EquipmentManager.Instance != null ? EquipmentManager.Instance.GetInventoryCapacity() : 8;

    public bool IsFull => items.Count >= MaxCapacity;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize here instead of Start() so the lookup is ready even when
        // the Inventory panel is disabled at scene start (Start() is skipped
        // on inactive GameObjects, but Awake() always runs on scene load).
        database.Initialize();
    }
    public void ForceUIRefresh()
    {
        RefreshUIEvent?.Invoke();
    }
    public ItemData GetItemFromID(int ID)
    {
        ItemData item = database.GetItem(ID);
        return item;
    }

    private void Start()
    {
        foreach (var invItem in items)
        {
            map[invItem.data.ID] = invItem;
        }
    }

    private void OnEnable()
    {
        RefreshUIEvent?.Invoke();
    }


    /// <summary>
    /// Attempts to add an item. Returns true if at least some quantity was added,
    /// false if the inventory was already full and nothing was added.
    /// Fires OnInventoryFull if a slot limit is hit.
    /// </summary>
    public bool AddItem(ItemData itemData, int amount = 1, int price = 0)
    {
        // ── Fish: each fish always occupies its own slot ──────────────────
        if (itemData is FishItemData)
        {
            if (IsFull)
            {
                Debug.LogWarning($"[Inventory] Full ({items.Count}/{MaxCapacity}) — could not add {itemData.displayName}.");
                OnInventoryFull?.Invoke();
                return false;
            }

            items.Add(new InventoryItem(itemData, 1, price));
            Debug.Log($"Added {itemData.displayName} worth {price} gold!");
            RefreshUIEvent?.Invoke();
            return true;
        }

        // ── Non-stackable: each unit needs its own slot ───────────────────
        if (!itemData.isStackable)
        {
            bool addedAny = false;
            for (int i = 0; i < amount; i++)
            {
                if (IsFull)
                {
                    Debug.LogWarning($"[Inventory] Full ({items.Count}/{MaxCapacity}) — could not add all of {itemData.displayName}.");
                    OnInventoryFull?.Invoke();
                    break;
                }
                items.Add(new InventoryItem(itemData, 1));
                addedAny = true;
            }
            RefreshUIEvent?.Invoke();
            return addedAny;
        }

        // ── Stackable: fill existing partial stacks first, then new slots ─
        int remaining = amount;
        bool addedAnyStackable = false;

        while (remaining > 0)
        {
            var existing = items.Find(i => i.data == itemData && i.quantity < itemData.maxStack);

            if (existing != null)
            {
                // Top up an existing partial stack — no new slot needed
                int spaceAvailable = itemData.maxStack - existing.quantity;
                int toAdd = Mathf.Min(spaceAvailable, remaining);
                existing.quantity += toAdd;
                remaining -= toAdd;
                addedAnyStackable = true;
            }
            else
            {
                // Need a new slot
                if (IsFull)
                {
                    Debug.LogWarning($"[Inventory] Full ({items.Count}/{MaxCapacity}) — could not add all of {itemData.displayName}.");
                    OnInventoryFull?.Invoke();
                    break;
                }

                int stackAmount = Mathf.Min(remaining, itemData.maxStack);
                items.Add(new InventoryItem(itemData, stackAmount));
                remaining -= stackAmount;
                addedAnyStackable = true;
            }
        }

        RefreshUIEvent?.Invoke();
        return addedAnyStackable;
    }

    public void RemoveItem(InventoryItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            RefreshUIEvent?.Invoke();
        }
    }

    public void UseItem(ItemData itemData)
    {
        var existing = items.Find(i => i.data == itemData);
        if (existing == null) return;

        switch (itemData.type)
        {
            case ItemType.Equippable:
                //implement sau
                break;
            case ItemType.Consumable:
                //implement sau
                break;
            case ItemType.Fish:
                //implement sau
                break;
            default:
                break;
        }

        //Nho lam failsafe cho item neu fail

        existing.quantity--;
        if (existing.quantity <= 0)
            items.Remove(existing);

        RefreshUIEvent?.Invoke();
    }
}
