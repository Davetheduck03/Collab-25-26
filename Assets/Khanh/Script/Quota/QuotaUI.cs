using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the current gold quota progress.
///
/// Setup:
///  1. Create a UI element in your HUD canvas.
///  2. Attach this script.
///  3. Wire up quotaText (required) and progressBar (optional) in the Inspector.
///
/// Example display: "247 / 500 G"
/// </summary>
public class QuotaUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text showing 'currentEarned / target G'")]
    [SerializeField] private TMP_Text quotaText;

    [Tooltip("(Optional) Image with fill type — fill amount reflects quota progress.")]
    [SerializeField] private Image progressBar;

    [Header("Display")]
    [Tooltip("Text shown once the quota is met.")]
    [SerializeField] private string quotaMetMessage = "QUOTA MET!";
    [Tooltip("Color of the text when quota is met.")]
    [SerializeField] private Color quotaMetColor  = Color.green;
    [Tooltip("Normal text color.")]
    [SerializeField] private Color normalColor    = Color.white;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void OnEnable()
    {
        QuotaManager.OnGoldEarned += UpdateUI;
        QuotaManager.OnQuotaMet   += HandleQuotaMet;
        QuotaManager.OnRunFailed  += HandleRunFailed;
    }

    private void OnDisable()
    {
        QuotaManager.OnGoldEarned -= UpdateUI;
        QuotaManager.OnQuotaMet   -= HandleQuotaMet;
        QuotaManager.OnRunFailed  -= HandleRunFailed;
    }

    private void Start()
    {
        // Populate immediately if QuotaManager is already alive
        if (QuotaManager.Instance != null)
            UpdateUI(QuotaManager.Instance.GoldEarned, QuotaManager.Instance.GoldTarget);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void UpdateUI(int current, int target)
    {
        if (quotaText != null)
        {
            quotaText.text  = $"{current} / {target} G";
            quotaText.color = (current >= target) ? quotaMetColor : normalColor;
        }

        if (progressBar != null)
            progressBar.fillAmount = (target > 0) ? Mathf.Clamp01((float)current / target) : 0f;
    }

    private void HandleQuotaMet()
    {
        if (quotaText != null)
        {
            quotaText.text  = quotaMetMessage;
            quotaText.color = quotaMetColor;
        }

        if (progressBar != null)
            progressBar.fillAmount = 1f;
    }

    private void HandleRunFailed()
    {
        // The game-over screen/logic is handled elsewhere.
        // You can add visual feedback here if desired (e.g. flash red).
        if (quotaText != null)
            quotaText.color = Color.red;
    }
}
