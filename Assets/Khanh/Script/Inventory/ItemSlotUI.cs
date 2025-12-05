using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text quantityText;

    private InventoryItem currentItem;
    private InventoryController inventory;

    public void SetEmpty()
    {
        currentItem = null;
        icon.enabled = false;
        quantityText.text = "";
    }

    public void SetItem(InventoryItem item, InventoryController inv)
    {
        currentItem = item;
        inventory = inv;

        icon.enabled = true;
        icon.sprite = item.data.Sprite;

        quantityText.text = item.data.isStackable ? item.quantity.ToString() : "";
    }

    public void OnClick()
    {
        if (currentItem == null) return;

        inventory.UseItem(currentItem.data);
        InventoryUI.Instance.RefreshUI();
    }
}
