using UnityEngine;

/// <summary>
/// ScriptableObject data for Boat equipment items.
/// Stats: speed, capacity, hp, cost.
///   - hp       → adds to the player's Health stat
///   - speed    → accessible via EquipmentManager.GetBoatSpeed()
///   - capacity → accessible via EquipmentManager.GetInventoryCapacity()
/// </summary>
[CreateAssetMenu(fileName = "BoatItemData", menuName = "FishyWishy/ItemData/BoatItemData")]
public class BoatItemData : EquippableData
{
    [Header("Boat Stats")]
    public int   tier;
    [Tooltip("Movement speed of the boat")]
    public float speed      = 3f;
    [Tooltip("Number of inventory/cargo slots the boat provides")]
    public int   capacity   = 8;
    [Tooltip("Bonus added to the player's max Health stat")]
    public int   hp         = 100;
    public int   cost       = 0;

    public override void OnEquip()
    {
        EquipmentManager.Instance?.EquipBoat(this);
    }
}
