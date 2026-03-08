using System;
using UnityEngine;

/// <summary>
/// Tracks gold earned during the current run against a configurable quota target.
///
/// Hookup:
///  • Add QuotaManager to your ManagerContainer prefab (or any persistent GO).
///  • Set goldTarget in the Inspector (e.g. 500).
///  • QuotaManager auto-subscribes to CurrencyManager.OnCurrencyAdded for gold tracking.
///  • QuotaManager auto-subscribes to the Boat's HealthComponent.OnDeath for failure.
///
/// Progression (not implemented yet):
///  • Listen to QuotaManager.OnQuotaMet to unlock next zone, end the day, etc.
///
/// Failure:
///  • Listen to QuotaManager.OnRunFailed to show game-over screen, reload, etc.
/// </summary>
public class QuotaManager : GameSingleton<QuotaManager>
{
    [Header("Quota Settings")]
    [Tooltip("Amount of gold that must be earned to meet the quota this run.")]
    public int goldTarget = 500;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int  goldEarned   = 0;
    private bool quotaMet     = false;
    private bool runEnded     = false;   // prevents double-firing events

    // ── Public read-only access ───────────────────────────────────────────────
    public int  GoldEarned  => goldEarned;
    public int  GoldTarget  => goldTarget;
    public bool IsQuotaMet  => quotaMet;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fires whenever gold is earned. Args: (currentEarned, target).</summary>
    public static event Action<int, int> OnGoldEarned;

    /// <summary>Fires once when accumulated gold first reaches the target.</summary>
    public static event Action OnQuotaMet;

    /// <summary>Fires when the boat dies (run failed before quota was met, or after).</summary>
    public static event Action OnRunFailed;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        ResetRun();
    }

    private void Start()
    {
        // Track all gold added through CurrencyManager
        CurrencyManager.OnCurrencyAdded += HandleGoldAdded;

        // Subscribe to the boat's death event for failure
        SubscribeToBoatDeath();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CurrencyManager.OnCurrencyAdded -= HandleGoldAdded;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Resets run state (call at the start of a new run/day).</summary>
    public void ResetRun()
    {
        goldEarned = 0;
        quotaMet   = false;
        runEnded   = false;
        OnGoldEarned?.Invoke(goldEarned, goldTarget);
        Debug.Log($"[QuotaManager] Run reset. Target: {goldTarget}g.");
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void HandleGoldAdded(int amount)
    {
        if (runEnded) return;

        goldEarned += amount;
        OnGoldEarned?.Invoke(goldEarned, goldTarget);
        Debug.Log($"[QuotaManager] Gold earned: {goldEarned} / {goldTarget}");

        if (!quotaMet && goldEarned >= goldTarget)
        {
            quotaMet = true;
            Debug.Log("[QuotaManager] QUOTA MET!");
            OnQuotaMet?.Invoke();
            // Progression hook goes here later (load next zone, etc.)
        }
    }

    private void HandleBoatDeath()
    {
        if (runEnded) return;
        runEnded = true;

        Debug.Log($"[QuotaManager] Boat destroyed — run failed. Earned {goldEarned} / {goldTarget}g.");
        OnRunFailed?.Invoke();
    }

    private void SubscribeToBoatDeath()
    {
        var boat = GameObject.FindWithTag("Boat") ?? GameObject.Find("Boat");
        if (boat == null)
        {
            Debug.LogWarning("[QuotaManager] Could not find 'Boat' — death detection disabled. Make sure the Boat GameObject is tagged 'Boat'.");
            return;
        }

        var hp = boat.GetComponent<HealthComponent>();
        if (hp == null)
        {
            Debug.LogWarning("[QuotaManager] Boat has no HealthComponent — death detection disabled.");
            return;
        }

        hp.OnDeath += HandleBoatDeath;
        Debug.Log("[QuotaManager] Subscribed to Boat HealthComponent.OnDeath.");
    }
}
