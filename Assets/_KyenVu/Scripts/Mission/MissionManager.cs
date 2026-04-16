using System.Collections.Generic;
using UnityEngine;

// NEW: Tracks the progress of a single objective inside a mission
[System.Serializable]
public class ObjectiveProgress
{
    public MissionObjective objective;
    public int currentAmount;
    public bool isComplete => currentAmount >= objective.targetAmount;

    public ObjectiveProgress(MissionObjective obj)
    {
        objective = obj;
        currentAmount = 0;
    }
}

// =======================================================
// --- UPDATED: ActiveMission now remembers the NPC! ---
// =======================================================
[System.Serializable]
public class ActiveMission
{
    public MissionSO missionData;
    public List<ObjectiveProgress> progressList = new List<ObjectiveProgress>();

    // NEW: Stores the name of the NPC who gave this mission
    public string questGiver;

    public bool isReadyToTurnIn
    {
        get
        {
            foreach (var p in progressList)
            {
                if (!p.isComplete) return false;
            }
            return true;
        }
    }

    // NEW: We now require a name when a mission is created
    public ActiveMission(MissionSO data, string npcName)
    {
        missionData = data;
        questGiver = npcName;

        foreach (var obj in data.objectives)
        {
            progressList.Add(new ObjectiveProgress(obj));
        }
    }
}

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance { get; private set; }

    [Header("Active Missions")]
    public List<ActiveMission> activeMissions = new List<ActiveMission>();

    [Header("Hidden Encounters")]
    private Dictionary<string, int> npcEncounters = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad only works on root GameObjects.
            // SetParent(null) promotes this object to root first if it's a child.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- ENCOUNTER SYSTEM ---

    public void IncreaseEncounter(string npcName)
    {
        // Get the current level (this also loads it if it wasn't loaded yet)
        int currentLevel = GetEncounterLevel(npcName);
        currentLevel++;

        npcEncounters[npcName] = currentLevel;

        // NEW: Save the progress permanently!
        PlayerPrefs.SetInt("Encounter_" + npcName, currentLevel);
        PlayerPrefs.Save();

        Debug.Log($"[Encounter] {npcName} relationship increased to {currentLevel} and saved!");
    }

    public int GetEncounterLevel(string npcName)
    {
        // NEW: Load from PlayerPrefs if we haven't loaded it yet this session
        if (!npcEncounters.ContainsKey(npcName))
        {
            npcEncounters[npcName] = PlayerPrefs.GetInt("Encounter_" + npcName, 0);
        }

        return npcEncounters[npcName];
    }

    // --- MISSION SYSTEM ---

    public void StartMission(MissionSO mission, string npcName = "")
    {
        if (activeMissions.Exists(m => m.missionData == mission)) return;

        // Pass the NPC name into the new mission!
        activeMissions.Add(new ActiveMission(mission, npcName));
        Debug.Log($"[Mission] Started: {mission.missionName} (Given by: {npcName})");
    }
    public void ProgressMission(MissionType type, string targetID, int amount = 1)
    {
        // --- NEW: Create a temporary list to hold missions that just finished ---
        List<MissionSO> newlyCompletedMissions = new List<MissionSO>();

        // Check every active mission
        foreach (var mission in activeMissions)
        {
            // Check every objective inside that mission
            foreach (var progress in mission.progressList)
            {
                // Did we catch the right fish/item?
                if (progress.objective.missionType == type && progress.objective.GetTargetID() == targetID)
                {
                    // Don't add more if this specific objective is already done
                    if (!progress.isComplete)
                    {
                        progress.currentAmount += amount;
                        Debug.Log($"[Mission] {mission.missionData.missionName} | Progress on {targetID}: {progress.currentAmount}/{progress.objective.targetAmount}");

                        // If this was the last objective needed, add it to our finished list!
                        if (mission.isReadyToTurnIn)
                        {
                            Debug.Log($"[Mission] {mission.missionData.missionName} is fully complete! Auto-turning in...");
                            newlyCompletedMissions.Add(mission.missionData);
                        }
                    }
                }
            }
        }

        // --- NEW: Safely turn in all the finished missions now that we are done looking at the list! ---
        foreach (var finishedMission in newlyCompletedMissions)
        {
            TurnInMission(finishedMission);
        }
    }

    public void TurnInMission(MissionSO mission)
    {
        var active = activeMissions.Find(m => m.missionData == mission);
        if (active != null && active.isReadyToTurnIn)
        {
            activeMissions.Remove(active);
            Debug.Log($"[Mission] Successfully turned in: {mission.missionName}");

            // Give Gold Reward
            if (mission.goldReward > 0 && CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddFreeCurrency(mission.goldReward);
                if (NotificationManager.Instance != null)
                    NotificationManager.Instance.ShowNotification($"Mission Complete! +{mission.goldReward} Gold");
            }

            // --- NEW: Level up the specific NPC who gave us this quest! ---
            if (!string.IsNullOrEmpty(active.questGiver))
            {
                IncreaseEncounter(active.questGiver);
            }
        }
    }
}
