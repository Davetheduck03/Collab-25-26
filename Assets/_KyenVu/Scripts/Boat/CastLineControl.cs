using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class CastLineControl : MonoBehaviour
{
    [Header("Fishing Movement Settings")]
    public float moveSpeed = 3f;

    [Header("Line Settings")]
    public float maxLineLength = 10f;     // Max depth (vertical)
    public float maxHorizontalRange = 3f; // Limit horizontal swing distance from origin
    public float lineSpeed = 2f;          // Normal sink/pull speed
    public float fastSinkMultiplier = 2f; // Sink faster when pressing S
    public float naturalSinkRate = 0.5f;  // Natural slow sink

    [Header("References")]
    public GameObject hook;       // Hook/bait to move
    public Transform lineOrigin;  // The point where the line starts (e.g., the boat)
    private LineRenderer lineRenderer;

    private Vector2 moveInput;
    private bool isFishing = false;
    private bool isCatching = false;
    private bool isSinking = false;
    private bool isPulling = false;
    private GameObject caughtFish;

    private float currentLineLength = 0f; // Vertical distance (depth)
    private float currentHorizontalOffset = 0f; // Sideways offset from origin

    // Event to notify BoatController when fishing ends
    public static event Action OnFishingFinished;

    public delegate void OnCaughtFish(); // pass fish data (fish type, etc) later will use for inventory
    public static event OnCaughtFish OnFishCaught;


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        // Setup default line appearance
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = false;
    }

    private void OnEnable()
    {
        BoatController.OnFishingStarted += HandleFishingStarted;
    }

    private void OnDisable()
    {
        BoatController.OnFishingStarted -= HandleFishingStarted;
    }

    private void HandleFishingStarted()
    {
        Debug.Log("Cast line! Fishing mode enabled.");
        isFishing = true;
        isCatching = false;
        currentLineLength = 0f;
        currentHorizontalOffset = 0f;
        caughtFish = null;

        if (hook != null && lineOrigin != null)
        {
            hook.SetActive(true);
            hook.transform.position = lineOrigin.position;
        }

        lineRenderer.enabled = true;
    }

    private void Update()
    {
        if (!isFishing) return;

        HandleLineMovement();
        UpdateHookPosition();
        UpdateLineRenderer();

        // Check if the caught fish has been reeled all the way up
        if (isCatching && caughtFish != null && currentLineLength <= 0.05f)
        {
            Debug.Log("Fish reeled in!");
            FinishFishing();
        }
    }

    private void HandleLineMovement()
    {
        // Horizontal movement
        currentHorizontalOffset += moveInput.x * moveSpeed * Time.deltaTime;
        currentHorizontalOffset = Mathf.Clamp(currentHorizontalOffset, -maxHorizontalRange, maxHorizontalRange);

        // Vertical control
        float speed = lineSpeed;

        if (isPulling)
            currentLineLength -= speed * Time.deltaTime;
            
        else if (isSinking)
            currentLineLength += speed * fastSinkMultiplier * Time.deltaTime;
        else
            currentLineLength += speed * naturalSinkRate * Time.deltaTime; // natural slow sink

        currentLineLength = Mathf.Clamp(currentLineLength, 0f, maxLineLength);
    }

    private void UpdateHookPosition()
    {
        if (hook == null || lineOrigin == null) return;

        Vector3 targetPos = lineOrigin.position +
                            Vector3.right * currentHorizontalOffset +
                            Vector3.down * currentLineLength;

        hook.transform.position = targetPos;
    }

    private void UpdateLineRenderer()
    {
        if (!lineRenderer.enabled || lineOrigin == null || hook == null) return;

        lineRenderer.SetPosition(0, lineOrigin.position);
        lineRenderer.SetPosition(1, hook.transform.position);
    }

    private void OnMove(InputValue value)
    {
        if (!isFishing) return;
        moveInput = value.Get<Vector2>();
    }

    private void OnSink(InputValue value)
    {
        if (!isFishing) return;
        isSinking = value.isPressed;
        Debug.Log("Sinking");
    }

    private void OnPull(InputValue value)
    {
        if (!isFishing) return;
        isPulling = value.isPressed;
        Debug.Log("Pulling");
    }

    private void OnAttackLeft()
    {
        if (!isCatching) return;
        Debug.Log("Attack Left!");
    }

    private void OnAttackRight()
    {
        if (!isCatching) return;
        Debug.Log("Attack Right!");
    }

    private void OnParry()
    {
        if (!isCatching) return;
        Debug.Log("Parry!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFishing) return;

        if (collision.CompareTag("Fish") && !isCatching)
        {
            Debug.Log("Fish caught on hook!");
            isCatching = true;
            caughtFish = collision.gameObject;

            // Attach fish to hook
            caughtFish.transform.SetParent(hook.transform);

            OnFishCaught?.Invoke();
        }
    }

    public void FinishFishing()
    {
        if (!isFishing) return;

        Debug.Log("Fishing session finished!");
        isFishing = false;
        moveInput = Vector2.zero;

        lineRenderer.enabled = false;
        hook.SetActive(false);

        OnFishingFinished?.Invoke();

        if (caughtFish != null)
        {
            // Optionally: disable or destroy fish after catching
            caughtFish.transform.SetParent(null);

            caughtFish = null;
        }
    }
}
