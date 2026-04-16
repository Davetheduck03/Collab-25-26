using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Canvas))]
public class PersistentCanvasCamera : MonoBehaviour
{
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        // Listen for when a new scene finishes loading
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the new camera in the scene (Make sure it's tagged as "MainCamera"!)
        Camera newCamera = Camera.main;

        if (newCamera != null)
        {
            // Re-assign the new camera to this Canvas
            canvas.worldCamera = newCamera;
        }
        else
        {
            Debug.LogWarning("[PersistentCanvasCamera] No camera tagged 'MainCamera' was found in the new scene!");
        }
    }
}