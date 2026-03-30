using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class PlayerPanel : MonoBehaviour
{
    // --- NEW: Singleton Instance ---
    public static PlayerPanel Instance { get; private set; }

    [Header("Panels")]
    public GameObject statsPanel;
    public GameObject inventoryPanel;
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

    // --- NEW: Awake method to set up the Instance ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroys duplicates if you reload the scene
            return;
        }
        Instance = this;
    }

    void Start()
    {
        statsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        animationImage.gameObject.SetActive(false);

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }

        if (inventoryButton) inventoryButton.onClick.AddListener(() => OpenMenu(inventoryPanel));
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

        statsPanel.SetActive(activePanel == statsPanel);
        inventoryPanel.SetActive(activePanel == inventoryPanel);

        // --- NEW: Force the inventory to redraw its items when opened! ---
        if (activePanel == inventoryPanel && InventoryController.Instance != null)
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
    public IEnumerator CloseMenu() // Changed to public in case you want to close it from another script!
    {
        playingAnimation = true;
        isMenuOpen = false;

        yield return StartCoroutine(PlayReverseAnimation(1f)); // Full speed for close

        if (colorAdjustments != null)
        {
            DOTween.To(() => colorAdjustments.saturation.value, x => colorAdjustments.saturation.value = x, closeSaturation, saturationTweenDuration).SetUpdate(true);
        }

        statsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
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
            activePanel.SetActive(false);
            activePanel = newPanel;
            activePanel.SetActive(true);
        }
    }

    private IEnumerator SwitchWithAnimation(GameObject newPanel)
    {
        playingAnimation = true;

        // Play close animation (reverse)
        yield return StartCoroutine(PlayReverseAnimation(switchAnimationDurationMultiplier));

        // Swap panels (no visual during swap to avoid flicker)
        activePanel.SetActive(false);
        activePanel = newPanel;
        activePanel.SetActive(true);

        // --- NEW: Force the inventory to redraw its items when switching to it! ---
        if (activePanel == inventoryPanel && InventoryController.Instance != null)
        {
            InventoryController.Instance.ForceUIRefresh();
        }

        // Play open animation (forward)
        yield return StartCoroutine(PlayForwardAnimation(switchAnimationDurationMultiplier));

        playingAnimation = false;
    }

    // Helper: Play frames forward
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

    // Helper: Play frames reverse
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