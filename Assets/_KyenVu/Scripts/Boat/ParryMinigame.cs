using UnityEngine;
using UnityEngine.UI;

public class ParryMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject parryPanel;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private RectTransform yellowZone;
    [SerializeField] private RectTransform redZone;
    [SerializeField] private RectTransform indicator;

    [Header("Settings")]
    [SerializeField] private float indicatorSpeed = 600f;
    [SerializeField] private float barWidth = 500f;
    [SerializeField] private float yellowZoneWidth = 120f;
    [SerializeField] private float redZoneWidth = 40f;

    [Header("Positioning")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0); // Offset above the fish (in World Space units)
    private Transform targetToFollow;
    private Camera mainCamera;

    private bool isActive = false;
    private float indicatorX = 0f;
    private int direction = 1;
    private float targetCenterX = 0f;

    public enum Result { Miss, Normal, Perfect }

    private void Start()
    {
        mainCamera = Camera.main;
        if (parryPanel != null) parryPanel.SetActive(false);

        if (yellowZone) yellowZone.sizeDelta = new Vector2(yellowZoneWidth, yellowZone.sizeDelta.y);
        if (redZone) redZone.sizeDelta = new Vector2(redZoneWidth, redZone.sizeDelta.y);
    }

    private void LateUpdate()
    {
        if (!isActive) return;

        // 1. Logic: Move the Indicator
        MoveIndicator();

        // 2. Visuals: Follow the Target
        if (targetToFollow != null && mainCamera != null)
        {
            // Convert Fish world position to Screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetToFollow.position + offset);
            parryPanel.transform.position = screenPos;
        }
    }

    private void MoveIndicator()
    {
        indicatorX += direction * indicatorSpeed * Time.deltaTime;
        float limit = (barWidth / 2f);

        if (indicatorX >= limit) { indicatorX = limit; direction = -1; }
        else if (indicatorX <= -limit) { indicatorX = -limit; direction = 1; }

        indicator.anchoredPosition = new Vector2(indicatorX, 0);
    }

    // UPDATED: Now accepts a Transform target
    public void StartMinigame(Transform target)
    {
        isActive = true;
        targetToFollow = target; // Set the fish as the target

        if (parryPanel) parryPanel.SetActive(true);

        // Reset Mechanics
        indicatorX = 0f;
        direction = Random.Range(0, 2) == 0 ? 1 : -1;
        float maxOffset = (barWidth / 2f) - (yellowZoneWidth / 2f);
        targetCenterX = Random.Range(-maxOffset, maxOffset);

        if (yellowZone) yellowZone.anchoredPosition = new Vector2(targetCenterX, 0);
        if (redZone) redZone.anchoredPosition = new Vector2(targetCenterX, 0);
    }

    public void StopMinigame()
    {
        isActive = false;
        targetToFollow = null;
        if (parryPanel) parryPanel.SetActive(false);
    }

    public Result CheckParry()
    {
        if (!isActive) return Result.Miss;

        float distance = Mathf.Abs(indicatorX - targetCenterX);

        if (distance <= (redZoneWidth / 2f)) return Result.Perfect;
        else if (distance <= (yellowZoneWidth / 2f)) return Result.Normal;

        return Result.Miss;
    }
}