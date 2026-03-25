using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persists across scenes and holds all active trigger/passive augment effects.
/// Add this component to the same persistent GameObject as ExperienceManager.
/// </summary>
public class AugmentTriggerManager : GameSingleton<AugmentTriggerManager>
{
    private HashSet<AugmentEffect> activeTriggers = new HashSet<AugmentEffect>();

    // Single-use flags
    private bool ascendedUsed = false;
    private bool miniDoctorUsed = false;
    private bool adrenalineUsed = false;

    // Skyborn: fire on the next fish caught then remove
    private bool skybornReady = false;

    // ── Unity ─────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        HealthComponent.OnDamageTaken   += HandleDamageTaken;
        CastLineControl.OnFishCaught    += HandleFishCaught;
        AugmentPanelManager.OnPanelClosed += HandlePanelClosed;
    }

    protected override void OnDestroy()
    {
        HealthComponent.OnDamageTaken   -= HandleDamageTaken;
        CastLineControl.OnFishCaught    -= HandleFishCaught;
        AugmentPanelManager.OnPanelClosed -= HandlePanelClosed;
        base.OnDestroy();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void RegisterTrigger(AugmentSO augment)
    {
        activeTriggers.Add(augment.effect);

        switch (augment.effect)
        {
            case AugmentEffect.TriggerWheresMyTea:
                StartCoroutine(WheresMyTeaRoutine(augment.value));
                break;

            case AugmentEffect.TriggerShadyBusiness:
                ApplyShadyBusiness();
                activeTriggers.Remove(AugmentEffect.TriggerShadyBusiness); // one-shot immediate
                break;

            case AugmentEffect.TriggerSkyborn:
                skybornReady = true;
                break;
        }

        Debug.Log($"[AugmentTriggerManager] Registered: {augment.effect}");
    }

    // ── Event Handlers ────────────────────────────────────────────────────────

    private void HandleDamageTaken(HealthComponent hc)
    {
        bool isBoat = hc.CompareTag("Boat") || hc.gameObject.name == "Boat";

        // ── I AM THE ASCENDED ──────────────────────────────────────────────
        if (isBoat && !ascendedUsed && activeTriggers.Contains(AugmentEffect.TriggerAscended))
        {
            if (hc.currentHealth <= 10f)
            {
                ascendedUsed = true;
                float maxHP = UpgradeManager.Instance != null
                    ? UpgradeManager.Instance.ComputeStat(UpgradeType.Health)
                    : hc.currentHealth + 20f;
                hc.currentHealth = Mathf.Min(hc.currentHealth + 20f, maxHP);
                Debug.Log($"[Augment] I AM THE ASCENDED triggered! HP → {hc.currentHealth}");
            }
        }

        // ── Adrenaline Rush ────────────────────────────────────────────────
        // TODO: replace the check below with a real runtime stamina value once
        // a StaminaManager is implemented. Currently this fires on boat damage
        // as a placeholder to show the hook is wired.
        if (isBoat && !adrenalineUsed && activeTriggers.Contains(AugmentEffect.TriggerAdrenalineRush))
        {
            // Placeholder: actual trigger is "stamina == 0"
            // When StaminaManager exists: if (StaminaManager.Instance.Current <= 0)
            Debug.Log("[Augment] Adrenaline Rush — waiting for stamina runtime to be implemented.");
        }
    }

    private void HandleFishCaught(GameObject fish)
    {
        // ── Critical Strike ────────────────────────────────────────────────
        // When the fish is at full HP (just caught), deal 15% of max HP as bonus damage.
        if (activeTriggers.Contains(AugmentEffect.TriggerCriticalStrike))
        {
            var fishHP = fish.GetComponent<HealthComponent>();
            if (fishHP != null)
            {
                float crit = fishHP.currentHealth * 0.15f;
                fishHP.currentHealth = Mathf.Max(0f, fishHP.currentHealth - crit);
                Debug.Log($"[Augment] Critical Strike! -{crit:F1} HP to fish on hook.");
            }
        }

        // ── Skyborn Fish ────────────────────────────────────────────────────
        if (skybornReady && activeTriggers.Contains(AugmentEffect.TriggerSkyborn))
        {
            skybornReady = false;
            activeTriggers.Remove(AugmentEffect.TriggerSkyborn);
            ApplySkyborn();
        }
    }

    private void HandlePanelClosed()
    {
        // ── Mini Doctor ────────────────────────────────────────────────────
        // Heals ALL missing HP the moment the augment panel closes (single use).
        if (!miniDoctorUsed && activeTriggers.Contains(AugmentEffect.TriggerMiniDoctor))
        {
            miniDoctorUsed = true;
            var hp = AugmentApplier.FindBoatHealthComponent();
            if (hp != null)
            {
                float maxHP = UpgradeManager.Instance != null
                    ? UpgradeManager.Instance.ComputeStat(UpgradeType.Health)
                    : hp.currentHealth;
                float healed = maxHP - hp.currentHealth;
                hp.currentHealth = maxHP;
                Debug.Log($"[Augment] Mini Doctor! Healed {healed:F1} missing HP → full.");
            }
        }
    }

    // ── Trigger Implementations ───────────────────────────────────────────────

    private IEnumerator WheresMyTeaRoutine(float duration)
    {
        UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Stamina, 20f);
        Debug.Log("[Augment] Where's my tea?! +20 Max Stamina for " + duration + "s.");

        yield return new WaitForSeconds(duration);

        UpgradeManager.Instance?.ApplyAugmentBonus(UpgradeType.Stamina, -20f);
        activeTriggers.Remove(AugmentEffect.TriggerWheresMyTea);
        Debug.Log("[Augment] Where's my tea?! expired. -20 Max Stamina.");
    }

    private void ApplyShadyBusiness()
    {
        if (InventoryController.Instance == null) return;

        var fishItems = InventoryController.Instance.items.FindAll(i => i.data is FishItemData);
        int toRemove = Mathf.FloorToInt(fishItems.Count * 0.25f);

        for (int i = 0; i < toRemove; i++)
            InventoryController.Instance.RemoveItem(fishItems[i]);

        var hp = AugmentApplier.FindBoatHealthComponent();
        if (hp != null)
        {
            float maxHP = UpgradeManager.Instance != null
                ? UpgradeManager.Instance.ComputeStat(UpgradeType.Health)
                : hp.currentHealth;
            hp.currentHealth = maxHP;
        }

        Debug.Log($"[Augment] Shady Business! Removed {toRemove} fish → full HP.");
    }

    private void ApplySkyborn()
    {
        float roll = Random.Range(0f, 100f);
        // 75% → 2 common fish  |  24.5% → 1 uncommon fish  |  0.5% → 1 rare fish
        // TODO: replace the strings below with references to actual FishItemData assets.
        // Easiest approach: add [SerializeField] FishItemData[] skybornFishPool on this component
        // and pick by rarity from there.
        if (roll < 75f)
            Debug.Log("[Augment] Skyborn Fish: would grant 2 common fish — wire FishItemData assets.");
        else if (roll < 99.5f)
            Debug.Log("[Augment] Skyborn Fish: would grant 1 uncommon fish — wire FishItemData assets.");
        else
            Debug.Log("[Augment] Skyborn Fish: would grant 1 rare fish — wire FishItemData assets.");
    }
}
