using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [Header("References")]
    [SerializeField] private Transform pfDamagePopup;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(float damageAmount, Vector3 position)
    {
        if (pfDamagePopup == null) return;

        Vector3 spawnPos = position + Vector3.up * 1f;
        Transform damagePopupTransform = Instantiate(pfDamagePopup, spawnPos, Quaternion.identity);

        DamagePopup popupScript = damagePopupTransform.GetComponent<DamagePopup>();
        popupScript.Setup(damageAmount);
    }

    // CHANGED: Now returns a GameObject and takes a 'size' parameter
    public GameObject ShowPrompt(string text, Vector3 position, Color color, float lifetime, float size)
    {
        if (pfDamagePopup == null) return null;

        // CHANGED: Made the random offset much smaller so it spawns very close to the fish
        Vector3 randomOffset = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(0.2f, 1.2f), 0f);
        Vector3 spawnPos = position + randomOffset;

        Transform popupTransform = Instantiate(pfDamagePopup, spawnPos, Quaternion.identity);
        DamagePopup popupScript = popupTransform.GetComponent<DamagePopup>();

        popupScript.SetupText(text, color, lifetime, size);

        // Return the object so the Fish can destroy it early!
        return popupTransform.gameObject;
    }
}