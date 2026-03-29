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

    private Vector2 moveInput;
    private bool isFishing = false;
    private bool isCatching = false;
    private bool isSinking = false;
    private bool isPulling = false;

    // NEW: Track if the fish is currently dead
    private bool fishIsDead = false;

    private GameObject caughtFish;

    private float currentLineLength = 0f;

    public float CurrentDepth => currentLineLength;
    private float currentHorizontalOffset = 0f;

    private bool autoReeling = false;
    private float targetPullLength = 0f;

    private float baseCatchLength = 0f;

    [SerializeField] DamageComponent damageComponent;

    // Events
    public static event Action OnFishingFinished;
    public static event Action<GameObject> OnFishCaught;
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
        // NEW: Listen for the fish death
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

        // NEW: Reset all inputs so the hook doesn't get stuck pulling up!
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
        UpdateLineRenderer();

        // Removed the old buggy FinishFishing() check from here
    }

    private void HandleLineMovement()
    {
        // While fighting
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

                    if (fishIsDead)
                    {
                        FinishFishing();
                    }
                }
            }

            currentLineLength = Mathf.Clamp(currentLineLength, 0f, maxLineLength);
            return;
        }

        // Normal fishing movement (seeking fish)
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
                    FinishFishing();
                    return; // Stop running movement logic for this frame
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
        lineRenderer.SetPosition(0, lineOrigin.position);
        lineRenderer.SetPosition(1, hook.transform.position);
    }

    private void OnMove(InputValue value)
    {
        
        moveInput = value.Get<Vector2>();
    }

    private void OnSink(InputValue value)
    {
        isSinking = value.isPressed;
    }

    private void OnPull(InputValue value)
    {
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
            // SoundManager.PlaySfx(SfxSoundType.Hooked_the_fish);
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

    // NEW: Method to handle when the fish dies
    private void HandleFishDefeated()
    {
        fishIsDead = true;
        targetPullLength = 0f; // Force the line to reel all the way to the top
        autoReeling = true;
    }

    public void FinishFishing()
    {
        isFishing = false;
        isCatching = false;
        autoReeling = false;
        fishIsDead = false;

        hook.SetActive(false);
        lineRenderer.enabled = false;

        // NEW: Unparent and permanently destroy the dead fish
        if (caughtFish != null)
        {
            caughtFish.transform.SetParent(null);
            Destroy(caughtFish);
            caughtFish = null;
        }

        // This event alerts BoatController to give movement back, 
        // and CameraManager to return the camera to the boat!
        OnFishingFinished?.Invoke();
    }
}