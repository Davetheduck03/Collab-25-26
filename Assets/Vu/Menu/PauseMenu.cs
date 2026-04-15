using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pausePanel;
    public GameObject gameUI;

    [Tooltip("Drag your SettingMenu GameObject here so the Pause Menu knows if it is open!")]
    public SettingMenu settingMenu; // <--- NEW: Reference to the Settings Menu

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
        // Hide panel initially
        pausePanel.localScale = Vector3.zero;
        pausePanel.gameObject.SetActive(false);

        // Try to get color adjustment
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogError(" No ColorAdjustments override found in the Volume!");
            }
        }
        else
        {
            Debug.LogError(" Global Volume is not assigned!");
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // --- NEW: Smart Escape Logic ---
            // If the settings menu is open, pressing ESC should ONLY close the settings, 
            // and leave the game paused on the Pause Menu.
            if (settingMenu != null && settingMenu.isMenuOpen)
            {
                settingMenu.ToggleMenu();
            }
            else
            {
                // Otherwise, do the normal Pause/Unpause toggle
                TogglePause();
            }
        }
    }

    public void TogglePause() // Changed to public so UI Buttons can call it if needed!
    {
        isPaused = !isPaused;
        Debug.Log($"Pause toggled: {isPaused}");

        if (isPaused)
        {
            // Pause gameplay
            Time.timeScale = 0f;

            pausePanel.gameObject.SetActive(true);
            if (gameUI) gameUI.SetActive(false);

            // Animate panel
            pausePanel.DOScale(1f, duration)
                .SetEase(openEase)
                .SetUpdate(true); // run even when Time.timeScale = 0

            // Desaturate screen
            if (colorAdjustments != null)
            {
                DOTween.To(
                    () => colorAdjustments.saturation.value,
                    x => colorAdjustments.saturation.value = x,
                    -100f,
                    duration
                ).SetUpdate(true);
            }
        }
        else
        {
            // Unpause
            pausePanel.DOScale(0f, duration)
                .SetEase(closeEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    pausePanel.gameObject.SetActive(false);
                    if (gameUI) gameUI.SetActive(true);
                    Time.timeScale = 1f;
                });

            if (colorAdjustments != null)
            {
                DOTween.To(
                    () => colorAdjustments.saturation.value,
                    x => colorAdjustments.saturation.value = x,
                    0f,
                    duration
                ).SetUpdate(true);
            }
        }
    }
}