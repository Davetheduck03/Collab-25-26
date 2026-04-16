using UnityEngine;

/// <summary>
/// Add this to the root GameObject of your Quota HUD (the one with QuotaUI on it,
/// or its parent canvas).  It calls DontDestroyOnLoad so the bar stays visible in
/// every scene, then destroys any duplicate that spawns when returning to the
/// top-down scene.
/// </summary>
public class QuotaHUDPersist : MonoBehaviour
{
    private static QuotaHUDPersist _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        transform.SetParent(null);   // detach from any scene canvas before persisting
        DontDestroyOnLoad(gameObject);
    }
}
