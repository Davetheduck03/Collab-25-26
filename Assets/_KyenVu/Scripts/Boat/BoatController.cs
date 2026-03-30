using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Phuc.SoundSystem;
using UnityEngine.SceneManagement; // NEW: Required for switching scenes

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public Rigidbody2D rb;

    [Header("Fishing Settings")]
    public GameObject hook;

    [Header("Scene Settings")]
    public string returnSceneName = "Top Down scene";

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

    // --- NEW: Check for the H key to return to town ---
    private void Update()
    {
        // Check if the H key was pressed this frame, and ensure we aren't currently fishing
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!isFishing)
            {
                ReturnToTown();
            }
            else
            {
                Debug.Log("Can't return to town right now, you are fishing!");
            }
        }
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
            // SoundManager.PlaySfx(SfxSoundType.Rod_casted);
            OnFishingStarted?.Invoke();
        }
    }

    // NEW: Optional method if you map a "Return" action in your PlayerInput component later
    private void OnReturn()
    {
        if (!isFishing)
        {
            ReturnToTown();
        }
    }

    private void HandleFishingFinished()
    {
        Debug.Log("Fishing finished! Returning to boat control.");
        isFishing = false;
        canMove = true;
    }

    // NEW: Method that actually loads the scene
    private void ReturnToTown()
    {
        Debug.Log($"Returning to {returnSceneName}. Fish data is safe in the InventoryController!");
        SceneManager.LoadScene(returnSceneName);
    }
}