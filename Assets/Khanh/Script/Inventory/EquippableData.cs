using UnityEngine;

[CreateAssetMenu(fileName = "EquippableData", menuName = "FishyWishy/ItemData/EquippableData")]
public class EquippableData : ItemData
{
    public string runtimeUID;

    public virtual void OnEquip()
    {

    }
}
