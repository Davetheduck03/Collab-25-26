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

    [Header("Scene Transition & UI")]
    public string returnSceneName = "Top Down scene";
    [Tooltip("Drag your fade screen UI Animator here.")]
    public Animator sceneTransitionAnimator;
    public float transitionDelay = 3f;

    // --- NEW: Confirmation Panel Reference ---
    [Tooltip("Drag your Return Confirmation UI Panel here.")]
    public GameObject returnConfirmationPanel;

    private Vector2 moveInput;
    private bool canMove = true;
    private bool isFishing = false;
    private bool isReturning = false;

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
        sceneTransitionAnimator.gameObject.SetActive(false);
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
       
        // Ensure the panel starts hidden
        if (returnConfirmationPanel != null)
            returnConfirmationPanel.SetActive(false);
    }

    private void Update()
    {
        if (isReturning) return;

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!isFishing)
            {
                // CHANGED: Prompt the UI instead of immediately loading
                PromptReturnToTown();
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
            // CHANGED: Also use the prompt for the Input System event
            PromptReturnToTown();
        }
    }

    public void PromptReturnToTown()
    {
        // If the panel is already open, do nothing
        if (returnConfirmationPanel != null && returnConfirmationPanel.activeSelf) return;

        // Freeze the boat while they decide
        canMove = false;
        rb.linearVelocity = Vector2.zero;

        if (returnConfirmationPanel != null)
        {
            returnConfirmationPanel.SetActive(true);
        }
        else
        {
            // Fallback: If you forgot to assign the panel, just go home!
            StartCoroutine(ReturnToTownSequence());
        }
    }

    public void ConfirmReturn()
    {
        if (returnConfirmationPanel != null)
            returnConfirmationPanel.SetActive(false);

        StartCoroutine(ReturnToTownSequence());
    }

    public void CancelReturn()
    {
        if (returnConfirmationPanel != null)
            returnConfirmationPanel.SetActive(false);

        // Unfreeze the boat!
        canMove = true;
    }

    // =========================================================================

    private void HandleFishCaught(GameObject caughtFish)
    {
        if (struggleCoroutine != null)
        {
            StopCoroutine(struggleCoroutine);
        }
        struggleCoroutine = StartCoroutine(RandomStruggleRoutine());
    }

    private void HandleFishingFinished(bool success, EnemySO caughtFishData)
    {
        if (struggleCoroutine != null)
        {
            StopCoroutine(struggleCoroutine);
        }

        StartCoroutine(FishingFinishedSequence(success, caughtFishData));
    }

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

        yield return new WaitForSeconds(1.5f);

        if (success && caughtFishData != null && CatchFishUI.Instance != null)
        {
            CatchFishUI.Instance.gameObject.SetActive(true);
            CatchFishUI.Instance.ShowCatchResult(caughtFishData);

            yield return new WaitUntil(() => !CatchFishUI.Instance.gameObject.activeSelf);
        }

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
        sceneTransitionAnimator.gameObject.SetActive(true);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Collider2D boatCollider = GetComponent<Collider2D>();
        if (boatCollider != null)
        {
            boatCollider.enabled = false;
        }

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

    public void PlaySplashAnimationEvent()
    {
        Water water = UnityEngine.Object.FindFirstObjectByType<Water>();

        if (water != null)
        {
            water.Ripple(transform.position, true, true, 0.6f);
        }
    }

    public void SpawnHookAnimationEvent()
    {
        Debug.Log("Hook Casted!");
        hook.SetActive(true);
        OnFishingStarted?.Invoke();
    }
}