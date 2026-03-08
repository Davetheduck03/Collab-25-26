using UnityEngine;

/// <summary>
/// Static helper — call Apply(augment) when a card is picked.
/// Handles all immediate effects; delegates trigger/passive effects to AugmentTriggerManager.
/// </summary>
public static class AugmentApplier
{
    public static void Apply(AugmentSO augment)
    {
        switch (augment.effect)
        {
            // ── Stat bonuses via UpgradeManager ──────────────────────────────
            case AugmentEffect.BonusDefense:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Defense, augment.value);
                break;

            case AugmentEffect.BonusAttack:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Attack, augment.value);
                break;

            case AugmentEffect.BonusHealth:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Health, augment.value);
                break;

            case AugmentEffect.BonusMana:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Mana, augment.value);
                break;

            case AugmentEffect.BonusStamina:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Stamina, augment.value);
                break;

            case AugmentEffect.BonusLuck:
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Luck, augment.value);
                break;

            // ── Economy ───────────────────────────────────────────────────────
            case AugmentEffect.GainGoldFlat:
                CurrencyManager.Instance?.AddCurrency((int)augment.value);
                break;

            case AugmentEffect.GainGoldRandom:
                CurrencyManager.Instance?.AddCurrency(Random.Range(1, 19)); // 1 ~ 18
                break;

            // ── EXP ───────────────────────────────────────────────────────────
            case AugmentEffect.GainExp:
                ExperienceManager.Instance?.AddExperience(augment.value);
                break;

            // ── Boat heals ────────────────────────────────────────────────────
            case AugmentEffect.HealBoatHP:
                HealBoat(augment.value);
                break;

            case AugmentEffect.HealStamina:
                // TODO: wire to a runtime StaminaManager when implemented
                Debug.Log($"[AugmentApplier] HealStamina +{augment.value} — stamina runtime not yet implemented.");
                break;

            // ── Boat stats ────────────────────────────────────────────────────
            case AugmentEffect.BoostBoatSpeed:
                ModifyBoatSpeed(augment.value);
                break;

            case AugmentEffect.BoatLoadAndHP:
                // value = load bonus (maxLineLength), value2 = HP bonus
                ModifyBoatLoad(augment.value);
                HealBoat(augment.value2);
                break;

            case AugmentEffect.HookDurability:
                // TODO: wire to a hook durability field when implemented
                Debug.Log($"[AugmentApplier] HookDurability +{augment.value} — not yet implemented.");
                break;

            case AugmentEffect.RodAttackMultiplier:
                // TODO: wire to rod attack multiplier when implemented
                Debug.Log($"[AugmentApplier] RodAttackMultiplier +{augment.value} — not yet implemented.");
                break;

            // ── Multi-stat ────────────────────────────────────────────────────
            case AugmentEffect.DefenseAndAttack:
                // value = defense bonus, value2 = attack bonus
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Defense, augment.value);
                UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Attack, augment.value2);
                break;

            case AugmentEffect.AllStatsPercent:
                ApplyAllStatsPercent(augment.value);
                break;

            // ── Trigger / passive: hand off to AugmentTriggerManager ──────────
            case AugmentEffect.TriggerCriticalStrike:
            case AugmentEffect.TriggerAscended:
            case AugmentEffect.TriggerMiniDoctor:
            case AugmentEffect.TriggerAdrenalineRush:
            case AugmentEffect.TriggerWheresMyTea:
            case AugmentEffect.TriggerShadyBusiness:
            case AugmentEffect.TriggerSkyborn:
                if (AugmentTriggerManager.Instance != null)
                    AugmentTriggerManager.Instance.RegisterTrigger(augment);
                else
                    Debug.LogWarning("[AugmentApplier] AugmentTriggerManager not found in scene.");
                break;

            default:
                Debug.LogWarning($"[AugmentApplier] Unhandled effect: {augment.effect}");
                break;
        }

        Debug.Log($"[AugmentApplier] Applied: {augment.displayName}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void HealBoat(float amount)
    {
        var hp = FindBoatHealthComponent();
        if (hp == null) { Debug.LogWarning("[AugmentApplier] Boat HealthComponent not found."); return; }

        float maxHP = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.ComputeStat(UpgradeType.Health)
            : hp.currentHealth;

        hp.currentHealth = Mathf.Min(hp.currentHealth + amount, maxHP);
        Debug.Log($"[AugmentApplier] Boat healed +{amount}. HP now: {hp.currentHealth}");
    }

    private static void ModifyBoatSpeed(float amount)
    {
        var boat = Object.FindFirstObjectByType<BoatController>();
        if (boat == null) { Debug.LogWarning("[AugmentApplier] BoatController not found."); return; }
        boat.moveSpeed += amount;
        Debug.Log($"[AugmentApplier] Boat speed +{amount}. Now: {boat.moveSpeed}");
    }

    private static void ModifyBoatLoad(float amount)
    {
        var cast = Object.FindFirstObjectByType<CastLineControl>();
        if (cast == null) { Debug.LogWarning("[AugmentApplier] CastLineControl not found."); return; }
        cast.maxLineLength += amount;
        Debug.Log($"[AugmentApplier] Boat load +{amount}. maxLineLength now: {cast.maxLineLength}");
    }

    private static void ApplyAllStatsPercent(float percent)
    {
        if (UpgradeManager.Instance == null) return;
        foreach (UpgradeType t in System.Enum.GetValues(typeof(UpgradeType)))
        {
            float current = UpgradeManager.Instance.ComputeStat(t);
            UpgradeManager.Instance.ApplyAugmentBonus(t, current * (percent / 100f));
        }
        Debug.Log($"[AugmentApplier] I AM GOD — all stats +{percent}%.");
    }

    /// <summary>Public so AugmentTriggerManager can reuse it.</summary>
    public static HealthComponent FindBoatHealthComponent()
    {
        var boat = GameObject.FindWithTag("Boat") ?? GameObject.Find("Boat");
        return boat?.GetComponent<HealthComponent>();
    }
}
