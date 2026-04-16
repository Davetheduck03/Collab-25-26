using System;
using UnityEngine;

/// <summary>
/// Tracks gold earned during the current run against a configurable quota target.
/// Now upgraded to work with the TimeManager for daily end-of-day checks!
/// </summary>
public class QuotaManager : GameSingleton<QuotaManager>
{
    [Header("Quota Settings")]
    [Tooltip("Amount of gold that must be earned to meet the quota this run.")]
    public int goldTarget = 500;

    [Tooltip("How much the quota increases after successfully surviving a day.")]
    public int dailyQuotaIncrease = 250;

    // ── Runtime state ─────────────────────────────────────────────────────────
    private int goldEarned = 0;
    private bool quotaMet = false;
    private bool runEnded = false;   // prevents double-firing events

    // ── Public read-only access ───────────────────────────────────────────────
    public int GoldEarned => goldEarned;
    public int GoldTarget => goldTarget;
    public bool IsQuotaMet => quotaMet;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Fires whenever gold is earned. Args: (currentEarned, target).</summary>
    public static event Action<int, int> OnGoldEarned;

    /// <summary>Fires once when accumulated gold first reaches the target.</summary>
    public static event Action OnQuotaMet;

    /// <summary>Fires when the boat dies OR the end-of-day quota is failed.</summary>
    public static event Action OnRunFailed;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        // Only reset if we are the real singleton — not a duplicate being destroyed.
        // Without this guard, returning to the top-down scene creates a short-lived
        // duplicate that fires ResetRun() and wipes out the accumulated gold.
        if (_instance == this)
            ResetRun();
    }

    private void Start()
    {
        // Track all gold added through CurrencyManager
        CurrencyManager.OnCurrencyAdded += HandleGoldAdded;

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CurrencyManager.OnCurrencyAdded -= HandleGoldAdded;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Resets daily earnings (call at the start of a new day).</summary>
    public void ResetRun()
    {
        goldEarned = 0;
        quotaMet = false;
        runEnded = false;
        OnGoldEarned?.Invoke(goldEarned, goldTarget);
        Debug.Log($"[QuotaManager] Day started. Target to hit today: {goldTarget}g.");
    }


    /// <summary>Called by TimeManager/Bed when the player goes to sleep.</summary>
    public bool CheckQuotaAtEndOfDay()
    {
        if (runEnded) return false;

        Debug.Log($"[QuotaManager] End of day check! Earned: {goldEarned}/{goldTarget}");

        if (goldEarned >= goldTarget)
        {
            Debug.Log("[QuotaManager] Quota Met! You survive to fish another day.");
            IncreaseQuota();
            ZoneProgressionManager.Instance?.UnlockNextZone();
            ResetRun(); // Reset the daily earnings back to 0 for the next morning
            return true; // Survived!
        }
        else
        {
            Debug.Log("[QuotaManager] Quota FAILED! Game Over.");
            runEnded = true;
            OnRunFailed?.Invoke();
            return false; // Game Over!
        }
    }

    private void IncreaseQuota()
    {
        goldTarget += dailyQuotaIncrease;
        Debug.Log($"[QuotaManager] Quota has increased! New target is {goldTarget}g.");
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void HandleGoldAdded(int amount)
    {
        if (runEnded) return;

        goldEarned += amount;
        OnGoldEarned?.Invoke(goldEarned, goldTarget);
        Debug.Log($"[QuotaManager] Gold earned today: {goldEarned} / {goldTarget}");

        if (!quotaMet && goldEarned >= goldTarget)
        {
            quotaMet = true;
            Debug.Log("[QuotaManager] QUOTA MET FOR TODAY! You can safely go to sleep.");
            OnQuotaMet?.Invoke();
        }
    }

}