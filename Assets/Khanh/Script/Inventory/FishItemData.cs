using UnityEngine;

[CreateAssetMenu(fileName = "FishItemData", menuName = "FishyWishy/ItemData/FishItemData")]
public class FishItemData : ItemData
{
    public EnemySO fishData;

    public int price;

    public virtual void OnSell()
    {
        // Custom behavior when the fish item is sold
        Debug.Log($"Sold fish item");
    }
}
