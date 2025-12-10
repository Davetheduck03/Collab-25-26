using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Parry : MonoBehaviour
{
    [Header("UI References")]
    public GameObject parryPanel;
    public Slider parrySlider;

    [Header("Visuals")]
    public Sprite partlyParrySprite;
    public Sprite perfectParrySprite;

    // Remove fixed oscillationSpeed, pass it in dynamically
    private float currentSpeed;

    [Header("Debug Settings")]
    public bool showGizmos = true;
    public Vector3 gizmoOffset = new Vector3(0, 100, 0);
    public Vector2 gizmoSize = new Vector2(200, 20);

    // State
    private bool isParrying = false;
    private float currentVal = 0f;
    private int direction = 1;

    // Timeout Logic
    private float timer;
    private float maxDuration;

    // Callback to notify Fish of the result (True = Success, False = Fail)
    private Action<bool> onResultCallback;

    // Ranges
    private float partlyMin, partlyMax;
    private float perfectMin, perfectMax;

    // Cleanup
    private GameObject partlyZoneObj;
    private GameObject perfectZoneObj;

    void Start()
    {
        if (parryPanel != null) parryPanel.SetActive(false);

        if (parrySlider != null)
        {
            parrySlider.minValue = 0f;
            parrySlider.maxValue = 100f;
            parrySlider.interactable = false;
            parrySlider.wholeNumbers = false;
        }
    }

    void Update()
    {
        if (!isParrying) return;

        MoveHandle();
        HandleTimer();

        // REMOVED: Input.GetKeyDown(KeyCode.Space)
        // We now rely on HandleParryInput triggered by the event
    }


    /// <summary>
    /// Starts the minigame with specific difficulty settings.
    /// </summary>
    public void BeginParry(float speed, float duration, Action<bool> onResult)
    {
        parryPanel.SetActive(true);
        CleanupOldVisuals();
        CalculateRanges();
        CreateZoneVisuals();

        currentVal = 0f;
        direction = 1;

        // Apply settings
        currentSpeed = speed;
        maxDuration = duration;
        timer = 0f;
        onResultCallback = onResult;

        isParrying = true;
    }

    private void MoveHandle()
    {
        currentVal += currentSpeed * direction * Time.deltaTime;

        if (currentVal >= 100f)
        {
            currentVal = 100f;
            direction = -1;
        }
        else if (currentVal <= 0f)
        {
            currentVal = 0f;
            direction = 1;
        }

        if (parrySlider != null) parrySlider.value = currentVal;
    }

    private void HandleTimer()
    {
        timer += Time.deltaTime;
        if (timer >= maxDuration)
        {
            Debug.Log("<color=red>Time out!</color>");
            FinishGame(false); // Fail due to timeout
        }
    }

    public void CheckResult()
    {
        float checkVal = parrySlider.value;
        bool success = false;

        // Check Perfect (Highest priority)
        if (checkVal >= perfectMin && checkVal <= perfectMax)
        {
            Debug.Log("<color=green>RESULT: PERFECT PARRY!</color>");
            success = true;
        }
        // Check Partly
        else if (checkVal >= partlyMin && checkVal <= partlyMax)
        {
            Debug.Log("<color=yellow>RESULT: PARTLY PARRY</color>");
            success = true; // Still counts as a success/block
        }
        // Miss
        else
        {
            Debug.Log("<color=red>RESULT: MISS!</color>");
            success = false;
        }

        FinishGame(success);
    }

    private void FinishGame(bool success)
    {
        isParrying = false;
        parryPanel.SetActive(false);
        onResultCallback?.Invoke(success); // Notify Fish
    }

    private void CalculateRanges()
    {
        float partlyWidth = 20f;
        partlyMin = Random.Range(0f, 100f - partlyWidth);
        partlyMax = partlyMin + partlyWidth;

        float center = (partlyMin + partlyMax) / 2f;
        float perfectWidth = 4f;
        perfectMin = center - (perfectWidth / 2f);
        perfectMax = center + (perfectWidth / 2f);
    }

    private void CreateZoneVisuals()
    {
        perfectZoneObj = CreateZoneImage("PerfectZone", perfectParrySprite, perfectMin, perfectMax);
        partlyZoneObj = CreateZoneImage("PartlyZone", partlyParrySprite, partlyMin, partlyMax);
    }

    private GameObject CreateZoneImage(string name, Sprite sprite, float minRange, float maxRange)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parrySlider.transform, false);
        go.transform.SetSiblingIndex(1);

        Image img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(minRange / 100f, 0f);
        rt.anchorMax = new Vector2(maxRange / 100f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    private void CleanupOldVisuals()
    {
        if (partlyZoneObj != null) Destroy(partlyZoneObj);
        if (perfectZoneObj != null) Destroy(perfectZoneObj);
    }

}