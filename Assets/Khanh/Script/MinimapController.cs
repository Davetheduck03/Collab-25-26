using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages the minimap UI in the fishing scene.
/// Place on the MinimapPanel GameObject (inside MinimapCanvas).
/// Set worldCenter and worldSize to match your fishing area bounds.
/// </summary>
public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance { get; private set; }

    [Header("UI References")]
    public RectTransform minimapRect;
    public RectTransform blipContainer;
    public GameObject blipPrefab;

    [Header("World Bounds")]
    [Tooltip("Centre of the fishing area in world space")]
    public Vector2 worldCenter = Vector2.zero;
    [Tooltip("Width and height of the fishing area in world units")]
    public Vector2 worldSize   = new Vector2(100f, 100f);

    [Header("Player")]
    [Tooltip("Leave empty to auto-find by 'Boat' tag")]
    public Transform boatTransform;

    // ── internal state ────────────────────────────────────────────────────────
    private readonly List<MinimapFishBlip> fishBlips = new();
    private readonly Dictionary<MinimapFishBlip, RectTransform> blipRects = new();

    private RectTransform playerArrow;
    private Vector3 lastBoatPos;

    // blip colours
    private static readonly Color ColNormal    = new Color(0.5f, 0.8f, 1f);     // light blue
    private static readonly Color ColQuest     = new Color(1f,   0.85f, 0.2f);  // gold
    private static readonly Color ColCompleted = new Color(0.4f, 1f,   0.5f);   // soft green

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (boatTransform == null)
        {
            var boat = GameObject.FindGameObjectWithTag("Boat");
            if (boat != null) boatTransform = boat.transform;
        }

        if (boatTransform != null)
            CreatePlayerArrow();
    }

    private void LateUpdate()
    {
        UpdateFishBlips();
        UpdatePlayerArrow();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Registration API
    // ─────────────────────────────────────────────────────────────────────────

    public void RegisterFish(MinimapFishBlip blip)
    {
        if (fishBlips.Contains(blip)) return;
        fishBlips.Add(blip);

        var rt = CreateBlip(ColNormal);
        blipRects[blip] = rt;
    }

    public void UnregisterFish(MinimapFishBlip blip)
    {
        if (blipRects.TryGetValue(blip, out var rt))
        {
            if (rt != null) Destroy(rt.gameObject);
            blipRects.Remove(blip);
        }
        fishBlips.Remove(blip);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Update helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void UpdateFishBlips()
    {
        foreach (var blip in fishBlips)
        {
            if (blip == null) continue;
            if (!blipRects.TryGetValue(blip, out var rt) || rt == null) continue;

            rt.anchoredPosition = WorldToMinimap(blip.transform.position);
            rt.GetComponent<Image>().color = GetBlipColor(blip.FishName);
        }
    }

    private void UpdatePlayerArrow()
    {
        if (playerArrow == null || boatTransform == null) return;

        playerArrow.anchoredPosition = WorldToMinimap(boatTransform.position);

        // Rotate arrow tip to face movement direction
        Vector3 delta = boatTransform.position - lastBoatPos;
        if (delta.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f;
            playerArrow.rotation = Quaternion.Euler(0, 0, angle);
        }
        lastBoatPos = boatTransform.position;

        // Keep player on top
        playerArrow.SetAsLastSibling();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Coordinate conversion
    // ─────────────────────────────────────────────────────────────────────────

    private Vector2 WorldToMinimap(Vector3 worldPos)
    {
        float nx = (worldPos.x - worldCenter.x) / worldSize.x + 0.5f;
        float ny = (worldPos.y - worldCenter.y) / worldSize.y + 0.5f;

        nx = Mathf.Clamp01(nx);
        ny = Mathf.Clamp01(ny);

        float mapW = minimapRect.rect.width;
        float mapH = minimapRect.rect.height;

        return new Vector2((nx - 0.5f) * mapW, (ny - 0.5f) * mapH);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Blip & arrow creation
    // ─────────────────────────────────────────────────────────────────────────

    private RectTransform CreateBlip(Color color)
    {
        var go = blipPrefab != null
            ? Instantiate(blipPrefab, blipContainer)
            : CreateFallbackBlip();

        var rt  = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();
        if (img != null) img.color = color;
        rt.sizeDelta = new Vector2(8f, 8f);
        return rt;
    }

    private GameObject CreateFallbackBlip()
    {
        var go  = new GameObject("Blip", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(blipContainer, false);
        return go;
    }

    /// <summary>
    /// Builds a layered player indicator: outer ring + fill dot + directional tip.
    /// </summary>
    private void CreatePlayerArrow()
    {
        // Root pivot
        var rootGO = new GameObject("PlayerArrow", typeof(RectTransform));
        rootGO.transform.SetParent(blipContainer, false);
        playerArrow = rootGO.GetComponent<RectTransform>();
        playerArrow.sizeDelta = new Vector2(14f, 14f);

        // Outer ring
        var ring    = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ring.transform.SetParent(rootGO.transform, false);
        var ringRT  = ring.GetComponent<RectTransform>();
        ringRT.anchorMin = Vector2.zero; ringRT.anchorMax = Vector2.one;
        ringRT.offsetMin = ringRT.offsetMax = Vector2.zero;
        var ringImg = ring.GetComponent<Image>();
        ringImg.color = new Color(1f, 1f, 1f, 0.6f);

        // Fill dot
        var fill    = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(rootGO.transform, false);
        var fillRT  = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0.2f, 0.2f);
        fillRT.anchorMax = new Vector2(0.8f, 0.8f);
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        var fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 1f);

        // Directional tip (small triangle above center)
        var tip    = new GameObject("Tip", typeof(RectTransform), typeof(Image));
        tip.transform.SetParent(rootGO.transform, false);
        var tipRT  = tip.GetComponent<RectTransform>();
        tipRT.anchorMin        = new Vector2(0.35f, 0.75f);
        tipRT.anchorMax        = new Vector2(0.65f, 1.1f);
        tipRT.offsetMin        = tipRT.offsetMax = Vector2.zero;
        var tipImg = tip.GetComponent<Image>();
        tipImg.color = new Color(0.3f, 0.7f, 1f);

        playerArrow.SetAsLastSibling();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Quest colour logic
    // ─────────────────────────────────────────────────────────────────────────

    private Color GetBlipColor(string fishName)
    {
        if (string.IsNullOrEmpty(fishName) || MissionManager.Instance == null)
            return ColNormal;

        foreach (var mission in MissionManager.Instance.activeMissions)
        {
            foreach (var progress in mission.progressList)
            {
                if (progress.objective.missionType != MissionType.Fish) continue;
                if (progress.objective.targetFish == null) continue;
                if (progress.objective.targetFish.UnitName != fishName) continue;

                return progress.isComplete ? ColCompleted : ColQuest;
            }
        }
        return ColNormal;
    }
}
