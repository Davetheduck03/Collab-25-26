using UnityEngine;

[CreateAssetMenu(fileName = "New Zone", menuName = "FishyWishy/Zone")]
public class ZoneSO : ScriptableObject
{
    public string zoneName;

    [Header("Horizontal Distance From Spawn (feet)")]
    public float minDis;
    public float maxDis;

    [Header("Rarity Spawn Weights (must add up to 100)")]
    public float commonWeight   = 70f;
    public float uncommonWeight = 20f;
    public float rareWeight     = 7f;
    public float epicWeight     = 3f;
    public float mythicWeight   = 0f;

    [Header("Multipliers")]
    public float currencyMultiplier = 1f;
    public float expMultiplier      = 1f;
    public float zoneMultiplier     = 1f; // used in: Fish Stats = Base * Biome * Zone * 1.1

    public Rarity RollRarity()
    {
        float total = commonWeight + uncommonWeight + rareWeight + epicWeight + mythicWeight;
        float roll  = Random.Range(0f, total);

        if (roll < commonWeight)                                          return Rarity.Common;
        if (roll < commonWeight + uncommonWeight)                         return Rarity.Uncommon;
        if (roll < commonWeight + uncommonWeight + rareWeight)            return Rarity.Rare;
        if (roll < commonWeight + uncommonWeight + rareWeight + epicWeight) return Rarity.Epic;
        return Rarity.Mythic;
    }
}
