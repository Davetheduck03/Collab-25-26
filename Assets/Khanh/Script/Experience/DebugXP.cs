using UnityEngine;

public class DebugXP : MonoBehaviour
{
    [SerializeField] private float xpAmount = 10f;
    [SerializeField] private KeyCode key = KeyCode.X;

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            ExperienceManager.Instance?.AddExperience(xpAmount);
            Debug.Log($"[DebugXP] +{xpAmount} XP");
        }
    }
}
