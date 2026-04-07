using Phuc.SoundSystem;
using System;
using System.Collections;
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

    [Header("Scene Transition")]
    public string returnSceneName = "Top Down scene";
    [Tooltip("Drag your fade screen UI Animator here.")]
    public Animator sceneTransitionAnimator;
    public float transitionDelay = 1f;

    private Vector2 moveInput;
    private bool canMove = true;
    private bool isFishing = false;
    private bool isReturning = false; // NEW: Prevents spamming

    private Coroutine struggleCoroutine;

    public static event Action OnFishingStarted;

    private void OnEnable()
    {
        CastLineControl.OnFishingFinished += HandleFishingFinished;
        CastLineControl.OnFishCaught += HandleFishCaught;
    }

    private void OnDisable()
    {
        CastLineControl.OnFishingFinished -= HandleFishingFinished;
        CastLineControl.OnFishCaught -= HandleFishCaught;
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
        // Don't do anything if we are already transitioning
        if (isReturning) return;

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!isFishing)
            {
                StartCoroutine(ReturnToTownSequence());
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
        if (!isFishing && !isReturning)
        {
            Debug.Log(" Started Fishing Animation");

            // Lock the boat and set the state
            isFishing = true;
            canMove = false;
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetTrigger("Fish");
            }
        }
    }

    private void OnReturn()
    {
        if (!isFishing && !isReturning)
        {
            StartCoroutine(ReturnToTownSequence());
        }
    }

    private void HandleFishCaught(GameObject caughtFish)
    {
        if (struggleCoroutine != null)
        {
            StopCoroutine(struggleCoroutine);
        }
        struggleCoroutine = StartCoroutine(RandomStruggleRoutine());
    }

    // 1. Update your Handle method signature
    private void HandleFishingFinished(bool success, EnemySO caughtFishData)
    {
        if (struggleCoroutine != null)
        {
            StopCoroutine(struggleCoroutine);
        }

        StartCoroutine(FishingFinishedSequence(success, caughtFishData));
    }

    // 2. Update your Coroutine to freeze when the UI is open
    private IEnumerator FishingFinishedSequence(bool success, EnemySO caughtFishData)
    {
        Debug.Log("Fishing finished! Playing 'Done' animation...");

        if (animator != null)
        {
            animator.SetTrigger("Done");
        }

        yield return new WaitForSeconds(1.0f);

        if (animator != null)
        {
            if (success) animator.SetTrigger("Happy");
            else animator.SetTrigger("Mad");
        }
        Debug.Log("--- FISHING UI CHECK ---");
        Debug.Log($"1. Was the catch a success? : {success}");
        Debug.Log($"2. Did we successfully grab the fish data? : {(caughtFishData != null ? "YES" : "NO (Data is null!)")}");
        Debug.Log($"3. Is the UI Singleton active? : {(CatchFishUI.Instance != null ? "YES" : "NO (Instance is null!)")}");
        yield return new WaitForSeconds(1.5f);
        if (success && caughtFishData != null && CatchFishUI.Instance != null)
        {
            // 1. Show the UI
            CatchFishUI.Instance.gameObject.SetActive(true);
            CatchFishUI.Instance.ShowCatchResult(caughtFishData);

            // 2. Freeze this coroutine right here until the UI closes itself!
            yield return new WaitUntil(() => !CatchFishUI.Instance.gameObject.activeSelf);
        }

        // 3. UI is closed, give the player back control
        isFishing = false;
        canMove = true;
        Debug.Log("Animations and UI complete! Returning to boat control.");
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

    private IEnumerator ReturnToTownSequence()
    {
        isReturning = true;
        canMove = false;

        // 1. Stop movement
        rb.linearVelocity = Vector2.zero;

        // 2. NEW: Disable physics so the boat cannot trigger water splashes when it gets destroyed
        rb.simulated = false;
        Collider2D boatCollider = GetComponent<Collider2D>();
        if (boatCollider != null)
        {
            boatCollider.enabled = false;
        }

        // 3. Freeze animator
        if (animator != null)
        {
            animator.speed = 0f;
        }

        Debug.Log("Starting scene transition...");

        if (sceneTransitionAnimator != null)
        {
            sceneTransitionAnimator.SetTrigger("FadeOut");
        }

        yield return new WaitForSeconds(transitionDelay);

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Splash(Clone)")
            {
                Destroy(obj);
            }
        }

        Debug.Log($"Returning to {returnSceneName}...");
        SceneManager.LoadScene(returnSceneName);
    }

    // =========================================================================
    // --- ANIMATION EVENTS ---
    // =========================================================================

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

    public void SpawnHookAnimationEvent()
    {
        Debug.Log("Hook Casted!");
        hook.SetActive(true);
        OnFishingStarted?.Invoke();
    }
}