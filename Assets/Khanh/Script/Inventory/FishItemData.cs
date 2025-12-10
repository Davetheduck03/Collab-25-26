using UnityEngine;

[CreateAssetMenu(fileName = "FishItemData", menuName = "FishyWishy/ItemData/FishItemData")]
public class FishItemData : ItemData
{
    public EnemySO fishData;

    public int price;

    public override void Sell()
    {
        base.Sell();

    }
}
