using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "FishyWishy/ItemDataBase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    private Dictionary<int, ItemData> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<int, ItemData>();

        foreach (var item in allItems)
        {
            if (!lookup.ContainsKey(item.ID))
                lookup.Add(item.ID, item);
            else
                Debug.LogWarning($"Duplicate item ID found: {item.ID}");
        }
    }

    public ItemData GetItem(int id)
    {
        if (lookup.TryGetValue(id, out var item))
            return item;

        Debug.LogWarning($"Item ID {id} not found!");
        return null;
    }
}
