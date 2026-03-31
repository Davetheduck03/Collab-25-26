using System;
using UnityEngine;

/// <summary>
/// Singleton that tracks which Hook, Rod, and Boat the player has equipped.
/// Provides stat bonuses to UpgradeManager.ComputeStat() and exposes
/// fishing-specific values (hook chance, reel speed, boat speed, capacity).
///
/// Attach this MonoBehaviour to a persistent GameObject in your scene.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    // ── Currently equipped items ───────────────────────────────────────────
    private HookItemData  equippedHook;
    private RodItemData   equippedRod;
    private BoatItemData  equippedBoat;

    /// <summary>Fired whenever any equipment slot changes.</summary>
    public static event Action OnEquipmentChanged;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ── Equip Methods ──────────────────────────────────────────────────────

    public void EquipHook(HookItemData hook)
    {
        equippedHook = hook;
        Debug.Log($"[Equipment] Equipped hook: {hook.displayName}  " +
                  $"(HookChance={hook.hookChance}%  RareBoost={hook.rareBoost})");
        BroadcastChange();
    }

    public void EquipRod(RodItemData rod)
    {
        equippedRod = rod;
        Debug.Log($"[Equipment] Equipped rod: {rod.displayName}  " +
                  $"(AttackMult={rod.attackMult}x  ReelSpeed={rod.reelSpeed}x)");
        BroadcastChange();
    }

    public void EquipBoat(BoatItemData boat)
    {
        equippedBoat = boat;
        Debug.Log($"[Equipment] Equipped boat: {boat.displayName}  " +
                  $"(Speed={boat.speed}  Capacity={boat.capacity}  HP={boat.hp})");
        BroadcastChange();
    }

    private void BroadcastChange()
    {
        OnEquipmentChanged?.Invoke();
        // Tell UpgradeManager to re-broadcast stat change events so any
        // stat display panels refresh automatically.
        UpgradeManager.Instance?.NotifyEquipmentChanged();
    }

    // ── Equipped Item Getters ──────────────────────────────────────────────

    public HookItemData  GetEquippedHook()  => equippedHook;
    public RodItemData   GetEquippedRod()   => equippedRod;
    public BoatItemData  GetEquippedBoat()  => equippedBoat;

    // ── Stat Bonus Getters (used by UpgradeManager.ComputeStat) ───────────

    /// <summary>
    /// Returns the flat additive bonus to apply to a player stat from equipment.
    /// Currently:
    ///   Luck   += Hook.rareBoost
    ///   Health += Boat.hp
    /// </summary>
    public float GetStatBonus(UpgradeType type)
    {
        float bonus = 0f;
        switch (type)
        {
            case UpgradeType.Luck:
                if (equippedHook != null) bonus += equippedHook.rareBoost;
                break;
            case UpgradeType.Health:
                if (equippedBoat != null) bonus += equippedBoat.hp;
                break;
        }
        return bonus;
    }

    /// <summary>
    /// Multiplicative attack bonus from the equipped rod (1.0 if none equipped).
    /// E.g. Iron Rod returns 1.3 → Attack stat is multiplied by 1.3.
    /// </summary>
    public float GetAttackMultiplier()
        => equippedRod != null ? equippedRod.attackMult : 1f;

    // ── Fishing / Movement Stat Accessors ─────────────────────────────────

    /// <summary>Hook chance percentage (0-100). Default 70 if no hook equipped.</summary>
    public float GetHookChance()
        => equippedHook != null ? equippedHook.hookChance : 70f;

    /// <summary>Reel speed multiplier. Default 1.0 if no rod equipped.</summary>
    public float GetReelSpeed()
        => equippedRod != null ? equippedRod.reelSpeed : 1f;

    /// <summary>Boat movement speed. Default 3 if no boat equipped.</summary>
    public float GetBoatSpeed()
        => equippedBoat != null ? equippedBoat.speed : 3f;

    /// <summary>Inventory / cargo capacity. Default 8 if no boat equipped.</summary>
    public int GetInventoryCapacity()
        => equippedBoat != null ? equippedBoat.capacity : 8;
}
