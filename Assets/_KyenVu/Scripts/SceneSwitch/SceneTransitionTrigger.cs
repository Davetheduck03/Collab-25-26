using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drop this on any trigger zone (e.g., the dock, a door) in either the
/// Top Down scene or the 2D Fishing scene.
///
/// Requirements on the same GameObject:
///   • A Collider2D with "Is Trigger" = true.
///   • Tag the player GameObject "Player".
///
/// Inspector:
///   • sceneToLoad  — exact name of the scene to load (must be in Build Settings).
///   • promptKey    — keyboard key the player presses to confirm travel (default: E).
///   • showPrompt   — optional UI GameObject to activate while player is inside the zone.
///   • requireKeyPress — if true the player must press the key; if false, loading is automatic.
/// </summary>
public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Destination")]
    [Tooltip("Exact scene name as it appears in File > Build Settings")]
    public string sceneToLoad;

    [Header("Interaction")]
    [Tooltip("If true the player presses a key to confirm; if false, transition is automatic on enter.")]
    public bool requireKeyPress = true;
    public KeyCode promptKey = KeyCode.E;

    [Header("Optional UI")]
    [Tooltip("A small 'Press E to travel' prompt GameObject. Leave empty if you handle it elsewhere.")]
    public GameObject travelPrompt;

    private bool playerInside = false;

    // ── Trigger events ─────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Boat")) return;

        playerInside = true;

        if (travelPrompt != null)
            travelPrompt.SetActive(true);

        if (!requireKeyPress)
            LoadScene();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        if (travelPrompt != null)
            travelPrompt.SetActive(false);
    }

    // ── Key press ──────────────────────────────────────────────

    private void Update()
    {
        if (requireKeyPress && playerInside && Input.GetKeyDown(promptKey))
            LoadScene();
    }

    // ── Scene loading ──────────────────────────────────────────

    private void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogWarning("[SceneTransitionTrigger] sceneToLoad is empty — set it in the Inspector!");
            return;
        }

        Debug.Log($"[SceneTransitionTrigger] Loading scene: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }
}
