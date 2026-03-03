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
        isDamagable = true;
    }

    public void TakeDamage(DamageData d_Data)
    {
        if (!isDamagable) return;

        float baseAmount = d_Data.amount;
        float finalDamage = (data != null)
            ? data.CalculateDamageTaken(baseAmount, d_Data.damageType)
            : baseAmount;

        currentHealth -= finalDamage;

        if (DamagePopupManager.Instance != null)
            DamagePopupManager.Instance.ShowDamage(finalDamage, transform.position);

        Debug.Log($"{gameObject.name} took {finalDamage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}