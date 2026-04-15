using UnityEngine;
using UnityEngine.UI;
using TMPro; // Used for text, remove if you use legacy Unity Text

public class ExperienceUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the UI Image that represents the EXP bar here. Image Type must be Filled!")]
    public Image expFillBar;

    [Tooltip("(Optional) Drag a TextMeshPro text here to show the level number.")]
    public TMP_Text levelText;

    [Tooltip("(Optional) Drag a TextMeshPro text here to show '50/100' numbers.")]
    public TMP_Text expText;

    private void OnEnable()
    {
        // Subscribe to the events in your ExperienceManager
        ExperienceManager.OnExpChanged += UpdateExpBar;
        ExperienceManager.OnLevelUp += UpdateLevelText;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks!
        ExperienceManager.OnExpChanged -= UpdateExpBar;
        ExperienceManager.OnLevelUp -= UpdateLevelText;
    }

    private void Start()
    {
        // Force an initial update so the UI is correct the moment the game starts
        if (ExperienceManager.Instance != null)
        {
            UpdateExpBar(ExperienceManager.Instance.CurrentExp, ExperienceManager.Instance.ExpRequired);
            UpdateLevelText(ExperienceManager.Instance.CurrentLevel);
        }
    }

    private void UpdateExpBar(float currentExp, float expRequired)
    {
        if (expFillBar != null && expRequired > 0)
        {
            // Calculate the percentage (0.0 to 1.0) for the fill amount
            expFillBar.fillAmount = currentExp / expRequired;
        }

        // Optional: Update text to say "50 / 100"
        if (expText != null)
        {
            expText.text = $"{currentExp} / {expRequired}";
        }
    }

    private void UpdateLevelText(int newLevel)
    {
        // Optional: Update text to say "Lv. 5"
        if (levelText != null)
        {
            levelText.text = $"Lv. {newLevel}";
        }
    }
}