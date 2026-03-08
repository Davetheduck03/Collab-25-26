using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to each augment card prefab. Wire up icon, title, description, and button in the Inspector.
/// </summary>
public class AugmentCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image rarityBorder;   // optional — tint border by rarity
    [SerializeField] private Button button;

    [Header("Rarity Colors (optional)")]
    public Color colorCommon   = Color.white;
    public Color colorUncommon = Color.cyan;
    public Color colorRare     = Color.blue;
    public Color colorEpic     = new Color(0.6f, 0f, 1f);
    public Color colorMythic   = Color.yellow;

    private AugmentSO currentAugment;

    /// <summary>Populate the card and register the pick callback.</summary>
    public void Setup(AugmentSO data, Action<AugmentSO> onPicked)
    {
        currentAugment = data;

        if (icon != null)           icon.sprite = data.icon;
        if (titleText != null)      titleText.text = data.displayName;
        if (descriptionText != null) descriptionText.text = data.description;

        if (rarityBorder != null)
            rarityBorder.color = GetRarityColor(data.rarity);

        if (button == null)
        {
            // Try to find the Button component on this GameObject or its children
            button = GetComponent<Button>() ?? GetComponentInChildren<Button>();
        }

        if (button == null)
        {
            Debug.LogError($"[AugmentCardUI] '{gameObject.name}' has no Button reference! " +
                           "Drag the Button component into the 'Button' field in the Inspector.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onPicked?.Invoke(currentAugment));
        Debug.Log($"[AugmentCardUI] '{gameObject.name}' button listener registered for '{data.displayName}'.");
    }

    private Color GetRarityColor(Rarity rarity) => rarity switch
    {
        Rarity.Common   => colorCommon,
        Rarity.Uncommon => colorUncommon,
        Rarity.Rare     => colorRare,
        Rarity.Epic     => colorEpic,
        Rarity.Mythic   => colorMythic,
        _               => colorCommon
    };
}
