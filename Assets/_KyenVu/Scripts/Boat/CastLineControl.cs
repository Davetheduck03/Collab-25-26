using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Phuc.SoundSystem;

[RequireComponent(typeof(LineRenderer))]
public class CastLineControl : MonoBehaviour
{
    [Header("Fishing Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Line Settings")]
    public float maxLineLength = 10f;
    public float maxHorizontalRange = 3f;
    public float lineSpeed = 2f;
    public float naturalSinkRate = 0.5f;

    [Header("Auto-Pull Settings")]
    public float pullSpeed = 4f;
    public float bobAmount = 0.1f;
    public float bobSpeed = 3f;

    [Header("References")]
    public GameObject hook;
    public Transform lineOrigin;
    private LineRenderer lineRenderer;

    // =========================================================
    // --- NEW: VISUAL LINE SETTINGS ---
    // =========================================================
    [Header("Line Visual Settings")]
    [Tooltip("How many points make up the line. Higher = smoother underwater curves!")]
    public int lineResolution = 20;

    [Tooltip("Drag an empty GameObject here placed at the exact Y-level of your water surface.")]
    public Transform waterSurfacePoint;

    public float waveAmplitude = 0.15f;  // How wide the wave is
    public float waveSpeed = 4f;         // How fast it wiggles
    public float waveFrequency = 2f;     // How many S-curves the line has

    private Vector2 moveInput;
    private bool isFishing = false;
    private bool isCatching = false;
    private bool isSinking = false;
    private bool isPulling = false;
    private bool fishIsDead = false;

    private GameObject caughtFish;
    private float currentLineLength = 0f;
    public float CurrentDepth => currentLineLength;
    private float currentHorizontalOffset = 0f;

    private bool autoReeling = false;
    private float targetPullLength = 0f;
    private float baseCatchLength = 0f;

    [SerializeField] DamageComponent damageComponent;

    public static event Action<bool> OnFishingFinished;
    public static event Action<GameObject> OnFishCaught;
    public static event Action<DamageComponent> OnPlayerAttackLeft;
    public static event Action<DamageComponent> OnPlayerAttackRight;
    public static event Action OnPlayerParry;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // Set the resolution immediately
        lineRenderer.positionCount = Mathf.Max(2, lineResolution);
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        BoatController.OnFishingStarted += HandleFishingStarted;
        Fish.OnFishHealthThresholdReached += AutoPullTrigger;
        Fish.OnFishDefeatedEvent += HandleFishDefeated;
    }

    private void OnDisable()
    {
        BoatController.OnFishingStarted -= HandleFishingStarted;
        Fish.OnFishHealthThresholdReached -= AutoPullTrigger;
        Fish.OnFishDefeatedEvent -= HandleFishDefeated;
    }

    private void HandleFishingStarted()
    {
        isFishing = true;
        isCatching = false;
        autoReeling = false;
        fishIsDead = false;
        currentLineLength = 0f;
        currentHorizontalOffset = 0f;

        isPulling = false;
        isSinking = false;
        moveInput = Vector2.zero;

        hook.SetActive(true);
        hook.transform.position = lineOrigin.position;
        lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (!isFishing) return;

        HandleLineMovement();
        UpdateHookPosition();

        // CHANGED: This now runs our complex wave math!
        UpdateLineRenderer();
    }

    private void HandleLineMovement()
    {
        if (isCatching)
        {
            if (!fishIsDead)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
                currentLineLength += bob * Time.deltaTime;
            }

            if (autoReeling)
            {
                currentLineLength = Mathf.MoveTowards(currentLineLength, targetPullLength, pullSpeed * Time.deltaTime);

                if (Mathf.Abs(currentLineLength - targetPullLength) < 0.05f)
                {
                    autoReeling = false;
                    if (fishIsDead) FinishFishing(true);
                }
            }
            currentLineLength = Mathf.Clamp(currentLineLength, 0f, maxLineLength);
            return;
        }

        if (!isCatching)
        {
            currentHorizontalOffset += moveInput.x * moveSpeed * Time.deltaTime;
            currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxHorizontalRange, maxHorizontalRange);

            if (isPulling)
            {
                currentLineLength -= lineSpeed * Time.deltaTime;

                if (currentLineLength <= 0f)
                {
                    currentLineLength = 0f;
                    FinishFishing(false);
                    return;
                }
            }
            else if (isSinking)
            {
                currentLineLength += lineSpeed * Time.deltaTime;
            }
            else
            {
                currentLineLength += naturalSinkRate * Time.deltaTime;
            }

            currentLineLength = Mathf.Clamp(currentLineLength, 0f, maxLineLength);
            return;
        }
    }

    private void UpdateHookPosition()
    {
        Vector3 targetPos =
            lineOrigin.position +
            Vector3.right * currentHorizontalOffset +
            Vector3.down * currentLineLength;

        hook.transform.position = targetPos;
    }

    private void UpdateLineRenderer()
    {
        int points = Mathf.Max(2, lineResolution);
        if (lineRenderer.positionCount != points)
        {
            lineRenderer.positionCount = points;
        }

        Vector3 startPos = lineOrigin.position;
        Vector3 endPos = hook.transform.position;

        // Figure out where the water is (fallback to 1 unit below the rod if unassigned)
        float waterY = waterSurfacePoint != null ? waterSurfacePoint.position.y : startPos.y - 1f;

        for (int i = 0; i < points; i++)
        {
            // Get a percentage from 0.0 (top of the line) to 1.0 (bottom at the hook)
            float t = i / (float)(points - 1);

            // Start by assuming the line is perfectly straight
            Vector3 currentPoint = Vector3.Lerp(startPos, endPos, t);

            // If this specific point of the line is UNDER the water surface, make it wavy!
            if (currentPoint.y < waterY)
            {
                float depth = waterY - currentPoint.y;

                // 1. Create the wiggling sine wave
                float waveOffset = Mathf.Sin((Time.time * waveSpeed) - (depth * waveFrequency)) * waveAmplitude;

                // 2. Smoothly taper the wave at the water surface so it doesn't cleanly "snap"
                float surfaceTaper = Mathf.Clamp01(depth * 2f);

                // 3. Smoothly taper the wave at the bottom so it always perfectly connects to the hook!
                float distanceToHook = Vector3.Distance(currentPoint, endPos);
                float hookTaper = Mathf.Clamp01(distanceToHook * 2f);

                // Apply the wave horizontally
                currentPoint.x += (waveOffset * surfaceTaper * hookTaper);
            }

            // Lock the point into the LineRenderer
            lineRenderer.SetPosition(i, currentPoint);
        }
    }

    private void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    private void OnSink(InputValue value) { isSinking = value.isPressed; }
    private void OnPull(InputValue value) { isPulling = value.isPressed; }

    private void OnAttackLeft()
    {
        if (!isCatching) return;
        OnPlayerAttackLeft?.Invoke(damageComponent);
    }

    private void OnAttackRight()
    {
        if (!isCatching) return;
        OnPlayerAttackRight?.Invoke(damageComponent);
    }

    private void OnParry()
    {
        if (!isCatching) return;
        OnPlayerParry?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFishing) return;

        if (collision.CompareTag("Fish") && !isCatching)
        {
            isCatching = true;
            caughtFish = collision.gameObject;
            baseCatchLength = currentLineLength;

            caughtFish.transform.SetParent(hook.transform);
            OnFishCaught?.Invoke(caughtFish);
        }
    }

    private void AutoPullTrigger(float hpPercent)
    {
        if (!isCatching) return;
        float percentage = 1f - hpPercent;
        targetPullLength = baseCatchLength * (1f - percentage);
        autoReeling = true;
    }

    private void HandleFishDefeated()
    {
        fishIsDead = true;
        targetPullLength = 0f;
        autoReeling = true;
    }

    public void FinishFishing(bool success)
    {
        isFishing = false;
        isCatching = false;
        autoReeling = false;
        fishIsDead = false;

        hook.SetActive(false);
        lineRenderer.enabled = false;

        if (caughtFish != null)
        {
            caughtFish.transform.SetParent(null);
            Destroy(caughtFish);
            caughtFish = null;
        }

        if (TimeManager.Instance != null)
        {
            int minutesTaken = success ? 30 : 60;
            TimeManager.Instance.AdvanceTime(minutesTaken);
        }

        OnFishingFinished?.Invoke(success);
    }
}