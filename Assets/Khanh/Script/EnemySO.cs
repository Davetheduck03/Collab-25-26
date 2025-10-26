using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "FishyWishy/New Enemy Stat")]

public class EnemySO : UnitSO
{
    [Header("Enemy Specific Info")]
    public float ExperienceReward;
    public float BasePrice;
}
