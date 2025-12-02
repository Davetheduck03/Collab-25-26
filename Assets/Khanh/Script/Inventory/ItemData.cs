using UnityEngine;

public enum ItemType
{
    Consumable,
    Equippable,
    Material,
    Quest,
    Fish,
}

public class ItemData : ScriptableObject
{
    public int ID;
    public string displayName;
    public string description;
    public Sprite Sprite;

    public ItemType type;
    public bool isStackable;
    public int maxStack;
}
