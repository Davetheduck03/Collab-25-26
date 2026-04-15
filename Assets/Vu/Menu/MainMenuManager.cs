using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Drag your Main Menu Buttons here. They will hide when a panel opens!")]
    public GameObject buttonLayout;

    [Header("Menu Panels")]
    public GameObject instructionMenu;
    public GameObject creditsMenu;
    public GameObject quitMenu; // <--- NEW: Quit confirmation panel

    [Header("Scene Transition")]
    public CanvasGroup fadeTransition;
    public float fadeDuration = 0.6f;

    [Header("Pop-up Animation Settings")]
    public float popUpDuration = 0.35f;
    public float popDownDuration = 0.25f;

    private void Start()
    {
        SetupStartState();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscape();
        }

        // Click outside to close logic
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (instructionMenu.activeSelf && !IsMouseOverRectTransform(instructionMenu, mousePos))
            {
                BackToMain();
            }
            else if (creditsMenu.activeSelf && !IsMouseOverRectTransform(creditsMenu, mousePos))
            {
                BackToMain();
            }
            // --- NEW: Click outside to close Quit Panel ---
            else if (quitMenu.activeSelf && !IsMouseOverRectTransform(quitMenu, mousePos))
            {
                BackToMain();
            }
        }
    }

    private void HandleEscape()
    {
        // --- NEW: Pressing ESC also closes the Quit Panel ---
        if (instructionMenu.activeSelf || creditsMenu.activeSelf || quitMenu.activeSelf)
        {
            BackToMain();
        }
    }

    private void SetupStartState()
    {
        if (fadeTransition != null)
        {
            fadeTransition.alpha = 1f;
            fadeTransition.DOFade(0, fadeDuration);
        }

        if (buttonLayout != null) buttonLayout.SetActive(true);

        HidePanel(instructionMenu, instantly: true);
        HidePanel(creditsMenu, instantly: true);
        HidePanel(quitMenu, instantly: true); // --- NEW: Hide quit menu on start
    }

    //==============================
    // OVERLAY SWITCH FUNCTIONS
    //==============================
    public void OpenSettings()
    {
        if (buttonLayout != null) buttonLayout.SetActive(false);
        ShowPanel(instructionMenu);
    }

    public void OpenCredits()
    {
        if (buttonLayout != null) buttonLayout.SetActive(false);
        ShowPanel(creditsMenu);
    }

    // --- NEW: Function to open the Quit Confirmation Panel ---
    public void OpenQuitMenu()
    {
        if (buttonLayout != null) buttonLayout.SetActive(false);
        ShowPanel(quitMenu);
    }

    public void BackToMain()
    {
        if (instructionMenu.activeSelf) HidePanel(instructionMenu);
        if (creditsMenu.activeSelf) HidePanel(creditsMenu);
        if (quitMenu.activeSelf) HidePanel(quitMenu); // --- NEW: Close quit menu

        // The buttons will restore automatically after the pop-down animation finishes!
    }

    //==============================
    // SCENE LOAD & QUIT
    //==============================
    public void PlayGame(string sceneName)
    {
        if (fadeTransition != null)
        {
            fadeTransition.gameObject.SetActive(true);
            fadeTransition.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    // --- CHANGED: This is now the "V" Button function! ---
    public void ConfirmQuit()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    //==============================
    // UI ANIMATION HELPERS (POP-UP)
    //==============================
    private void ShowPanel(GameObject panel, bool instantly = false)
    {
        if (panel == null) return;

        panel.transform.DOKill();
        panel.SetActive(true);

        if (instantly)
        {
            panel.transform.localScale = Vector3.one;
            return;
        }

        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, popUpDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void HidePanel(GameObject panel, bool instantly = false)
    {
        if (panel == null) return;

        panel.transform.DOKill();

        if (instantly)
        {
            panel.transform.localScale = Vector3.zero;
            panel.SetActive(false);
            CheckAndRestoreButtons();
            return;
        }

        panel.transform.DOScale(Vector3.zero, popDownDuration)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                panel.SetActive(false);
                // Restored this so your buttons wait for the animation to finish!
                CheckAndRestoreButtons();
            });
    }

    private void CheckAndRestoreButtons()
    {
        // --- NEW: Added quitMenu check to ensure ALL panels are closed ---
        if (!instructionMenu.activeSelf && !creditsMenu.activeSelf && !quitMenu.activeSelf)
        {
            if (buttonLayout != null) buttonLayout.SetActive(true);
        }
    }

    // Helper for checking clicks outside
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