using Phuc.SoundSystem;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed;
    public Rigidbody2D rb;

    [Header("Fishing Settings")]
    public GameObject hook;

    [Header("Animation")]
    public Animator animator;

    [Header("Scene Settings")]
    public string returnSceneName = "Top Down scene";

    private Vector2 moveInput;
    private bool canMove = true;
    private bool isFishing = false;

    private Coroutine struggleCoroutine;

    public static event Action OnFishingStarted;

    private void OnEnable()
    {
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

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
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
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

            if (animator != null)
            {
                animator.SetBool("isMoving", moveInput.x != 0);
            }

            if (moveInput.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
                transform.localScale = scale;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
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

            if (animator != null)
            {
                animator.SetTrigger("Fish");
            }
            struggleCoroutine = StartCoroutine(RandomStruggleRoutine());

            OnFishingStarted?.Invoke();
        }
    }

    private void OnReturn()
    {
        if (!isFishing)
        {
            ReturnToTown();
        }
    }

    // CHANGED: Now starts a Coroutine instead of finishing instantly
    private void HandleFishingFinished(bool success)
    {
        if (struggleCoroutine != null)
        {
            StopCoroutine(struggleCoroutine);
        }

        // Start the animation sequence!
        StartCoroutine(FishingFinishedSequence(success));
    }

    // NEW: The sequence that handles playing animations in order
    private IEnumerator FishingFinishedSequence(bool success)
    {
        Debug.Log("Fishing finished! Playing 'Done' animation...");

        if (animator != null)
        {
            animator.SetTrigger("Done");
        }

        // 1. Wait for the "Done" animation to finish playing
        // (Change 1.0f to exactly however many seconds your Done animation lasts!)
        yield return new WaitForSeconds(1.0f);

        if (animator != null)
        {
            if (success) animator.SetTrigger("Happy");
            else animator.SetTrigger("Mad");
        }

        // 2. Wait for the Happy or Mad animation to finish playing
        // (Change 1.5f to the length of your Happy/Mad animation)
        yield return new WaitForSeconds(1.5f);

        // 3. Finally give control back to the player
        isFishing = false;
        canMove = true;
        Debug.Log("Animations complete! Returning to boat control.");
    }

    private IEnumerator RandomStruggleRoutine()
    {
        while (isFishing)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

            if (isFishing && animator != null)
            {
                animator.SetTrigger("Struggle");
            }
        }
    }

    private void ReturnToTown()
    {
        Debug.Log($"Returning to {returnSceneName}. Fish data is safe in the InventoryController!");
        SceneManager.LoadScene(returnSceneName);
    }

    public void PlaySplashAnimationEvent()
    {
       Water water = UnityEngine.Object.FindFirstObjectByType<Water>();

        if (water != null)
        {
            water.Ripple(transform.position, true, true, 0.6f);
        }
        else
        {
            Debug.LogWarning("[BoatController] Tried to play splash animation, but no Water was found in the scene!");
        }
    }
}