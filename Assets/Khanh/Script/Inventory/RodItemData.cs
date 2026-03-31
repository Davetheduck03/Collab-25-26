using UnityEngine;

/// <summary>
/// ScriptableObject data for Fishing Rod equipment items.
/// Stats: attackMult (multiplier), reelSpeed, durability, cost.
///   - attackMult → multiplies the player's Attack stat (e.g. 1.3 = 30% bonus)
///   - reelSpeed  → accessible via EquipmentManager.GetReelSpeed()
/// </summary>
[CreateAssetMenu(fileName = "RodItemData", menuName = "FishyWishy/ItemData/RodItemData")]
public class RodItemData : EquippableData
{
    [Header("Rod Stats")]
    public int   tier;
    [Tooltip("Multiplier applied to the player's Attack stat (1.0 = no change)")]
    public float attackMult  = 1f;
    [Tooltip("Multiplier applied to reel speed during fishing (1.0 = no change)")]
    public float reelSpeed   = 1f;
    public int   durability  = 50;
    public int   cost        = 0;

    public override void OnEquip()
    {
        EquipmentManager.Instance?.EquipRod(this);
    }
}
