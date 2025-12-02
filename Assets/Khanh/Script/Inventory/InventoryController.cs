using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    public ItemDatabase database;

    public List<InventoryItem> items = new();
    private Dictionary<int, InventoryItem> map = new();

    private void Start()
    {
        database.Initialize();

        // Initialize dictionary for runtime inventory
        foreach (var invItem in items)
        {
            map[invItem.data.ID] = invItem;
        }
    }

    public void AddItem(ItemData itemData, int amount = 1)
    {
        if (!itemData.isStackable)
        {
            for (int i = 0; i < amount; i++)
            {
                items.Add(new InventoryItem(itemData, 1));
            }
            return;
        }

        var existing = items.Find(i => i.data == itemData);

        if (existing != null)
        {
            existing.quantity += amount;

            if (existing.quantity > itemData.maxStack)
                existing.quantity = itemData.maxStack;
        }
        else
        {
            items.Add(new InventoryItem(itemData, amount));
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
    }
}
