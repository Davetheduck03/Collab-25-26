using UnityEngine;
using UnityEngine.SceneManagement;

// We remove the interface because InteractableObject handles the interaction now.
public class ScreenSwitcher : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of the scene you want to load")]
    public string sceneToLoad = "2D scene";

    // This is the public method we will trigger from the UnityEvent
    public void SwitchScene()
    {
        Debug.Log($"Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}