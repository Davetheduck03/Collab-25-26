using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Drag your Gold TextMeshPro object here")]
    public TextMeshProUGUI goldText;

    private void OnEnable()
    {
        // Listen for changes in the player's wallet
        CurrencyManager.OnCurrencyChanged += UpdateGoldDisplay;
    }

    private void OnDisable()
    {
        // Stop listening when the UI is turned off/destroyed
        CurrencyManager.OnCurrencyChanged -= UpdateGoldDisplay;
    }

    private void Start()
    {
        // When the game starts, grab the current amount immediately
        if (CurrencyManager.Instance != null)
        {
            UpdateGoldDisplay(CurrencyManager.Instance.GetCurrency());
        }
    }

    private void UpdateGoldDisplay(int currentGold)
    {
        if (goldText != null)
        {
            // You can format this however you like! 
            // The "N0" adds commas for big numbers (e.g. 1,000)
            goldText.text = currentGold.ToString("N0") + " <color=#FFD700>G</color>";
        }
    }
}