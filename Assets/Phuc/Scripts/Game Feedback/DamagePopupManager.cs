using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [Header("References")]
    [SerializeField] private Transform pfDamagePopup; // Drag your Prefab here

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(float damageAmount, Vector3 position)
    {
        if (pfDamagePopup == null) return;

        // Spawn slightly above the target position
        Vector3 spawnPos = position + Vector3.up * 1f;

        Transform damagePopupTransform = Instantiate(pfDamagePopup, spawnPos, Quaternion.identity);

        DamagePopup popupScript = damagePopupTransform.GetComponent<DamagePopup>();
        popupScript.Setup(damageAmount);
    }
}