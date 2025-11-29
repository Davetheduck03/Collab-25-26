using UnityEngine;
using UnityEngine.InputSystem;
using System;

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
    public float pullSpeed = 4f; // speed when auto pulling
    public float bobAmount = 0.1f;
    public float bobSpeed = 3f;

    [Header("References")]
    public GameObject hook;
    public Transform lineOrigin;
    private LineRenderer lineRenderer;

    private Vector2 moveInput;
    private bool isFishing = false;
    private bool isCatching = false;
    private bool isSinking = false;
    private bool isPulling = false;

    private GameObject caughtFish;

    private float currentLineLength = 0f;
    private float currentHorizontalOffset = 0f;

    private bool autoReeling = false;
    private float targetPullLength = 0f;

    // Store line length at moment of catch
    private float baseCatchLength = 0f;

    [SerializeField] DamageComponent damageComponent;

    // Events
    public static event Action OnFishingFinished;
    public static event Action OnFishCaught;
    public static event Action<DamageComponent> OnPlayerAttackLeft;
    public static event Action<DamageComponent> OnPlayerAttackRight;
    public static event Action OnPlayerParry;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        BoatController.OnFishingStarted += HandleFishingStarted;
        Fish.OnFishHealthThresholdReached += AutoPullTrigger;
    }

    private void OnDisable()
    {
        BoatController.OnFishingStarted -= HandleFishingStarted;
        Fish.OnFishHealthThresholdReached -= AutoPullTrigger;
    }

    private void HandleFishingStarted()
    {
        isFishing = true;
        isCatching = false;
        autoReeling = false;
        currentLineLength = 0f;
        currentHorizontalOffset = 0f;

        hook.SetActive(true);
        hook.transform.position = lineOrigin.position;
        lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (!isFishing) return;

        HandleLineMovement();
        UpdateHookPosition();
        UpdateLineRenderer();

        if (isCatching && caughtFish != null && currentLineLength <= 0.05f)
        {
            FinishFishing();
        }
    }

    private void HandleLineMovement()
    {
        // While fighting disable player sink/pull
        if (isCatching)
        {
            // Bobbing motion
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            currentLineLength += bob * Time.deltaTime;

            // Auto reel if needed
            if (autoReeling)
            {
                currentLineLength = Mathf.MoveTowards(currentLineLength, targetPullLength, pullSpeed * Time.deltaTime);

                if (Mathf.Abs(currentLineLength - targetPullLength) < 0.05f)
                    autoReeling = false;
            }

            currentLineLength = Mathf.Clamp(currentLineLength, 0f, maxLineLength);
            return;
        }

        // Normal fishing movement
        if (!isCatching)
        {
            // horizontal movement
            currentHorizontalOffset += moveInput.x * moveSpeed * Time.deltaTime;
            currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxHorizontalRange, maxHorizontalRange);

            // vertical movement
            if (isPulling)
            {
                currentLineLength -= lineSpeed * Time.deltaTime;
            }
            else if (isSinking)
            {
                currentLineLength += lineSpeed * Time.deltaTime;
            }
            else
            {
                currentLineLength += naturalSinkRate * Time.deltaTime; // natural slow sink
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
        lineRenderer.SetPosition(0, lineOrigin.position);
        lineRenderer.SetPosition(1, hook.transform.position);
    }

    private void OnMove(InputValue value)
    {
        if (!isFishing || isCatching) return;
        moveInput = value.Get<Vector2>();
    }

    private void OnSink(InputValue value)
    {
        if (!isFishing || isCatching) return;
        isSinking = value.isPressed;

    }

    private void OnPull(InputValue value)
    {
        if (!isFishing || isCatching) return;
        isPulling = value.isPressed;
    }

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

            OnFishCaught?.Invoke();
        }
    }

    // AUTO-PULL triggered from Fish when HP crosses thresholds
    private void AutoPullTrigger(float hpPercent)
    {
        if (!isCatching) return;

        float percentage = 1f - hpPercent;     // 20%, 40%, 60%, 80%
        targetPullLength = baseCatchLength * (1f - percentage);

        autoReeling = true;
    }

    public void FinishFishing()
    {
        isFishing = false;
        isCatching = false;
        autoReeling = false;

        hook.SetActive(false);
        lineRenderer.enabled = false;

        if (caughtFish != null)
            caughtFish.transform.SetParent(null);

        OnFishingFinished?.Invoke();
    }
}
