using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject statsPanel;     // Your stats UI
    public GameObject gameUI;         // Main UI

    [Header("Animation (Frame-by-frame)")]
    public Image animationImage;      // UI Image for frames
    public Sprite[] openFrames;       // Frames when opening
    public float frameRate = 0.05f;   // Seconds per frame

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Global Volume Settings")]
    public float openSaturation = -80f;
    public float closeSaturation = 0f;
    public float saturationTweenDuration = 0.3f;

    private bool isOpen = false;
    private bool playingAnimation = false;

    void Start()
    {
        statsPanel.SetActive(false);
        animationImage.gameObject.SetActive(false);

        // Try to get color adjustments
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogWarning("No ColorAdjustments override found in Volume.");
            }
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.iKey.wasPressedThisFrame)
        {
            TogglePanel();
        }
    }

    // Called by keyboard or UI Button
    public void TogglePanel()
    {
        if (playingAnimation) return;

        if (isOpen)
        {
            StartCoroutine(PlayCloseAnimation());
        }
        else
        {
            StartCoroutine(PlayOpenAnimation());
        }
    }

    IEnumerator PlayOpenAnimation()
    {
        playingAnimation = true;
        isOpen = true;
        statsPanel.SetActive(true);
        // Disable gameplay UI and pause game
        if (gameUI) gameUI.SetActive(false);
        Time.timeScale = 0f;

        // Post-processing saturation
        if (colorAdjustments != null)
        {
            DOTween.To(
                () => colorAdjustments.saturation.value,
                x => colorAdjustments.saturation.value = x,
                openSaturation,
                saturationTweenDuration
            ).SetUpdate(true);
        }

        // Show animation image
        animationImage.gameObject.SetActive(true);

        foreach (Sprite sprite in openFrames)
        {
            animationImage.sprite = sprite;
            yield return new WaitForSecondsRealtime(frameRate);
        }

        animationImage.gameObject.SetActive(false);

        playingAnimation = false;
    }

    IEnumerator PlayCloseAnimation()
    {
        playingAnimation = true;
        isOpen = false;

        animationImage.gameObject.SetActive(true);

        // Play openFrames in reverse
        for (int i = openFrames.Length - 1; i >= 0; i--)
        {
            animationImage.sprite = openFrames[i];
            yield return new WaitForSecondsRealtime(frameRate);
        }

        animationImage.gameObject.SetActive(false);

        // Post-processing return to normal
        if (colorAdjustments != null)
        {
            DOTween.To(
                () => colorAdjustments.saturation.value,
                x => colorAdjustments.saturation.value = x,
                closeSaturation,
                saturationTweenDuration
            ).SetUpdate(true);
        }

        statsPanel.SetActive(false);

        // Return gameplay
        if (gameUI) gameUI.SetActive(true);
        Time.timeScale = 1f;

        playingAnimation = false;
    }
}
