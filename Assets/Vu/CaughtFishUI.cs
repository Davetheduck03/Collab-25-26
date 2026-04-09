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
    public static CatchFishUI Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        closeButton.onClick.AddListener(ClosePanel);
    }
    private void Start()
    {
        gameObject.SetActive(false);
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