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
    public EquippableData eq1;
    public EquippableData eq2;
    public EquippableData eq3;
    public EquippableData eq4;
    public EquippableData eq5;
    public EquippableData eq6;
    public EquippableData eq7;
    public EquippableData eq8;
    public EquippableData eq9;
    public EquippableData eq10;
    public EquippableData eq11;
    public EquippableData eq12;

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

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(2), 1);
            Debug.Log("Added Item ID 2");
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(3), 1);
            Debug.Log("Added Item ID 3");
        }
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(4), 1);
            Debug.Log("Added Item ID 4");
        }
        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(5), 1);
            Debug.Log("Added Item ID 5");
        }
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(6), 1);
            Debug.Log("Added Item ID 6");
        }
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(7), 1);
            Debug.Log("Added Item ID 7");
        }
        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(8), 1);
            Debug.Log("Added Item ID 8");
        }
        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(15), 1);
            Debug.Log("Added Item ID 15");
        }
            if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(16), 1);
                Debug.Log("Added Item ID 16");
            }
            if (Keyboard.current.f10Key.wasPressedThisFrame)
            {
                InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(17), 1);
                Debug.Log("Added Item ID 17");
            }
            if (Keyboard.current.f11Key.wasPressedThisFrame)
            {
                InventoryController.Instance.AddItem(InventoryController.Instance.GetItemFromID(18), 1);
                Debug.Log("Added Item ID 18");
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