using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image icon;
    public TMP_Text quantityText;

    private InventoryItem currentItem;
    private InventoryController inventory;

    public void SetItem(InventoryItem item, InventoryController inv)
    {
        currentItem = item;
        inventory = inv;

        icon.enabled = true;
        icon.sprite = item.data.Sprite;
        if (item.data.type == ItemType.Fish)
        {
            quantityText.text = $"{item.sellPrice}g";
        }
        else 
        { 
            quantityText.text = item.data.isStackable ? item.quantity.ToString() : "";
        }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            ItemTooltipUI.Instance?.Show(currentItem);
        }
    }

    // Hover exit - hide tooltip
    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    // Click - show context menu
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // Left Click for context menu
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ItemContextMenuUI.Instance?.Show(currentItem, inventory, eventData.position);
            ItemTooltipUI.Instance?.Hide();
        }
    }
}
