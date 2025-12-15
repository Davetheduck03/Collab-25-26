using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : UnitComponent
{
    public float currentHealth;
    public bool isDamagable;

    protected override void OnInitialize()
    {
        currentHealth = data.health;
        isDamagable = true;
    }

    protected override void OnBoatSetUp()
    {
        currentHealth = UpgradeManager.Instance.ComputeStat(UpgradeType.Health);
    }

    public void TakeDamage(DamageData d_Data)
    {
        if (!isDamagable) return;

        float baseAmount = d_Data.amount;
        float finalDamage = this.data.CalculateDamageTaken(baseAmount, d_Data.damageType);
        currentHealth -= finalDamage;

        // --- NEW: Trigger Popup ---
        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(finalDamage, transform.position);
        }

        Debug.Log($"{gameObject.name} took {finalDamage} {data.damageType} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}