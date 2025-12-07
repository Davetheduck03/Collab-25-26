using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text quantityText;

    private InventoryItem currentItem;
    private InventoryController inventory;

    public void SetItem(InventoryItem item, InventoryController inv)
    {
        Debug.Log($"SetItem called for: {item.data.displayName}");
        Debug.Log($"Icon object active: {icon.gameObject.activeInHierarchy}");
        Debug.Log($"Icon enabled before: {icon.enabled}");

        currentItem = item;
        inventory = inv;

        icon.enabled = true;
        icon.sprite = item.data.Sprite;

        quantityText.text = item.data.isStackable ? item.quantity.ToString() : "";
    }

    public void SetEmpty()
    {
        currentItem = null;
        icon.enabled = false;  // Deactivate instead of just disabling
        quantityText.text = "";
    }

    public void OnClick()
    {
        if (currentItem == null) return;

        inventory.UseItem(currentItem.data);
        InventoryUI.Instance.RefreshUI();
    }
}
