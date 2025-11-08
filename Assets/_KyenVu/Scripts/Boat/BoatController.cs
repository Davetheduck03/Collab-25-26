using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Fishing Settings")]
    public GameObject hook;

    private Vector2 moveInput;
    private bool canMove = true;
    private bool isFishing = false;

    // Event to notify that fishing has started
    public static event Action OnFishingStarted;

    private void OnEnable()
    {
        // Listen for finish event from CastLineControl
        CastLineControl.OnFishingFinished += HandleFishingFinished;
    }

    private void OnDisable()
    {
        CastLineControl.OnFishingFinished -= HandleFishingFinished;
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        hook.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (canMove)
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void OnBoatMove(InputValue value)
    {
        if (canMove)
            moveInput = value.Get<Vector2>();
    }

    private void OnFish()
    {
        if (!isFishing)
        {
            Debug.Log(" Started Fishing");
            hook.SetActive(true);
            isFishing = true;
            canMove = false;
            rb.linearVelocity = Vector2.zero;
            // Notify others (like CastLineControl) that fishing has started
            OnFishingStarted?.Invoke();
        }
    }

    private void HandleFishingFinished()
    {
        Debug.Log("Fishing finished! Returning to boat control.");
        isFishing = false;
        canMove = true;
    }
}
