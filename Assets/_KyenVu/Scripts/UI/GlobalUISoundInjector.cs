using UnityEngine;
using UnityEngine.UI; // Required to find Buttons
using UnityEngine.SceneManagement; // Required to know when a new scene loads
using Phuc.SoundSystem;

public class GlobalUISoundInjector : MonoBehaviour
{
    public static GlobalUISoundInjector Instance { get; private set; }

    [Header("Global Default Sounds")]
    [Tooltip("The sound every normal button should make when hovered")]
    public SO_SFXEvent defaultHoverSound;
    [Tooltip("The sound every normal button should make when clicked")]
    public SO_SFXEvent defaultClickSound;

    private void Awake()
    {
        // Singleton pattern to keep this alive forever
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null); // Ensure it's a root object
        DontDestroyOnLoad(gameObject);

        // Tell Unity to run our injection method every time a new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InjectSoundsIntoButtons();
    }

    [ContextMenu("Inject Sounds Now")]
    public void InjectSoundsIntoButtons()
    {
        // Find EVERY single button in the entire scene (even hidden ones!)
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int injectedCount = 0;

        foreach (Button btn in allButtons)
        {
            // Check if the button ALREADY has a sound trigger attached
            UISoundTrigger existingTrigger = btn.GetComponent<UISoundTrigger>();

            if (existingTrigger == null)
            {
                // If it doesn't have one, add it dynamically through code!
                UISoundTrigger newTrigger = btn.gameObject.AddComponent<UISoundTrigger>();

                // Assign the global default sounds
                newTrigger.hoverSound = defaultHoverSound;
                newTrigger.clickSound = defaultClickSound;

                injectedCount++;
            }
        }

        Debug.Log($"[GlobalUISoundInjector] Automatically injected sound triggers into {injectedCount} buttons!");
    }
}