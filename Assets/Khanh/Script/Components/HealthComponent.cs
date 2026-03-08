using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : UnitComponent
{
    public float currentHealth;
    public bool isDamagable;

    // Subscribe to this to handle death without destroying the GameObject (e.g. the Boat)
    public event Action OnDeath;

    // Static event — fires on ANY HealthComponent after damage is applied.
    // Used by AugmentTriggerManager (I AM THE ASCENDED, Adrenaline Rush, etc.)
    public static event Action<HealthComponent> OnDamageTaken;

    protected override void OnInitialize()
    {
        currentHealth = data.health;
        isDamagable = true;
    }

    protected override void OnBoatSetUp()
    {
        const float FALLBACK_HEALTH = 100f;

        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning($"[HealthComponent] UpgradeManager.Instance is null — using fallback health: {FALLBACK_HEALTH}.");
            currentHealth = FALLBACK_HEALTH;
            isDamagable = true;
            return;
        }

        float computed = UpgradeManager.Instance.ComputeStat(UpgradeType.Health);
        if (computed > 0f)
        {
            currentHealth = computed;
        }
        else
        {
            Debug.LogWarning($"[HealthComponent] UpgradeManager returned 0 for Health — using fallback: {FALLBACK_HEALTH}. Check your PlayerStats asset has a Health entry.");
            currentHealth = FALLBACK_HEALTH;
        }

        isDamagable = true;
    }

    public void TakeDamage(DamageData d_Data)
    {
        if (!isDamagable) return;

        float baseAmount = d_Data.amount;
        float finalDamage = (data != null)
            ? data.CalculateDamageTaken(baseAmount, d_Data.damageType)
            : baseAmount;

        // Diagnostic: log health BEFORE subtraction so we can see the true value
        Debug.Log($"[HealthComponent] {gameObject.name} — HP before damage: {currentHealth} | incoming: {finalDamage} | data null: {data == null}");

        // Bug fix: clamp health at 0 so it never goes negative
        currentHealth = Mathf.Max(0f, currentHealth - finalDamage);

        // Notify augment triggers (e.g. I AM THE ASCENDED)
        OnDamageTaken?.Invoke(this);

        if (DamagePopupManager.Instance != null)
            DamagePopupManager.Instance.ShowDamage(finalDamage, transform.position);

        string dealer = (d_Data.damageDealer != null) ? d_Data.damageDealer.name : "Unknown";
        Debug.Log($"[{dealer}] dealt {finalDamage} damage to [{gameObject.name}]. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (OnDeath != null)
        {
            // A listener is registered (e.g. the Boat's game-over handler) — let it decide what happens
            Debug.Log($"[HealthComponent] {gameObject.name} died — invoking OnDeath event.");
            OnDeath.Invoke();
        }
        else
        {
            // No listener — safe to destroy (e.g. fish, enemies)
            Debug.Log($"[HealthComponent] {gameObject.name} died — no OnDeath listener, destroying.");
            Destroy(gameObject);
        }
    }
}