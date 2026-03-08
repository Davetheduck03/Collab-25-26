using UnityEngine;

[CreateAssetMenu(fileName = "New Augment", menuName = "FishyWishy/Augment")]
public class AugmentSO : ScriptableObject
{
    [Header("Display")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("Rarity & Pool Weight")]
    public Rarity rarity;
    [Tooltip("Relative weight for random selection. Common=100, Uncommon=50, Rare=20, Epic=5, Mythic=1")]
    public int weight = 100;

    [Header("Effect")]
    public AugmentEffect effect;

    [Tooltip("Primary value — e.g. defence bonus, gold amount, EXP amount, HP heal, stat %.")]
    public float value;

    [Tooltip("Secondary value for multi-stat augments — e.g. Full Armour attack bonus, BoatLoadAndHP HP bonus.")]
    public float value2;
}
