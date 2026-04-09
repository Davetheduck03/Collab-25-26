using UnityEngine;
using UnityEngine.UI;

public class BoatHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the parent GameObject of this entire ticket/card here.")]
    public GameObject healthPanel;

    [Tooltip("Drag the UI Image that represents the red health bar here. Make sure Image Type is set to Filled!")]
    public Image healthSlider;

    [Header("Boat Reference")]
    [Tooltip("Drag your Boat GameObject here so we can read its HealthComponent.")]
    public HealthComponent boatHealth;

    private void OnEnable()
    {
        // Subscribe to your existing fishing events
        BoatController.OnFishingStarted += ShowAndResetHealth;
        CastLineControl.OnFishingFinished += HideHealth;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks!
        BoatController.OnFishingStarted -= ShowAndResetHealth;
        CastLineControl.OnFishingFinished -= HideHealth;
    }

    private void Start()
    {
        // Ensure the health bar is hidden when walking around town
        if (healthPanel != null)
        {
            healthPanel.SetActive(false);
        }

        // DELETED: boatHealth = GetComponent<HealthComponent>();
        // We removed this so the script uses the Boat you dragged into the Inspector!
    }

    private void Update()
    {
        // Constantly sync the red bar with the boat's actual health 
        // ONLY if the panel is currently visible on screen.
        if (healthPanel != null && healthPanel.activeSelf)
        {
            SyncHealthBar();
        }
    }

    private void ShowAndResetHealth()
    {
        if (healthPanel != null) healthPanel.SetActive(true);

        if (boatHealth != null)
        {
            boatHealth.currentHealth = boatHealth.maxHealth;

            SyncHealthBar();
            Debug.Log("[BoatHealthUI] Fishing started! Boat health fully reset.");
        }
    }

    private void HideHealth(bool success, EnemySO caughtFishData)
    {
        if (healthPanel != null)
        {
            healthPanel.SetActive(false);
            Debug.Log("[BoatHealthUI] Fishing ended! Hiding health bar.");
        }
    }

    private void SyncHealthBar()
    {
        if (boatHealth != null && healthSlider != null && boatHealth.maxHealth > 0)
        {
            // CHANGED: Calculate the percentage (0.0 to 1.0)
            healthSlider.fillAmount = boatHealth.currentHealth / boatHealth.maxHealth;
        }
    }
}