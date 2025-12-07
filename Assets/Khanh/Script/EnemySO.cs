using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "FishyWishy/New Enemy Stat")]

public class EnemySO : UnitSO
{
    [Header("Enemy Specific Info")]
    public float experienceReward;
    public Rarity rarity;

    [Header("Price Range")]
    [SerializeField] private int minPrice;
    [SerializeField] private int maxPrice;

    [Header("Item Drop")]
    public FishItemData itemData;

    public int GeneratePrice()
    {
        return Random.Range(minPrice, maxPrice + 1);
    }
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}
