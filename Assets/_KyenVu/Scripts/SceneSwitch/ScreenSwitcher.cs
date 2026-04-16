using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenSwitcher : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Type the exact name of the scene you want to load")]
    public string sceneToLoad = "2D scene";
    public Animator sceneTransitionAnimator;

    // This is the public method we will trigger from the UnityEvent
    public void SwitchScene()
    {
        if (TimeManager.Instance != null && !TimeManager.Instance.IsDockOpen())
        {
            NotificationManager.Instance.ShowNotification("The dock is closed, please come back at 7:00 AM.");
            return; // Stop the code, don't let them on the boat!
        }

        // =================================================================
        // --- NEW: CHECK FOR FISHING ROD BEFORE LETTING THEM LEAVE ---
        // =================================================================
        bool hasRod = false;

        // 1. Check if it's in their inventory
        if (InventoryController.Instance != null)
        {
            foreach (var item in InventoryController.Instance.items)
            {
                if (item.data is RodItemData)
                {
                    hasRod = true;
                    break;
                }
            }
        }

        // 2. Check if it is already equipped (just in case!)
        if (!hasRod && EquipmentManager.Instance != null)
        {
            if (EquipmentManager.Instance.GetEquippedRod() != null)
            {
                hasRod = true;
            }
        }

        // 3. Block them if they don't have one
        if (!hasRod)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("You need to buy a Fishing Rod from the shop first! Accept the mission for some money");
            }
            return; // Stop the code, don't let them on the boat!
        }
        // =================================================================

        // Show the equipment selection menu before entering the fishing scene.
        // The menu will call SceneManager.LoadScene(sceneToLoad) when the player confirms.
        if (PreFishingMenuUI.Instance != null)
        {
            PreFishingMenuUI.Instance.Open(sceneToLoad);
            return;
        }

        // Fallback: load directly if the menu isn't present in the scene.
        if (sceneTransitionAnimator != null)
            sceneTransitionAnimator.SetTrigger("FadeOut");

        SceneManager.LoadScene(sceneToLoad);
    }
}