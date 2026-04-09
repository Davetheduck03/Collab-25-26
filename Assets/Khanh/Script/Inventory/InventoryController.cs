using System;
using System.Collections.Generic;
using System.IO; // --- NEW: Required for saving/loading JSON ---
using UnityEngine;
using UnityEngine.InputSystem;

// =========================================================
// --- NEW: SERIALIZABLE SAVE CLASSES ---
// =========================================================
[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> savedItems = new();
}

[Serializable]
public class InventoryItemSaveData
{
    public int itemID;
    public int quantity;
    public int sellPrice;
}

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

    private string savePath; // --- NEW: Path to the save file ---

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

        savePath = Path.Combine(Application.persistentDataPath, "inventoryData.json");

        database.Initialize();

        // --- NEW: Load the inventory immediately after the database is ready! ---
        LoadInventory();
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

    // =========================================================
    // --- NEW: SAVE AND LOAD LOGIC ---
    // =========================================================
    private void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var item in items)
        {
            saveData.savedItems.Add(new InventoryItemSaveData
            {
                itemID = item.data.ID,
                quantity = item.quantity,
                sellPrice = item.sellPrice
            });
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(savePath, json);
        Debug.Log("[InventoryController] Inventory saved.");
    }

    private void LoadInventory()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            items.Clear();

            foreach (var savedItem in saveData.savedItems)
            {
                // Look up the actual ItemData from the database using the saved ID
                ItemData data = database.GetItem(savedItem.itemID);
                if (data != null)
                {
                    items.Add(new InventoryItem(data, savedItem.quantity, savedItem.sellPrice));
                }
                else
                {
                    Debug.LogWarning($"[InventoryController] Could not find item ID {savedItem.itemID} in database during load!");
                }
            }
            Debug.Log("[InventoryController] Inventory loaded.");
        }
    }

    private void OnApplicationQuit()
    {
        SaveInventory(); // Failsafe save when the game closes
    }
    // =========================================================

    public bool AddItem(ItemData itemData, int amount = 1, int price = 0)
    {
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

            SaveInventory(); // --- NEW: Save the change! ---
            RefreshUIEvent?.Invoke();
            return true;
        }

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

            if (addedAny) SaveInventory(); // --- NEW: Save the change! ---
            RefreshUIEvent?.Invoke();
            return addedAny;
        }

        int remaining = amount;
        bool addedAnyStackable = false;

        while (remaining > 0)
        {
            var existing = items.Find(i => i.data == itemData && i.quantity < itemData.maxStack);

            if (existing != null)
            {
                int spaceAvailable = itemData.maxStack - existing.quantity;
                int toAdd = Mathf.Min(spaceAvailable, remaining);
                existing.quantity += toAdd;
                remaining -= toAdd;
                addedAnyStackable = true;
            }
            else
            {
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

        if (addedAnyStackable) SaveInventory(); // --- NEW: Save the change! ---
        RefreshUIEvent?.Invoke();
        return addedAnyStackable;
    }

    public void RemoveItem(InventoryItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            SaveInventory(); // --- NEW: Save the change! ---
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

        existing.quantity--;
        if (existing.quantity <= 0)
            items.Remove(existing);

        SaveInventory(); // --- NEW: Save the change! ---
        RefreshUIEvent?.Invoke();
    }
}