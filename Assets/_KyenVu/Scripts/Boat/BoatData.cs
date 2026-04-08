using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatData : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("Drag an empty GameObject here to represent where the boat should teleport when it dies (e.g., the Dock).")]
    public Transform respawnPoint;

    [Tooltip("How long should the boat blink and be invincible after dying?")]
    public float invincibilityDuration = 2f;

    [HideInInspector]
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
            // CHANGED: Uncommented the subscription so the death event actually fires!
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
        Debug.Log("[BoatData] Boat has been destroyed! Respawning...");

        // 1. Temporarily stop taking damage
        if (healthComponent != null)
        {
            healthComponent.isDamagable = false;

            // Heal the boat back to full so it doesn't instantly die again
            healthComponent.currentHealth = healthComponent.maxHealth;

            // Update the UI if you have one
            if (DamagePopupManager.Instance != null)
            {
                // Optional: Show a "Respawned" text or just let the health bar visually refill
            }
        }

        // 2. Stop fishing and release the fish!
        // By calling FinishFishing(false), your CastLineControl automatically destroys the fish,
        // hides the line, and tells BoatController to give you movement control back.
        CastLineControl castLine = GetComponentInChildren<CastLineControl>();
        if (castLine != null)
        {
            castLine.FinishFishing(false);
        }

        // 3. Teleport back to the safe zone
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }
        else
        {
            Debug.LogWarning("[BoatData] No Respawn Point assigned! Teleporting to 0,0,0.");
            transform.position = Vector3.zero;
        }

        // 4. Start the blink effect and restore vulnerability
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        // Grab all the sprites on the boat (the hull, the character, etc.)
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();

        float blinkInterval = 0.15f;
        float timer = 0f;

        // Toggle the sprites on and off to create a blinking effect
        while (timer < invincibilityDuration)
        {
            foreach (var sr in renderers)
            {
                if (sr != null) sr.enabled = !sr.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        // Ensure all sprites are solidly visible at the end of the blink
        foreach (var sr in renderers)
        {
            if (sr != null) sr.enabled = true;
        }

        // Make the boat vulnerable to damage again
        if (healthComponent != null)
        {
            healthComponent.isDamagable = true;
            Debug.Log("[BoatData] Invincibility ended. Boat is damagable again.");
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (healthComponent != null)
            healthComponent.OnDeath -= HandleBoatDeath;
    }
}