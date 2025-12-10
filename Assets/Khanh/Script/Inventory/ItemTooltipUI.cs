using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance;

    [Header("UI References")]
    public GameObject tooltipPanel;
    public Image itemIcon;
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public TMP_Text itemType;
    public TMP_Text itemPrice;
    public TMP_Text itemRarity;

    [Header("Settings")]
    public Vector2 offset = new Vector2(15f, -15f);

    private RectTransform tooltipRect;
    private RectTransform canvasRect;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            FollowMouse();
        }
    }

    private void FollowMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;

        float pivotX = 0f;
        float pivotY = 1f;

        if (mousePos.x + tooltipWidth + offset.x > Screen.width)
        {
            pivotX = 1f;
        }

        if (mousePos.y - tooltipHeight + offset.y < 0)
        {
            pivotY = 0f;
        }

        tooltipRect.pivot = new Vector2(pivotX, pivotY);

        Vector2 finalOffset = new Vector2(
            pivotX == 0 ? offset.x : -offset.x,
            pivotY == 1 ? offset.y : -offset.y
        );

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            tooltipPanel.transform.position = mousePos + finalOffset;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                mousePos,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            tooltipRect.localPosition = localPoint + finalOffset;
        }
    }

    public void Show(InventoryItem item)
    {
        if (item == null || item.data == null) return;

        //itemIcon.sprite = item.data.Sprite;
        itemName.text = item.data.displayName;
        itemDescription.text = item.data.description;
        itemType.text = item.data.type.ToString();

        if (item.data.type == ItemType.Fish)
        {
            itemPrice.gameObject.SetActive(true);
            itemPrice.text = $"Value: {item.sellPrice}g";
        }
        else
        {
            itemPrice.gameObject.SetActive(false);
        }

        if (item.data is FishItemData fishData && fishData.fishData != null)
        {
            itemRarity.gameObject.SetActive(true);
            itemRarity.text = fishData.fishData.rarity.ToString();
            itemRarity.color = GetRarityColor(fishData.fishData.rarity);
        }
        else
        {
            itemRarity.gameObject.SetActive(false);
        }

        tooltipPanel.SetActive(true);
        FollowMouse();
    }

    public void Hide()
    {
        tooltipPanel.SetActive(false);
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Color.white,
            Rarity.Uncommon => Color.green,
            Rarity.Rare => new Color(0.3f, 0.5f, 1f),
            Rarity.Epic => new Color(0.6f, 0.2f, 0.8f),
            Rarity.Legendary => new Color(1f, 0.5f, 0f),
            Rarity.Mythic => new Color(1f, 0.2f, 0.2f),
            _ => Color.white
        };
    }
}