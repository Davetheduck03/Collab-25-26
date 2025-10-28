using UnityEngine;

public class RewardComponent : UnitComponent
{
    public float expReward;
    public int price;
    public Rarity rarity;

    protected override void OnInitialize()
    {
        if (data is EnemySO enemyData)
        {
            expReward = enemyData.experienceReward;
            price = enemyData.price;
            rarity = enemyData.rarity;
        }
    }

}
