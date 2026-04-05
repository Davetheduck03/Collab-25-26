using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishPrefab;

    [Tooltip("Drag the Boat scene instance here (NOT the Boat prefab asset).")]
    public GameObject boat;

    [Tooltip("Add all EnemySO fish here regardless of rarity — the spawner filters by zone.")]
    public List<EnemySO> fishData;

    [Header("Respawn Settings")]
    [Tooltip("Seconds to wait after a fish is caught before spawning the next one.")]
    public float respawnCooldown = 3f;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        CastLineControl.OnFishingFinished += HandleFishingFinished;
    }

    private void OnDisable()
    {
        CastLineControl.OnFishingFinished -= HandleFishingFinished;
    }

    private void Start()
    {
        Spawn();
    }

    // ── Respawn ───────────────────────────────────────────────────────────────

    // CHANGED: Added "bool success" so it perfectly matches the CastLineControl event!
    private void HandleFishingFinished(bool success)
    {
        StartCoroutine(RespawnAfterCooldown());
    }

    private IEnumerator RespawnAfterCooldown()
    {
        Debug.Log($"[FishSpawner] Respawning in {respawnCooldown}s...");
        yield return new WaitForSeconds(respawnCooldown);
        Spawn();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Fish Spawn()
    {
        if (fishPrefab == null || fishData == null || fishData.Count == 0)
        {
            Debug.LogError("[FishSpawner] Missing fishPrefab or fishData.");
            return null;
        }

        // Roll a rarity based on current zone weights
        Rarity rarity = ZoneManager.Instance != null
            ? ZoneManager.Instance.RollRarity()
            : Rarity.Common;

        // Filter pool to that rarity; fall back to Common if nothing matches
        var pool = fishData.FindAll(f => f.rarity == rarity);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"[FishSpawner] No fish of rarity {rarity} — falling back to Common.");
            pool = fishData.FindAll(f => f.rarity == Rarity.Common);
        }
        if (pool.Count == 0)
        {
            Debug.LogError("[FishSpawner] Fish pool is empty!");
            return null;
        }

        EnemySO chosen = pool[Random.Range(0, pool.Count)];

        // Compute zone stat scale: Base * Biome(1) * Zone * 1.1
        float scale = 1f;
        if (ZoneManager.Instance?.CurrentZone != null)
            scale = ZoneManager.Instance.CurrentZone.zoneMultiplier * 1.1f;

        // Spawn
        GameObject obj = Instantiate(fishPrefab, transform.position, Quaternion.identity);
        Fish fish = obj.GetComponent<Fish>();
        fish.unitData = chosen;
        fish.zoneStatScale = scale;

        if (boat != null)
            fish.m_Boat = boat;

        fish.Initialize();

        Debug.Log($"[FishSpawner] Spawned {chosen.UnitName} ({rarity}) with scale {scale:F2}");
        return fish;
    }
}