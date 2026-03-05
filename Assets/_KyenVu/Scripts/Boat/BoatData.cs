using UnityEngine;
using System.Collections.Generic;

public class BoatData : MonoBehaviour
{
    public List<UnitComponent> components;

    private HealthComponent healthComponent;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log($"[BoatData] Initialize() started on '{gameObject.name}'. UpgradeManager present: {UpgradeManager.Instance != null}");

        GetComponents(components);
        Debug.Log($"[BoatData] Found {components.Count} UnitComponent(s): {string.Join(", ", components.ConvertAll(c => c.GetType().Name))}");

        foreach (var component in components)
        {
            Debug.Log($"[BoatData] Calling BoatSetUp on {component.GetType().Name}...");
            component.BoatSetUp();
            Debug.Log($"[BoatData] BoatSetUp finished for {component.GetType().Name}");
        }

        // Cache and subscribe to the health component's death event
        healthComponent = GetComponent<HealthComponent>();
        if (healthComponent != null)
        {
            healthComponent.OnDeath += HandleBoatDeath;
            Debug.Log($"[BoatData] Boat initialized. Starting HP: {healthComponent.currentHealth} | OnDeath subscribed: true");
        }
        else
        {
            Debug.LogWarning("[BoatData] No HealthComponent found on Boat.");
        }
    }

    private void HandleBoatDeath()
    {
        // Stop taking further damage
        healthComponent.isDamagable = false;
        Debug.Log("[BoatData] Boat has been destroyed! GAME OVER.");

        // TODO: trigger your actual game-over / respawn logic here
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (healthComponent != null)
            healthComponent.OnDeath -= HandleBoatDeath;
    }
}