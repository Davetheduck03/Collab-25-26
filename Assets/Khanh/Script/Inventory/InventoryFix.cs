using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryFix : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            InventoryUI.Instance.inventory.AddItem(InventoryController.Instance.GetItemFromID(1), 1);
            Debug.Log("Added Item ID 1");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            InventoryUI.Instance.inventory.AddItem(InventoryController.Instance.GetItemFromID(13), 1);
            Debug.Log("Added Item ID 13");
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            PrintInventory();
        }
    }

    private void PrintInventory()
    {
        Debug.Log("=== Current Inventory ===");
        foreach (var item in InventoryController.Instance.items)
        {
            Debug.Log($"{item.data.name} x{item.quantity}");
        }
    }

}
