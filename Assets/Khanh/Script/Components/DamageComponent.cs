using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : UnitComponent
{
    [SerializeField] private float damage;
    [SerializeField] private DamageType damageType;

    protected override void OnInitialize()
    {
        damage = data.damage;
        damageType = data.damageType;
    }

    protected override void OnBoatSetUp()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning("[DamageComponent] UpgradeManager.Instance is null — keeping serialized damage value.");
            return;
        }

        float computed = UpgradeManager.Instance.ComputeStat(UpgradeType.Attack);
        if (computed > 0f)
            damage = computed;
        else
            Debug.LogWarning("[DamageComponent] UpgradeManager returned 0 for Attack — keeping serialized damage value.");
    }

    /// <summary>
    /// Re-reads Attack from UpgradeManager (which includes equipment bonuses).
    /// Called by EquipmentComponent whenever equipment changes at runtime.
    /// </summary>
    public void Refresh()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning("[DamageComponent] Refresh() skipped — UpgradeManager.Instance is null.");
            return;
        }

        float computed = UpgradeManager.Instance.ComputeStat(UpgradeType.Attack);
        if (computed > 0f)
        {
            damage = computed;
            Debug.Log($"[DamageComponent] Refreshed damage to {damage} after equipment change.");
        }
    }

    public void TryDealDamage(GameObject target)
    {
        if (target.TryGetComponent<HealthComponent>(out HealthComponent health))
        {
            if (health.isDamagable)
            {
                DamageData damageData = new DamageData(damage, damageType, this.gameObject);
                health.TakeDamage(damageData);
            }
        }
    }
}