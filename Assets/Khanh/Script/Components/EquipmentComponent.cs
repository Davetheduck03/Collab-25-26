using UnityEngine;

/// <summary>
/// UnitComponent that lives on the Boat and keeps its stats in sync with
/// whatever the player has equipped.  It sits alongside HealthComponent and
/// DamageComponent in BoatData's component list and is initialised by the
/// same BoatSetUp() pass.
///
/// When the player equips a new Hook, Rod, or Boat item, EquipmentManager
/// fires OnEquipmentChanged. This component catches that event and tells
/// HealthComponent / DamageComponent to re-read from UpgradeManager, which
/// already folds equipment bonuses into its ComputeStat() output.
/// </summary>
public class EquipmentComponent : UnitComponent
{
    private HealthComponent  healthComponent;
    private DamageComponent  damageComponent;

    // ── UnitComponent lifecycle ────────────────────────────────────────────

    protected override void OnBoatSetUp()
    {
        healthComponent = GetComponent<HealthComponent>();
        damageComponent = GetComponent<DamageComponent>();

        if (healthComponent == null)
            Debug.LogWarning("[EquipmentComponent] No HealthComponent found on Boat.");
        if (damageComponent == null)
            Debug.LogWarning("[EquipmentComponent] No DamageComponent found on Boat.");

        EquipmentManager.OnEquipmentChanged += RefreshStats;
        Debug.Log("[EquipmentComponent] Initialised — listening for equipment changes.");
    }

    // ── Equipment change handler ───────────────────────────────────────────

    private void RefreshStats()
    {
        Debug.Log("[EquipmentComponent] Equipment changed — refreshing boat stats.");
        damageComponent?.Refresh();
        healthComponent?.RefreshMaxHealth();
    }

    // ── Cleanup ────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        EquipmentManager.OnEquipmentChanged -= RefreshStats;
    }
}
