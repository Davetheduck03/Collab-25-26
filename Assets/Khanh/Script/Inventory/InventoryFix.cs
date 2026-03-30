using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryFix : MonoBehaviour
{
    public EnemySO so1;
    public EnemySO so2;
    public EnemySO so3;
    public EnemySO so4;
    public EnemySO so5;
    public EnemySO so6;

    void Update()
    {
        // Ensure the controller exists before trying to add items!
        if (InventoryController.Instance == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            // CHANGED: Talk directly to InventoryController.Instance
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(1), 1);
            Debug.Log("Added Item ID 1");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(9), 1, so1.GeneratePrice());
            Debug.Log("Added Item ID 9");
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(10), 1, so2.GeneratePrice());
            Debug.Log("Added Item ID 10");
        }
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(11), 1, so3.GeneratePrice());
            Debug.Log("Added Item ID 11");
        }
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(12), 1, so4.GeneratePrice());
            Debug.Log("Added Item ID 12");
        }
        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(13), 1, so5.GeneratePrice());
            Debug.Log("Added Item ID 13");
        }
        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(14), 1, so6.GeneratePrice());
            Debug.Log("Added Item ID 14");
        }

        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            PrintInventory();
        }
    }

    private void PrintInventory()
    {
        Debug.Log("=== Current Inventory ===");
        if (InventoryController.Instance == null) return;

        foreach (var item in InventoryController.Instance.items)
        {
            Debug.Log($"{item.data.name} x{item.quantity}");
        }
    }
}