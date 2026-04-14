using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class PlayerPanel : MonoBehaviour
{
    public static PlayerPanel Instance { get; private set; }

    [Header("Panels")]
    public GameObject statsPanel;

    [Tooltip("Drag the visual UI Canvas panel here (the background, slots, etc.)")]
    public GameObject inventoryUIPanel;

    [Tooltip("(Optional) Drag the object holding InventoryController here. NOTE: If it's a DontDestroyOnLoad Singleton, it's best to leave this empty so it never turns off!")]
    public GameObject inventoryControllerObject;

    public GameObject gameUI;

    [Header("Animation (Shared)")]
    public Image animationImage;
    public Sprite[] openFrames;
    public float frameRate = 0.05f;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Global Settings")]
    public float openSaturation = -80f;
    public float closeSaturation = 0f;
    public float saturationTweenDuration = 0.3f;

    [Header("Buttons")]
    public Button inventoryButton;
    public Button statsButton;

    [Header("Switching")]
    public bool playAnimationOnSwitch = true;
    public float switchAnimationDurationMultiplier = 0.5f;

    private bool isMenuOpen = false;
    private bool playingAnimation = false;
    private GameObject activePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (statsPanel) statsPanel.SetActive(false);
        if (inventoryUIPanel) inventoryUIPanel.SetActive(false);
        animationImage.gameObject.SetActive(false);

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        // --- CHANGED: Use our new Smart Toggle method for the buttons ---
        if (inventoryButton) inventoryButton.onClick.AddListener(() => OnMenuButtonClicked(inventoryUIPanel));
        if (statsButton) statsButton.onClick.AddListener(() => OnMenuButtonClicked(statsPanel));
    }

    void Update()
    {
        // 1. Hotkey Toggle
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }

        // 2. --- NEW: Click outside to close ---
        if (isMenuOpen && !playingAnimation && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Check if the click happened over the active panel or the menu buttons
            bool clickedInsidePanel = IsMouseOverRectTransform(activePanel, mousePos);
            bool clickedInvButton = IsMouseOverRectTransform(inventoryButton != null ? inventoryButton.gameObject : null, mousePos);
            bool clickedStatsButton = IsMouseOverRectTransform(statsButton != null ? statsButton.gameObject : null, mousePos);

            // If we didn't click on any of our UI elements, close the menu!
            if (!clickedInsidePanel && !clickedInvButton && !clickedStatsButton)
            {
                StartCoroutine(CloseMenu());
            }
        }
    }

    // =========================================================
    // --- NEW: SMART BUTTON TOGGLE LOGIC ---
    // =========================================================
    public void OnMenuButtonClicked(GameObject targetPanel)
    {
        if (playingAnimation) return;

        if (!isMenuOpen)
        {
            // The menu is closed, so open it to the requested panel
            StartCoroutine(PlayOpenAnimation(targetPanel, isFullOpen: true));
        }
        else if (activePanel == targetPanel)
        {
            // The menu is already open AND we clicked the button for the panel we are currently looking at -> Close it!
            StartCoroutine(CloseMenu());
        }
        else
        {
            // The menu is open, but we clicked a different panel's button -> Switch to it!
            SwitchPanel(targetPanel);
        }
    }

    // Helper method to accurately check if the mouse is hovering over a specific UI element
    private bool IsMouseOverRectTransform(GameObject obj, Vector2 mousePos)
    {
        if (obj == null || !obj.activeInHierarchy) return false;

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            // Automatically adapts to whatever Canvas Render Mode you are using
            Canvas canvas = obj.GetComponentInParent<Canvas>();
            Camera uiCamera = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            return RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, uiCamera);
        }
        return false;
    }
    // =========================================================

    public void ToggleMenu()
    {
        if (playingAnimation) return;

        if (isMenuOpen)
        {
            StartCoroutine(CloseMenu());
        }
        else
        {
            OnMenuButtonClicked(statsPanel); // Stats first
        }
    }

    public void OpenMenu(GameObject panelToOpen)
    {
        if (playingAnimation || isMenuOpen) return;
        StartCoroutine(PlayOpenAnimation(panelToOpen, isFullOpen: true));
    }

    private IEnumerator PlayOpenAnimation(GameObject panelToOpen, bool isFullOpen)
    {
        playingAnimation = true;
        isMenuOpen = true;
        activePanel = panelToOpen;

        if (statsPanel) statsPanel.SetActive(activePanel == statsPanel);
        if (inventoryUIPanel) inventoryUIPanel.SetActive(activePanel == inventoryUIPanel);

        if (inventoryControllerObject != null)
        {
            inventoryControllerObject.SetActive(activePanel == inventoryUIPanel);
        }

        if (activePanel == inventoryUIPanel && InventoryController.Instance != null)
        {
            InventoryController.Instance.ForceUIRefresh();
        }

        if (isFullOpen)
        {
            if (gameUI) gameUI.SetActive(false);
            Time.timeScale = 0f;
            if (colorAdjustments != null)
            {
                DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, openSaturation, saturationTweenDuration).SetUpdate(true);
            }
        }

        yield return StartCoroutine(PlayForwardAnimation(isFullOpen ? 1f : switchAnimationDurationMultiplier));

        playingAnimation = false;
    }

    public IEnumerator CloseMenu()
    {
        playingAnimation = true;
        isMenuOpen = false;

        yield return StartCoroutine(PlayReverseAnimation(1f));

        if (colorAdjustments != null)
        {
            DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, closeSaturation, saturationTweenDuration).SetUpdate(true);
        }

        if (statsPanel) statsPanel.SetActive(false);
        if (inventoryUIPanel) inventoryUIPanel.SetActive(false);

        if (inventoryControllerObject != null) inventoryControllerObject.SetActive(false);

        activePanel = null;

        if (gameUI) gameUI.SetActive(true);
        Time.timeScale = 1f;

        playingAnimation = false;
    }

    public void SwitchPanel(GameObject newPanel)
    {
        if (!isMenuOpen || playingAnimation || activePanel == newPanel) return;

        if (playAnimationOnSwitch)
        {
            StartCoroutine(SwitchWithAnimation(newPanel));
        }
        else
        {
            if (activePanel != null) activePanel.SetActive(false);

            if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
            {
                inventoryControllerObject.SetActive(false);
            }

            activePanel = newPanel;

            if (activePanel != null) activePanel.SetActive(true);

            if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
            {
                inventoryControllerObject.SetActive(true);
            }
        }
    }

    private IEnumerator SwitchWithAnimation(GameObject newPanel)
    {
        playingAnimation = true;

        yield return StartCoroutine(PlayReverseAnimation(switchAnimationDurationMultiplier));

        if (activePanel != null) activePanel.SetActive(false);

        if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
        {
            inventoryControllerObject.SetActive(false);
        }

        activePanel = newPanel;

        if (activePanel != null) activePanel.SetActive(true);

        if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
        {
            inventoryControllerObject.SetActive(true);
        }

        if (activePanel == inventoryUIPanel && InventoryController.Instance != null)
        {
            InventoryController.Instance.ForceUIRefresh();
        }

        yield return StartCoroutine(PlayForwardAnimation(switchAnimationDurationMultiplier));

        playingAnimation = false;
    }

    private IEnumerator PlayForwardAnimation(float durationMultiplier)
    {
        animationImage.gameObject.SetActive(true);
        foreach (Sprite sprite in openFrames)
        {
            animationImage.sprite = sprite;
            yield return new WaitForSecondsRealtime(frameRate / durationMultiplier);
        }
        animationImage.gameObject.SetActive(false);
    }

    private IEnumerator PlayReverseAnimation(float durationMultiplier)
    {
        animationImage.gameObject.SetActive(true);
        for (int i = openFrames.Length - 1; i >= 0; i--)
        {
            animationImage.sprite = openFrames[i];
            yield return new WaitForSecondsRealtime(frameRate / durationMultiplier);
        }
        animationImage.gameObject.SetActive(false);
    }
}