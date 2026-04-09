using UnityEngine;
using UnityEngine.SceneManagement;

// We remove the interface because InteractableObject handles the interaction now.
public class ScreenSwitcher : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of the scene you want to load")]
    public string sceneToLoad = "2D scene";
    public Animator sceneTransitionAnimator;

    // This is the public method we will trigger from the UnityEvent
    public void SwitchScene()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsDockOpen())
        {
            NotificationManager.Instance.ShowNotification("The dock is closed, please come back at 7:00 AM.");
            return; // Stop the code, don't let them on the boat!
        }
        if (sceneTransitionAnimator != null)
        {
            sceneTransitionAnimator.SetTrigger("FadeOut");
        }
        SceneManager.LoadScene(sceneToLoad);
    }
}