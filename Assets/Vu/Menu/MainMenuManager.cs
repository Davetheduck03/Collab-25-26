using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public CanvasGroup mainMenu;
    public CanvasGroup settingsMenu;
    public CanvasGroup creditsMenu;

    [Header("Transition")]
    public CanvasGroup fadeTransition;
    public float fadeDuration = 0.6f;

    private void Start()
    {
        SetupStartState();
    }

    private void SetupStartState()
    {
        fadeTransition.alpha = 1f;
        fadeTransition.DOFade(0, fadeDuration);

        ShowPanel(mainMenu);
        HidePanel(settingsMenu, instantly: true);
        HidePanel(creditsMenu, instantly: true);
    }

    //==============================
    // PANEL SWITCH FUNCTIONS
    //==============================
    public void OpenSettings()
    {
        SwitchPanel(mainMenu, settingsMenu);
    }

    public void OpenCredits()
    {
        SwitchPanel(mainMenu, creditsMenu);
    }

    public void BackToMain()
    {
        SwitchPanel(settingsMenu, mainMenu);
        SwitchPanel(creditsMenu, mainMenu);
    }

    //==============================
    // SCENE LOAD WITH TRANSITION
    //==============================
    public void PlayGame(string sceneName)
    {
        fadeTransition.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    //==============================
    // UI ANIMATION HELPERS
    //==============================
    private void SwitchPanel(CanvasGroup from, CanvasGroup to)
    {
        HidePanel(from);
        ShowPanel(to);
    }

    private void ShowPanel(CanvasGroup panel, bool instantly = false)
    {
        panel.blocksRaycasts = true;
        panel.interactable = true;

        if (instantly)
        {
            panel.alpha = 1;
            return;
        }

        panel.DOFade(1f, 0.35f);
        panel.transform.DOScale(1.05f, 0.35f).From().SetEase(Ease.OutBack);
    }

    private void HidePanel(CanvasGroup panel, bool instantly = false)
    {
        panel.blocksRaycasts = false;
        panel.interactable = false;

        if (instantly)
        {
            panel.alpha = 0;
            return;
        }

        panel.DOFade(0f, 0.2f);
    }
}
