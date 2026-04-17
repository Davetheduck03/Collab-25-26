using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using UnityEngine.UI; // <--- NEW: Required for UI Sliders
using Phuc.SoundSystem; // <--- NEW: Required to talk to your SoundManager

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Header("Confirmation Panels")]
    [Tooltip("Drag your Main Menu Confirmation Panel here.")]
    public GameObject mainMenuConfirmPanel;
    [Tooltip("Drag your Reset Day Confirmation Panel here.")]
    public GameObject resetDayConfirmPanel;

    // ==========================================
    // --- NEW: AUDIO SLIDERS ---
    // ==========================================
    [Header("Audio Settings")]
    public Slider bgmSlider;
    public Slider sfxSlider;

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

        // ==========================================
        // --- NEW: LOAD SAVED AUDIO SETTINGS ---
        // ==========================================
        if (bgmSlider != null)
        {
            // Load saved value (default to 0.75 if no save exists)
            bgmSlider.value = PlayerPrefs.GetFloat("SavedBGMVolume", 0.75f);

            // Apply it immediately
            UpdateBGMVolume(bgmSlider.value);

            // Listen for when the player drags the slider
            bgmSlider.onValueChanged.AddListener(UpdateBGMVolume);
        }

        if (sfxSlider != null)
        {
            // Load saved value (default to 0.75 if no save exists)
            sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVolume", 0.75f);

            // Apply it immediately
            UpdateSFXVolume(sfxSlider.value);

            // Listen for when the player drags the slider
            sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);
        }
    }

    void Update()
    {
        // --- Smart Escape Logic ---
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (mainMenuConfirmPanel.activeSelf) CloseMainMenuConfirm();
            else if (resetDayConfirmPanel.activeSelf) CloseResetDayConfirm();
            else TogglePause();
        }
    }

    // ==========================================
    // --- NEW: VOLUME UPDATE METHODS ---
    // ==========================================
    public void UpdateBGMVolume(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetBGMVolume(value);
        }
        // Save the setting so it remembers next time!
        PlayerPrefs.SetFloat("SavedBGMVolume", value);
    }

    public void UpdateSFXVolume(float value)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSFXVolume(value);
        }
        // Save the setting so it remembers next time!
        PlayerPrefs.SetFloat("SavedSFXVolume", value);
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

            if (colorAdjustments != null)
            {
                DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, -100f, duration).SetUpdate(true);
            }
        }
        else
        {
            HidePanel(mainMenuConfirmPanel);
            HidePanel(resetDayConfirmPanel);

            HidePanel(pausePanel, () =>
            {
                Time.timeScale = 1f;
            });

            if (colorAdjustments != null)
            {
                DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, 0f, duration).SetUpdate(true);
            }
        }
    }

    // ... (Keep the rest of your Main Menu, Reset Day, and DOTween methods exactly the same) ...

    public void OpenMainMenuConfirm() { ShowPanel(mainMenuConfirmPanel); }
    public void CloseMainMenuConfirm() { HidePanel(mainMenuConfirmPanel); }
    public void ConfirmMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuSceneName); }

    public void OpenResetDayConfirm() { ShowPanel(resetDayConfirmPanel); }
    public void CloseResetDayConfirm() { HidePanel(resetDayConfirmPanel); }
    public void ConfirmResetDay()
    {
        if (QuotaManager.Instance != null && CurrencyManager.Instance != null)
        {
            int goldMadeToday = QuotaManager.Instance.GoldEarned;
            if (goldMadeToday > 0) CurrencyManager.Instance.RemoveCurrency(goldMadeToday);
            QuotaManager.Instance.ResetRun();
        }
        if (InventoryController.Instance != null) InventoryController.Instance.ClearAllItems();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetupPanel(GameObject panel) { if (panel == null) return; panel.transform.localScale = Vector3.zero; panel.SetActive(false); }
    private void ShowPanel(GameObject panel) { if (panel == null) return; panel.transform.DOKill(); panel.SetActive(true); panel.transform.DOScale(Vector3.one, duration).SetEase(openEase).SetUpdate(true); }
    private void HidePanel(GameObject panel, System.Action onComplete = null) { if (panel == null || !panel.activeSelf) return; panel.transform.DOKill(); panel.transform.DOScale(Vector3.zero, duration).SetEase(closeEase).SetUpdate(true).OnComplete(() => { panel.SetActive(false); onComplete?.Invoke(); }); }
}