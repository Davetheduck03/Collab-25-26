using UnityEngine;

public class MainMenuCleaner : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[MainMenuCleaner] Wiping old game session data...");

        // 1. Destroy the persistent gameplay managers so they reset perfectly next time
        if (TimeManager.Instance != null) Destroy(TimeManager.Instance.gameObject);
        if (MissionManager.Instance != null) Destroy(MissionManager.Instance.gameObject);
        if (QuotaManager.Instance != null) Destroy(QuotaManager.Instance.gameObject);

        // Try to wipe the UI managers just in case they got stuck as zombies
        if (DailyResultUI.Instance != null) Destroy(DailyResultUI.Instance.gameObject);
        if (DialogueManager.Instance != null) Destroy(DialogueManager.Instance.gameObject);

        // =========================================================
        // IMPORTANT: If your Inventory, Currency, or Equipment 
        // managers also use DontDestroyOnLoad, you MUST wipe them here!
        // Just remove the "//" from the front of these lines if you have them:
        // =========================================================

        if (InventoryController.Instance != null) Destroy(InventoryController.Instance.gameObject);
        if (CurrencyManager.Instance != null) Destroy(CurrencyManager.Instance.gameObject);
        if (EquipmentManager.Instance != null) Destroy(EquipmentManager.Instance.gameObject);


        // NOTE: We do NOT destroy the SoundManager or GlobalUISoundInjector! 
        // We want the music and button sounds to keep working on the Main Menu!

        Debug.Log("[MainMenuCleaner] Game session successfully wiped! Ready for a new game.");
    }
}