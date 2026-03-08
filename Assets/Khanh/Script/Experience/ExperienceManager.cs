using System;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerExpData
{
    public int playerLevel = 1;
    public float currentExp = 0f;
}

public class ExperienceManager : GameSingleton<ExperienceManager>
{
    // Fired with the new level number when a level-up occurs
    public static event Action<int> OnLevelUp;

    // Fired whenever exp changes: (currentExp, expRequired)
    public static event Action<float, float> OnExpChanged;

    [Header("Config")]
    [Tooltip("Base EXP required to reach level 2.")]
    [SerializeField] private float baseExpThreshold = 100f;

    [Tooltip("Multiplier applied each level. e.g. 1.5 = each level costs 50% more EXP.")]
    [SerializeField] private float thresholdMultiplier = 1.5f;

    private PlayerExpData expData = new PlayerExpData();
    private string SavePath => Path.Combine(Application.persistentDataPath, "playerExp.json");

    public int CurrentLevel => expData.playerLevel;
    public float CurrentExp => expData.currentExp;
    public float ExpRequired => GetThreshold(expData.playerLevel);

    // ─── Unity ────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        LoadData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>Returns the EXP required to level up from <paramref name="level"/>.</summary>
    public float GetThreshold(int level)
    {
        return Mathf.Round(baseExpThreshold * Mathf.Pow(thresholdMultiplier, level - 1));
    }

    /// <summary>Add experience, trigger level-ups if threshold reached.</summary>
    public void AddExperience(float amount)
    {
        if (amount <= 0) return;

        expData.currentExp += amount;
        Debug.Log($"[ExperienceManager] +{amount} EXP → {expData.currentExp}/{GetThreshold(expData.playerLevel)} (Level {expData.playerLevel})");

        // Handle one or more level-ups in a single EXP gain
        while (expData.currentExp >= GetThreshold(expData.playerLevel))
        {
            expData.currentExp -= GetThreshold(expData.playerLevel);
            expData.playerLevel++;

            Debug.Log($"[ExperienceManager] LEVEL UP! Now Level {expData.playerLevel}");
            OnLevelUp?.Invoke(expData.playerLevel);

            if (AugmentPanelManager.Instance != null)
                AugmentPanelManager.Instance.OpenPanel();
            else
                Debug.LogWarning("[ExperienceManager] AugmentPanelManager not found in scene.");
        }

        OnExpChanged?.Invoke(expData.currentExp, GetThreshold(expData.playerLevel));
        SaveData();
    }

    // ─── Save / Load ──────────────────────────────────────────────────────────

    private void SaveData()
    {
        string json = JsonUtility.ToJson(expData);
        File.WriteAllText(SavePath, json);
    }

    private void LoadData()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            expData = JsonUtility.FromJson<PlayerExpData>(json);
            Debug.Log($"[ExperienceManager] Loaded — Level {expData.playerLevel}, EXP {expData.currentExp}");
        }
        else
        {
            expData = new PlayerExpData();
        }
    }
}
