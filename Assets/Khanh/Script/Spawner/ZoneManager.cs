using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : GameSingleton<ZoneManager>
{
    [Tooltip("Drag all ZoneSO assets here in order from nearest to furthest.")]
    [SerializeField] private List<ZoneSO> zones;

    public ZoneSO CurrentZone { get; private set; }

    /// <summary>How far the boat is from this ZoneManager's position (horizontal).</summary>
    public float DistanceFromOrigin { get; private set; }

    private Transform _boatTransform;

    // ── Unity ─────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        // Prefer the zone the player selected in the pre-fishing menu.
        // Fall back to zones[0] if no progression manager is present (e.g. direct play in editor).
        var prog = ZoneProgressionManager.Instance;
        if (prog != null && prog.GetZone(prog.SelectedZoneIndex) != null)
        {
            CurrentZone = prog.GetZone(prog.SelectedZoneIndex);
            Debug.Log($"[ZoneManager] Using selected zone: {CurrentZone.zoneName}");
        }
        else if (zones != null && zones.Count > 0)
        {
            CurrentZone = zones[0];
        }
        else
        {
            Debug.LogWarning("[ZoneManager] No zones assigned!");
        }
    }

    private void Start()
    {
        // Only track boat position when no zone has been explicitly chosen.
        if (ZoneProgressionManager.Instance != null) return;

        var boat = GameObject.FindWithTag("Boat") ?? GameObject.Find("Boat");
        if (boat != null)
            _boatTransform = boat.transform;
        else
            Debug.LogWarning("[ZoneManager] Boat not found — distance tracking disabled.");
    }

    private void Update()
    {
        // When the player pre-selected a zone, keep it locked for the whole trip.
        if (ZoneProgressionManager.Instance != null) return;

        if (_boatTransform == null) return;

        DistanceFromOrigin = Mathf.Abs(_boatTransform.position.x - transform.position.x);

        foreach (var zone in zones)
        {
            if (DistanceFromOrigin >= zone.minDis && DistanceFromOrigin < zone.maxDis)
            {
                if (CurrentZone != zone)
                {
                    CurrentZone = zone;
                    Debug.Log($"[ZoneManager] Entered zone: {zone.name} at {DistanceFromOrigin:F1}f");
                }
                return;
            }
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Roll a rarity using the current zone's spawn weights.</summary>
    public Rarity RollRarity()
    {
        if (CurrentZone == null)
        {
            Debug.LogWarning("[ZoneManager] No current zone — defaulting to Common.");
            return Rarity.Common;
        }
        return CurrentZone.RollRarity();
    }
}
