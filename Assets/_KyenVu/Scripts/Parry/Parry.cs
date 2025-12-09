using UnityEngine;
using UnityEngine.UI; // Required for Image components

public class Parry : MonoBehaviour
{
    [Header("References")]
    public GameObject parryPanel;           // The parent container (e.g., a UI Panel)
    public RectTransform indicator;         // The moving needle/line object

    [Header("Visuals")]
    public Sprite partlyParrySprite;
    public Sprite perfectParrySprite;

    [Header("Settings")]
    public float oscillationSpeed = 200f;   // Speed of the indicator

    // State
    private bool isParrying = false;
    private float currentIndicatorValue = 0f; // 0 to 100
    private int direction = 1;

    // Ranges (0 to 100)
    private float partlyMin, partlyMax;
    private float perfectMin, perfectMax;

    // References to created sprite objects so we can clean them up
    private GameObject partlyZoneObj;
    private GameObject perfectZoneObj;

    void Start()
    {
        SetupIndicator();
        // For testing purposes, start immediately. 
        // In a real game, you would call BeginParry() from your Fish script.
        BeginParry();
    }

    void Update()
    {
        if (!isParrying) return;

        MoveIndicator();

        // Check Input (You can change KeyCode.Space to your specific input manager)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckResult();
        }
    }

    /// <summary>
    /// Call this to start the minigame
    /// </summary>
    public void BeginParry()
    {
        parryPanel.SetActive(true);
        CleanupOldVisuals();
        CalculateRanges();
        CreateZoneVisuals();

        currentIndicatorValue = 0;
        isParrying = true;
    }

    // --- NEW: Helper to fix the visual alignment ---
    private void SetupIndicator()
    {
        if (indicator != null)
        {
            // 1. Force the Pivot to the CENTER (X = 0.5)
            // This ensures the "Value" represents the middle of the needle, not the edge.
            // We keep the Y pivot however you had it (likely 0 for bottom or 0.5 for center)
            indicator.pivot = new Vector2(0.5f, indicator.pivot.y);

            // 2. Force the Anchor to Center Bottom (or Center Middle) for easier math
            // This prevents "stretching" because min and max are the same.
            indicator.anchorMin = new Vector2(0.5f, 0f);
            indicator.anchorMax = new Vector2(0.5f, 0f);
        }
    }


    private void CalculateRanges()
    {
        // 1. Calculate Partly Range (Width of 20, anywhere between 0-80)
        float partlyWidth = 20f;
        partlyMin = Random.Range(0f, 100f - partlyWidth);
        partlyMax = partlyMin + partlyWidth;

        // 2. Calculate Perfect Range (Width of 4, centered inside Partly)
        float center = (partlyMin + partlyMax) / 2f;
        float perfectWidth = 4f;
        perfectMin = center - (perfectWidth / 2f);
        perfectMax = center + (perfectWidth / 2f);
    }

    private void CreateZoneVisuals()
    {
        // Create the "Partly" Zone Image
        partlyZoneObj = CreateZoneImage("PartlyZone", partlyParrySprite, partlyMin, partlyMax);

        // Create the "Perfect" Zone Image (render it second so it appears on top)
        perfectZoneObj = CreateZoneImage("PerfectZone", perfectParrySprite, perfectMin, perfectMax);
    }

    private GameObject CreateZoneImage(string name, Sprite sprite, float minRange, float maxRange)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parryPanel.transform, false);

        // Add Image Component
        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced; // Allows stretching without distortion if set up correctly

        // Position using RectTransform Anchors (Percentage of parent width)
        RectTransform rt = go.GetComponent<RectTransform>();

        // Convert 0-100 range to 0.0-1.0 for anchors
        float minAnchorX = minRange / 100f;
        float maxAnchorX = maxRange / 100f;

        // Set Anchors to stretch horizontally based on the range
        rt.anchorMin = new Vector2(minAnchorX, 0f); // Bottom-Left of zone
        rt.anchorMax = new Vector2(maxAnchorX, 1f); // Top-Right of zone

        // Zero out offsets so it fills the anchored area exactly
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    private void MoveIndicator()
    {
        currentIndicatorValue += oscillationSpeed * direction * Time.deltaTime;

        if (currentIndicatorValue >= 100f)
        {
            currentIndicatorValue = 100f;
            direction = -1;
        }
        else if (currentIndicatorValue <= 0f)
        {
            currentIndicatorValue = 0f;
            direction = 1;
        }

        // Update UI Position of the indicator
        if (indicator != null)
        {
            // Get the width of the parent panel
            RectTransform parentRect = parryPanel.GetComponent<RectTransform>();
            float width = parentRect.rect.width;

            // Calculate X position relative to Center (Anchor 0.5)
            // 0 -> -Width/2
            // 50 -> 0
            // 100 -> +Width/2
            float normalizedPos = (currentIndicatorValue / 100f) - 0.5f;
            float xPos = normalizedPos * width;

            // Apply to anchoredPosition (keeping Y the same)
            indicator.anchoredPosition = new Vector2(xPos, indicator.anchoredPosition.y);
        }
    }

    private void CheckResult()
    {
        isParrying = false;

        if (currentIndicatorValue >= perfectMin && currentIndicatorValue <= perfectMax)
        {
            Debug.Log("<color=green>PERFECT PARRY!</color>");
            // Call success logic here
        }
        else if (currentIndicatorValue >= partlyMin && currentIndicatorValue <= partlyMax)
        {
            Debug.Log("<color=yellow>PARTLY PARRY</color>");
            // Call partial success logic here
        }
        else
        {
            Debug.Log("<color=red>MISS!</color>");
            // Call fail logic here
        }

        // Hide panel after short delay?
        // parryPanel.SetActive(false);
    }

    private void CleanupOldVisuals()
    {
        if (partlyZoneObj != null) Destroy(partlyZoneObj);
        if (perfectZoneObj != null) Destroy(perfectZoneObj);
    }
}