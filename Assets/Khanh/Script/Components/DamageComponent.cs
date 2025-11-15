using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : UnitComponent
{
    [SerializeField]private float damage;
    [SerializeField]private DamageType damageType;


    protected override void OnInitialize()
    {
        damage = data.damage;
        damageType = data.damageType;
    }

    protected override void OnBoatSetUp()
    {
        damage = UpgradeManager.Instance.ComputeStat(UpgradeType.Attack);
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
            else return;
        }
    }
}