public enum AugmentEffect
{
    // ── Immediate stat bonuses (via UpgradeManager.ApplyAugmentBonus) ─────────
    BonusDefense,           // Cloth Armour, Chainmail Vest
    BonusAttack,
    BonusHealth,
    BonusMana,
    BonusStamina,
    BonusLuck,

    // ── Immediate economy ─────────────────────────────────────────────────────
    GainGoldFlat,           // The One Piece is real?!
    GainGoldRandom,         // Who dropped the wallet? → Random(1, 18)

    // ── Immediate EXP ─────────────────────────────────────────────────────────
    GainExp,                // Happy Birthday, Graduation, Mature

    // ── Immediate heals / boat stats ──────────────────────────────────────────
    HealBoatHP,             // Repair kit, Bandage
    HealStamina,            // Sparkling Water on sales
    BoostBoatSpeed,         // MotorBoat
    HookDurability,         // My salesman is legit
    RodAttackMultiplier,    // Enchantments?

    // ── Multi-stat (use value + value2) ───────────────────────────────────────
    BoatLoadAndHP,          // Are we a Tank or a Truck?! — value=load, value2=HP
    DefenseAndAttack,       // Full Armour — value=defense, value2=attack
    AllStatsPercent,        // I AM GOD — value=percent (10)

    // ── Trigger / passive (registered in AugmentTriggerManager) ──────────────
    TriggerCriticalStrike,  // First hit = 15% of fish max HP
    TriggerAscended,        // If HP drops ≤ 10, heal +20 (single use)
    TriggerMiniDoctor,      // On next panel close, heal missing HP (single use)
    TriggerAdrenalineRush,  // When stamina hits 0, restore to 30% (single use)
    TriggerWheresMyTea,     // +20 stamina / max stamina for 60s, then revert
    TriggerShadyBusiness,   // Remove 25% fish from inventory → heal to full HP
    TriggerSkyborn,         // Grant random fish items on next catch
}
