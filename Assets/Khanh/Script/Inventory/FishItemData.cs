using UnityEngine;

[CreateAssetMenu(fileName = "FishItemData", menuName = "FishyWishy/ItemData/FishItemData")]
public class FishItemData : ItemData
{
    public float sellPrice;

    public virtual void OnSell()
    {

    }
}
