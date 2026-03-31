using UnityEngine;

/// <summary>
/// ScriptableObject data for Hook equipment items.
/// Stats: hookChance (%), rareBoost, durability, cost.
///   - rareBoost  → adds to the player's Luck stat
///   - hookChance → accessible via EquipmentManager.GetHookChance()
/// </summary>
[CreateAssetMenu(fileName = "HookItemData", menuName = "FishyWishy/ItemData/HookItemData")]
public class HookItemData : EquippableData
{
    [Header("Hook Stats")]
    public int    tier;
    [Tooltip("Percentage chance (0-100) to hook a fish on bite")]
    public float  hookChance  = 70f;
    [Tooltip("Flat bonus added to the player's Luck stat")]
    public float  rareBoost   = 0f;
    public int    durability  = 50;
    public int    cost        = 0;

    public override void OnEquip()
    {
        EquipmentManager.Instance?.EquipHook(this);
    }
}
