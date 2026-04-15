using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // --- NEW: Required for checking mouse clicks ---

public class SettingMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Drag the visual UI panel of the settings menu here so we can turn it on/off.")]
    public GameObject settingsUIPanel;

    [Tooltip("Drag the visual UI panel of the instructions here.")]
    public GameObject instructionUIPanel; // <--- NEW: Instruction Panel

    [Header("Buttons (Prevents instant-closing)")]
    [Tooltip("(Optional) Drag the button used to open settings here.")]
    public GameObject settingsButton;
    [Tooltip("(Optional) Drag the button used to open instructions here.")]
    public GameObject instructionButton;

    [Header("Volume Sliders")]
    public Slider masterSlider;
    public Slider sfxSlider;

    public bool isMenuOpen { get; private set; } = false;
    public bool isInstructionOpen { get; private set; } = false; // <--- NEW: Tracks instruction state

    private void Start()
    {
        LoadVolumes();

        // Ensure the menus are hidden when the game starts
        if (settingsUIPanel != null) settingsUIPanel.SetActive(false);
        if (instructionUIPanel != null) instructionUIPanel.SetActive(false);

        isMenuOpen = false;
        isInstructionOpen = false;
    }

    private void Update()
    {
        // --- NEW: Click outside to close logic ---
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // 1. Check if the Instruction Panel is open on top
            if (isInstructionOpen && instructionUIPanel != null)
            {
                bool clickedInsideInstructions = IsMouseOverRectTransform(instructionUIPanel, mousePos);
                bool clickedInstructionBtn = IsMouseOverRectTransform(instructionButton, mousePos);

                // If they clicked outside the instruction panel AND didn't click the open button, close it!
                if (!clickedInsideInstructions && !clickedInstructionBtn)
                {
                    CloseInstructionPanel();
                }
            }
            // 2. Otherwise, check if the Settings Panel is open
            else if (isMenuOpen && settingsUIPanel != null)
            {
                bool clickedInsideSettings = IsMouseOverRectTransform(settingsUIPanel, mousePos);
                bool clickedSettingsBtn = IsMouseOverRectTransform(settingsButton, mousePos);

                // If they clicked outside the settings panel AND didn't click the open button, close it!
                if (!clickedInsideSettings && !clickedSettingsBtn)
                {
                    ToggleMenu();
                }
            }
        }
    }

    // ==================== PANEL TOGGLES ====================

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        if (settingsUIPanel != null)
        {
            settingsUIPanel.SetActive(isMenuOpen);
        }

        // If we are closing the settings menu, automatically close the instructions too just in case
        if (!isMenuOpen && isInstructionOpen)
        {
            CloseInstructionPanel();
        }
    }

    // --- NEW: Public function to open the instruction panel ---
    public void OpenInstructionPanel()
    {
        isInstructionOpen = true;
        if (instructionUIPanel != null)
        {
            instructionUIPanel.SetActive(true);
        }
    }

    // --- NEW: Public function to close the instruction panel ---
    public void CloseInstructionPanel()
    {
        isInstructionOpen = false;
        if (instructionUIPanel != null)
        {
            instructionUIPanel.SetActive(false);
        }
    }

    // ==================== HELPER METHOD ====================
    // Accurately checks if the mouse is hovering over a specific UI element
    private bool IsMouseOverRectTransform(GameObject obj, Vector2 mousePos)
    {
        if (obj == null || !obj.activeInHierarchy) return false;

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            Canvas canvas = obj.GetComponentInParent<Canvas>();
            Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            return RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, uiCamera);
        }
        return false;
    }

    // ==================== VOLUME ====================
    public void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void LoadVolumes()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (masterSlider != null) masterSlider.value = master;
        if (sfxSlider != null) sfxSlider.value = sfx;

        AudioListener.volume = master;
    }
}