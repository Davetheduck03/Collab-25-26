using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persistent singleton that tracks which fishing zones are unlocked
/// and which zone the player has selected for the next fishing trip.
///
/// Setup:
///   1. Add this component to a GameObject in your top-down (hub) scene.
///   2. Assign all ZoneSO assets to allZones in order (Zone 0 first).
///   3. Zone 0 is always unlocked; each quota met unlocks the next zone.
///
/// Other systems:
///   • QuotaManager calls UnlockNextZone() when a quota is completed.
///   • ZoneManager reads SelectedZoneIndex on Start to set the active zone.
///   • PreFishingMenuUI calls GetUnlockedZones() to populate the zone list.
/// </summary>
public class ZoneProgressionManager : GameSingleton<ZoneProgressionManager>
{
    [Tooltip("All ZoneSO assets in unlock order. Zone 0 is always available.")]
    [SerializeField] private List<ZoneSO> allZones;

    private const string UNLOCKED_KEY  = "ZoneUnlockedCount";
    private const string SELECTED_KEY  = "ZoneSelectedIndex";

    // ── Runtime state ─────────────────────────────────────────────────────────

    /// <summary>How many zones are currently unlocked (always >= 1).</summary>
    public int UnlockedCount { get; private set; }

    /// <summary>Zone the player has chosen for the next fishing trip.</summary>
    public int SelectedZoneIndex
    {
        get => _selectedIndex;
        set => _selectedIndex = Mathf.Clamp(value, 0, Mathf.Max(0, UnlockedCount - 1));
    }
    private int _selectedIndex = 0;

    /// <summary>Total number of zones in the game.</summary>
    public int TotalZones => allZones != null ? allZones.Count : 0;

    /// <summary>True if every zone is already unlocked.</summary>
    public bool AllZonesUnlocked => UnlockedCount >= TotalZones;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fires when a new zone is unlocked. Arg = total unlocked count.</summary>
    public static event Action<int> OnZoneUnlocked;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    protected override void Awake()
    {
        persistAcrossScenes = true;
        base.Awake();

        UnlockedCount  = Mathf.Max(1, PlayerPrefs.GetInt(UNLOCKED_KEY, 1));
        _selectedIndex = Mathf.Clamp(PlayerPrefs.GetInt(SELECTED_KEY, 0), 0, UnlockedCount - 1);

        Debug.Log($"[ZoneProgressionManager] Loaded: {UnlockedCount}/{TotalZones} zones unlocked, selected={_selectedIndex}");
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Returns the ZoneSO at the given index, or null if out of range.</summary>
    public ZoneSO GetZone(int index)
    {
        if (allZones == null || index < 0 || index >= allZones.Count) return null;
        return allZones[index];
    }

    /// <summary>Returns all unlocked zones in order.</summary>
    public List<ZoneSO> GetUnlockedZones()
    {
        var result = new List<ZoneSO>();
        if (allZones == null) return result;
        int count = Mathf.Min(UnlockedCount, allZones.Count);
        for (int i = 0; i < count; i++)
            result.Add(allZones[i]);
        return result;
    }

    /// <summary>
    /// Unlock the next zone. Called by QuotaManager when a quota is completed.
    /// Does nothing if all zones are already unlocked.
    /// </summary>
    public void UnlockNextZone()
    {
        if (AllZonesUnlocked)
        {
            Debug.Log("[ZoneProgressionManager] All zones already unlocked.");
            return;
        }

        UnlockedCount++;
        PlayerPrefs.SetInt(UNLOCKED_KEY, UnlockedCount);
        PlayerPrefs.Save();

        Debug.Log($"[ZoneProgressionManager] Unlocked Zone {UnlockedCount - 1}! Total unlocked: {UnlockedCount}/{TotalZones}");
        OnZoneUnlocked?.Invoke(UnlockedCount);
    }

    /// <summary>Persist the selected zone index so it survives scene reloads.</summary>
    public void SaveSelectedZone()
    {
        PlayerPrefs.SetInt(SELECTED_KEY, _selectedIndex);
        PlayerPrefs.Save();
    }

    /// <summary>Debug helper — resets all progression (editor only).</summary>
    [ContextMenu("Reset Zone Progression")]
    private void ResetProgression()
    {
        UnlockedCount  = 1;
        _selectedIndex = 0;
        PlayerPrefs.SetInt(UNLOCKED_KEY, 1);
        PlayerPrefs.SetInt(SELECTED_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log("[ZoneProgressionManager] Progression reset.");
    }
}
