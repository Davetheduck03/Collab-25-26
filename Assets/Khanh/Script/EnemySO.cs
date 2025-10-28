using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "FishyWishy/New Enemy Stat")]

public class EnemySO : UnitSO
{
    [Header("Enemy Specific Info")]
    public float experienceReward;
    public Rarity rarity;
    [SerializeField]private int minPrice;
    [SerializeField]private int maxPrice;
    public int price;

    private void OnEnable()
    {
        SetPrice();
    }

    public void SetPrice()
    {
        price = UnityEngine.Random.Range(minPrice, maxPrice);
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
