using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public InventoryItem (ItemData itemData, int amount)
    {
        data = itemData;
        quantity = amount;
    }
}


public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance;

    public ItemDatabase database;
    public static event Action RefreshUIEvent;
    public List<InventoryItem> items = new();
    private Dictionary<int, InventoryItem> map = new();

    private void Awake()
    {
        Instance = this;
    }

    public ItemData GetItemFromID(int ID)
    {
        ItemData item = database.GetItem(ID);
        return item;
    }

    private void Start()
    {
        database.Initialize();

        foreach (var invItem in items)
        {
            map[invItem.data.ID] = invItem;
        }
    }

    private void OnEnable()
    {
        RefreshUIEvent?.Invoke();
    }

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (!itemData.isStackable)
        {
            for (int i = 0; i < amount; i++)
            {
                items.Add(new InventoryItem(itemData, 1));
            }
            RefreshUIEvent?.Invoke();
            return;
        }

        int remaining = amount;

        while (remaining > 0)
        {
            var existing = items.Find(i => i.data == itemData && i.quantity < itemData.maxStack);

            if (existing != null)
            {
                int spaceAvailable = itemData.maxStack - existing.quantity;
                int toAdd = Mathf.Min(spaceAvailable, remaining);

                existing.quantity += toAdd;
                remaining -= toAdd;
            }
            else
            {
                int stackAmount = Mathf.Min(remaining, itemData.maxStack);
                items.Add(new InventoryItem(itemData, stackAmount));
                remaining -= stackAmount;
            }
        }

        RefreshUIEvent?.Invoke();
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
