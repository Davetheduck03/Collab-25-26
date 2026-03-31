using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CatchFishUI : MonoBehaviour
{
    public Image fishIcon;
    public TextMeshProUGUI fishNameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI rarityText;
    public Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void ShowCatchResult(EnemySO caughtFish)
    {
        if (caughtFish == null) return;

        gameObject.SetActive(true);

        fishNameText.text = caughtFish.UnitName;
        priceText.text = $"Price: ${caughtFish.GeneratePrice()}";

        if (fishIcon != null && caughtFish.Icon != null)
            fishIcon.sprite = caughtFish.Icon;

        if (rarityText != null)
            rarityText.text = $"Rarity: {caughtFish.rarity}";

        // Simple DOTween animation
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    public void ClosePanel()
    {
        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                 .OnComplete(() => gameObject.SetActive(false));
    }
}