using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

[System.Serializable]
public class InstructionStep
{
    [Tooltip("The image showing the controls or action.")]
    public Sprite instructionImage;

    [TextArea(3, 5)]
    [Tooltip("The text explaining what to do.")]
    public string instructionText;
}

public class InstructionPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The text at the top that says 'Top-Down Mode' or 'Fishing Mode'.")]
    public TMP_Text modeTitleText;

    [Tooltip("The image that changes for each step.")]
    public Image displayImage;

    [Tooltip("The text that changes for each step.")]
    public TMP_Text displayDescription;

    [Tooltip("Add a 'Canvas Group' component to the object holding your Image and Text, then drag it here! This makes them fade together.")]
    public CanvasGroup contentCanvasGroup;

    [Header("Buttons")]
    [Tooltip("Drag your X (Close) button here so clicking it doesn't cycle the instructions!")]
    public GameObject closeButton; // <--- NEW: Tells the script to ignore clicks on the X button

    [Header("Hint Settings")]
    public TMP_Text hintText;
    public float hintDelay = 3f;

    [Header("Instruction Data")]
    public InstructionStep[] topDownInstructions;
    public InstructionStep[] fishingInstructions;

    [Header("Settings")]
    public float fadeDuration = 0.25f;
    public string topDownTitle = "Top-Down Mode";
    public string fishingTitle = "Fishing Mode";

    private int currentIndex = 0;
    private bool isFading = false;

    private int TotalInstructions => topDownInstructions.Length + fishingInstructions.Length;

    private void OnEnable()
    {
        currentIndex = 0;
        isFading = false;

        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.DOKill();
            contentCanvasGroup.alpha = 1f;
        }

        UpdateUI();
        RestartHintTimer();
    }

    private void Update()
    {
        if (!isFading && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (IsMouseOverRectTransform(gameObject, mousePos))
            {
                // --- NEW: If they clicked exactly on the X button, do NOT cycle the instructions! ---
                if (closeButton != null && IsMouseOverRectTransform(closeButton, mousePos))
                {
                    return; // Stop here, let Unity's normal Button component handle the click.
                }

                CycleInstruction();
            }
        }
    }

    public void CycleInstruction()
    {
        if (isFading || TotalInstructions == 0) return;

        isFading = true;

        if (hintText != null)
        {
            hintText.DOKill();
            hintText.alpha = 0f;
        }

        contentCanvasGroup.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            currentIndex++;
            if (currentIndex >= TotalInstructions)
            {
                currentIndex = 0;
            }

            UpdateUI();

            contentCanvasGroup.DOFade(1f, fadeDuration).SetUpdate(true).OnComplete(() =>
            {
                isFading = false;
                RestartHintTimer();
            });
        });
    }

    private void UpdateUI()
    {
        if (TotalInstructions == 0) return;

        InstructionStep currentStep;

        if (currentIndex < topDownInstructions.Length)
        {
            modeTitleText.text = topDownTitle;
            currentStep = topDownInstructions[currentIndex];
        }
        else
        {
            modeTitleText.text = fishingTitle;
            int fishingIndex = currentIndex - topDownInstructions.Length;
            currentStep = fishingInstructions[fishingIndex];
        }

        if (displayImage != null) displayImage.sprite = currentStep.instructionImage;
        if (displayDescription != null) displayDescription.text = currentStep.instructionText;
    }

    private void RestartHintTimer()
    {
        if (hintText != null)
        {
            hintText.DOKill();
            hintText.alpha = 0f;
            hintText.DOFade(1f, 0.5f).SetDelay(hintDelay).SetUpdate(true);
        }
    }

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
}