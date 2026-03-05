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