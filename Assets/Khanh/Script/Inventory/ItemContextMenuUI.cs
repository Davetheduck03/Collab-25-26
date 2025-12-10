using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

public class ItemContextMenuUI : MonoBehaviour
{
    public static ItemContextMenuUI Instance;

    [Header("UI References")]
    public GameObject menuPanel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    [Header("Settings")]
    public float buttonSpacing = 5f;

    private InventoryItem currentItem;
    private InventoryController inventory;
    private List<GameObject> activeButtons = new();

    public static event Action<InventoryItem, int> OnItemSold;
    public static event Action<InventoryItem> OnItemUsed;
    public static event Action<InventoryItem> OnItemEquipped;
    public static event Action<InventoryItem> OnItemDropped;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(InventoryItem item, InventoryController inv, Vector2 position)
    {
        currentItem = item;
        inventory = inv;

        ClearButtons();
        CreateButtonsForItem(item);
        CreateButton("Cancel", Hide);
        PositionMenu(position);

        menuPanel.SetActive(true);
    }

    public void Hide()
    {
        menuPanel.SetActive(false);
        currentItem = null;
    }

    private void ClearButtons()
    {
        foreach (var button in activeButtons)
        {
            Destroy(button);
        }
        activeButtons.Clear();
    }

    private void CreateButtonsForItem(InventoryItem item)
    {
        switch (item.data.type)
        {
            case ItemType.Consumable:
                CreateButton("Use", OnUseClicked);
                CreateButton("Drop", OnDropClicked);
                break;

            case ItemType.Equippable:
                CreateButton("Equip", OnEquipClicked);
                CreateButton("Sell", OnSellClicked);
                CreateButton("Drop", OnDropClicked);
                break;

            case ItemType.Fish:
                CreateButton($"Sell ({item.sellPrice}g)", OnSellClicked);
                CreateButton("Drop", OnDropClicked);
                break;

            case ItemType.Material:
                CreateButton("Sell", OnSellClicked);
                CreateButton("Drop", OnDropClicked);
                break;

            case ItemType.Quest:
                CreateButton("Examine", OnExamineClicked);
                break;
        }
    }

    private void CreateButton(string label, Action onClick)
    {
        var buttonObj = Instantiate(buttonPrefab, buttonContainer);
        buttonObj.SetActive(true);

        var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }

        var button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => onClick?.Invoke());
        }

        activeButtons.Add(buttonObj);
    }

    private void PositionMenu(Vector2 position)
    {
        RectTransform rect = menuPanel.GetComponent<RectTransform>();

        float pivotX = position.x / Screen.width > 0.5f ? 1 : 0;
        float pivotY = position.y / Screen.height > 0.5f ? 1 : 0;

        rect.pivot = new Vector2(pivotX, pivotY);
        menuPanel.transform.position = position;
    }

    private void OnUseClicked()
    {
        if (currentItem == null) return;

        if (currentItem.data is ConsumableData consumable)
        {
            consumable.OnUse();
        }

        inventory.UseItem(currentItem.data);
        OnItemUsed?.Invoke(currentItem);
        Hide();
    }

    private void OnEquipClicked()
    {
        if (currentItem == null) return;

        if (currentItem.data is EquippableData equippable)
        {
            equippable.OnEquip();
        }

        OnItemEquipped?.Invoke(currentItem);
        Hide();
    }

    private void OnSellClicked()
    {
        if (currentItem == null) return;

        int sellPrice = currentItem.sellPrice;

        Debug.Log($"Sold {currentItem.data.displayName} for {sellPrice}g");

        inventory.RemoveItem(currentItem);
        OnItemSold?.Invoke(currentItem, sellPrice);
        Hide();
    }

    private void OnDropClicked()
    {
        if (currentItem == null) return;

        Debug.Log($"Dropped {currentItem.data.displayName}");

        inventory.RemoveItem(currentItem);
        OnItemDropped?.Invoke(currentItem);
        Hide();
    }

    private void OnExamineClicked()
    {
        if (currentItem == null) return;

        Debug.Log($"Examining: {currentItem.data.displayName}\n{currentItem.data.description}");
        Hide();
    }

    private void Update()
    {
        if (!menuPanel.activeSelf) return;

        // Close menu when clicking outside
        if (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            RectTransform rect = menuPanel.GetComponent<RectTransform>();
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, null))
            {
                Hide();
            }
        }

        // Close with Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Hide();
        }
    }
}