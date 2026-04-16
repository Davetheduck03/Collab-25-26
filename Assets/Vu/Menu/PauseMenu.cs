using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Header("Confirmation Panels")]
    [Tooltip("Drag your Main Menu Confirmation Panel here.")]
    public GameObject mainMenuConfirmPanel;
    [Tooltip("Drag your Reset Day Confirmation Panel here.")]
    public GameObject resetDayConfirmPanel;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private bool isPaused = false;

    void Start()
    {
        // Hide all panels initially
        SetupPanel(pausePanel);
        SetupPanel(mainMenuConfirmPanel);
        SetupPanel(resetDayConfirmPanel);

        // Try to get color adjustment
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogError("[PauseMenu] No ColorAdjustments override found in the Volume!");
            }
        }
    }

    void Update()
    {
        // --- Smart Escape Logic ---
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 1. If a confirmation panel is open, ESC closes it
            if (mainMenuConfirmPanel.activeSelf)
            {
                CloseMainMenuConfirm();
            }
            else if (resetDayConfirmPanel.activeSelf)
            {
                CloseResetDayConfirm();
            }
            // 2. Otherwise, toggle the pause menu normally
            else
            {
                TogglePause();
            }
        }
    }

    // ==========================================
    // PAUSE TOGGLE
    // ==========================================
    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            ShowPanel(pausePanel);

            // Desaturate screen
            if (colorAdjustments != null)
            {
                DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, -100f, duration)
                    .SetUpdate(true);
            }
        }
        else
        {
            // Close everything when unpausing just in case
            HidePanel(mainMenuConfirmPanel);
            HidePanel(resetDayConfirmPanel);

            HidePanel(pausePanel, () =>
            {
                Time.timeScale = 1f; // Only unpause time AFTER animation finishes
            });

            // Resaturate screen
            if (colorAdjustments != null)
            {
                DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, 0f, duration)
                    .SetUpdate(true);
            }
        }
    }

    // ==========================================
    // MAIN MENU CONFIRMATION
    // ==========================================
    public void OpenMainMenuConfirm()
    {
        ShowPanel(mainMenuConfirmPanel);
    }

    public void CloseMainMenuConfirm()
    {
        HidePanel(mainMenuConfirmPanel);
    }

    public void ConfirmMainMenu()
    {
        // IMPORTANT: Unfreeze time before loading a new scene!
        Time.timeScale = 1f;

        // Optional: Add scene transition logic here if you have it
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ==========================================
    // RESET DAY CONFIRMATION
    // ==========================================
    public void OpenResetDayConfirm()
    {
        ShowPanel(resetDayConfirmPanel);
    }

    public void CloseResetDayConfirm()
    {
        HidePanel(resetDayConfirmPanel);
    }

    public void ConfirmResetDay()
    {
        // 1. DEDUCT TODAY'S GOLD
        // We use the QuotaManager to find out exactly how much we made *today*
        if (QuotaManager.Instance != null && CurrencyManager.Instance != null)
        {
            int goldMadeToday = QuotaManager.Instance.GoldEarned;
            if (goldMadeToday > 0)
            {
                CurrencyManager.Instance.RemoveCurrency(goldMadeToday);
                Debug.Log($"[Reset Day] Deducted {goldMadeToday} gold that was earned today.");
            }

            // Tell QuotaManager to reset today's tracking back to 0
            QuotaManager.Instance.ResetRun();
        }

        // 2. WIPE INVENTORY
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.ClearAllItems();
        }

        // 3. UNFREEZE TIME & RELOAD
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ==========================================
    // DOTWEEN HELPER METHODS
    // ==========================================
    private void SetupPanel(GameObject panel)
    {
        if (panel == null) return;
        panel.transform.localScale = Vector3.zero;
        panel.SetActive(false);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        panel.transform.DOKill();
        panel.SetActive(true);
        panel.transform.DOScale(Vector3.one, duration).SetEase(openEase).SetUpdate(true);
    }

    private void HidePanel(GameObject panel, System.Action onComplete = null)
    {
        if (panel == null || !panel.activeSelf) return;
        panel.transform.DOKill();
        panel.transform.DOScale(Vector3.zero, duration).SetEase(closeEase).SetUpdate(true).OnComplete(() =>
        {
            panel.SetActive(false);
            onComplete?.Invoke();
        });
    }
}