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

    // --- CHANGED: Split the Inventory into UI and Logic ---
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

        // We specifically DO NOT turn off the inventoryControllerObject here, 
        // because it needs to stay awake in the background to catch fish!

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        if (inventoryButton) inventoryButton.onClick.AddListener(() => OpenMenu(inventoryUIPanel));
        if (statsButton) statsButton.onClick.AddListener(() => OpenMenu(statsPanel));
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (playingAnimation) return;

        if (isMenuOpen)
        {
            StartCoroutine(CloseMenu());
        }
        else
        {
            OpenMenu(statsPanel); // Stats first
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

        // --- NEW: Toggle the logic object ONLY if you specifically assigned it ---
        if (inventoryControllerObject != null)
        {
            inventoryControllerObject.SetActive(activePanel == inventoryUIPanel);
        }

        // Force the inventory to redraw its items when opened!
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

        // --- NEW: Toggle off the logic object ONLY if assigned ---
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

            // If we are switching AWAY from the inventory, and the controller object is assigned, turn it off
            if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
            {
                inventoryControllerObject.SetActive(false);
            }

            activePanel = newPanel;

            if (activePanel != null) activePanel.SetActive(true);

            // If we are switching TO the inventory, and the controller object is assigned, turn it on
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

        // If we are switching AWAY from the inventory, turn off controller object (if assigned)
        if (activePanel == inventoryUIPanel && inventoryControllerObject != null)
        {
            inventoryControllerObject.SetActive(false);
        }

        activePanel = newPanel;

        if (activePanel != null) activePanel.SetActive(true);

        // If we are switching TO the inventory, turn on controller object (if assigned)
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