using UnityEngine;
using UnityEngine.UI; // Required for the Slider

public class BoatHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the parent GameObject of this entire ticket/card here.")]
    public GameObject healthPanel;

    [Tooltip("Drag the UI Slider that represents the red health bar here.")]
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
        boatHealth = GetComponent<HealthComponent>();
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

    // Notice we include the bool and EnemySO to match your event signature!
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
        if (boatHealth != null && healthSlider != null)
        {
            // Automatically adjust the slider to match the boat's max and current HP
                healthSlider.fillAmount = boatHealth.maxHealth;
                healthSlider.fillAmount = boatHealth.currentHealth;
        }
    }
}