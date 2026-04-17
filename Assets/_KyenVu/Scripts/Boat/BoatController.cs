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
    [Tooltip("Drag your Return Confirmation UI Panel here.")]
    public GameObject returnConfirmationPanel;

    // ==========================================
    // --- NEW: COMPREHENSIVE AUDIO SETTINGS ---
    // ==========================================
    [Header("Audio: Sound Effects")]
    public SO_SFXEvent paddleSfx;
    public SO_SFXEvent castRodSfx;
    public SO_SFXEvent fishHookedSfx;
    public SO_SFXEvent boatCreakSfx;

    [Header("Audio: Character Voices")]
    public SO_SFXEvent charEffortSfx; // Grunt when casting
    public SO_SFXEvent charHappySfx;  // Cheer on catch
    public SO_SFXEvent charMadSfx;    // Sigh/groan on miss

    [Header("Audio: Music")]
    public SO_BGMEvent normalBgm;     // Peaceful water theme
    public SO_BGMEvent battleBgm;     // Intense fishing battle theme
    // ==========================================

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
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        hook.SetActive(false);
        sceneTransitionAnimator.gameObject.SetActive(false);
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (returnConfirmationPanel != null)
            returnConfirmationPanel.SetActive(false);
    }

    private void Start()
    {
        // Make sure normal music is playing when we load into the boat
        if (normalBgm != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(normalBgm);
        }
    }

    private void Update()
    {
        if (isReturning) return;

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (!isFishing) PromptReturnToTown();
            else Debug.Log("Can't return to town right now, you are fishing!");
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
            if (animator != null) animator.SetBool("isMoving", false);
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
            isFishing = true;
            canMove = false;
            rb.linearVelocity = Vector2.zero;

            if (animator != null) animator.SetTrigger("Fish");
        }
    }

    private void OnReturn()
    {
        if (!isFishing && !isReturning) PromptReturnToTown();
    }

    public void PromptReturnToTown()
    {
        if (returnConfirmationPanel != null && returnConfirmationPanel.activeSelf) return;

        canMove = false;
        rb.linearVelocity = Vector2.zero;

        if (returnConfirmationPanel != null) returnConfirmationPanel.SetActive(true);
        else StartCoroutine(ReturnToTownSequence());
    }

    public void ConfirmReturn()
    {
        if (returnConfirmationPanel != null) returnConfirmationPanel.SetActive(false);
        StartCoroutine(ReturnToTownSequence());
    }

    public void CancelReturn()
    {
        if (returnConfirmationPanel != null) returnConfirmationPanel.SetActive(false);
        canMove = true;
    }

    // ==========================================
    // FISHING EVENT HANDLERS
    // ==========================================

    private void HandleFishCaught(GameObject caughtFish)
    {
        if (struggleCoroutine != null) StopCoroutine(struggleCoroutine);
        struggleCoroutine = StartCoroutine(RandomStruggleRoutine());

        // Audio: Fish hooked splash & instant Battle Theme!
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(fishHookedSfx);
            if (battleBgm != null) SoundManager.Instance.PlayBGM(battleBgm);
        }
    }

    private void HandleFishingFinished(bool success, EnemySO caughtFishData)
    {
        if (struggleCoroutine != null) StopCoroutine(struggleCoroutine);
        StartCoroutine(FishingFinishedSequence(success, caughtFishData));
    }

    private IEnumerator FishingFinishedSequence(bool success, EnemySO caughtFishData)
    {
        if (animator != null) animator.SetTrigger("Done");

        yield return new WaitForSeconds(1.0f);

        // Audio & Animation based on Success or Failure
        if (animator != null)
        {
            if (success)
            {
                animator.SetTrigger("Happy");
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySfx(charHappySfx); // Cheer!
                    SoundManager.Instance.PlaySfx(boatCreakSfx); // Jump on boat
                }
            }
            else
            {
                animator.SetTrigger("Mad");
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySfx(charMadSfx);   // Groan/Sigh
                    SoundManager.Instance.PlaySfx(boatCreakSfx); // Stomp on boat
                }
            }
        }

        yield return new WaitForSeconds(1.5f);

        // Audio: Return to normal peaceful music after the struggle is over
        if (SoundManager.Instance != null && normalBgm != null)
        {
            SoundManager.Instance.PlayBGM(normalBgm);
        }

        if (success && caughtFishData != null && CatchFishUI.Instance != null)
        {
            CatchFishUI.Instance.gameObject.SetActive(true);
            CatchFishUI.Instance.ShowCatchResult(caughtFishData);
            yield return new WaitUntil(() => !CatchFishUI.Instance.gameObject.activeSelf);
        }

        isFishing = false;
        canMove = true;
    }

    private IEnumerator RandomStruggleRoutine()
    {
        while (isFishing)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 5f));

            if (isFishing && animator != null)
            {
                animator.SetTrigger("Struggle");
                SoundManager.Instance?.PlaySfx(charEffortSfx);
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
        if (boatCollider != null) boatCollider.enabled = false;

        if (animator != null) animator.speed = 0f;
        if (sceneTransitionAnimator != null) sceneTransitionAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(transitionDelay);

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Splash(Clone)") Destroy(obj);
        }

        SceneManager.LoadScene(returnSceneName);
    }

    // ==========================================
    // ANIMATION EVENTS
    // ==========================================

    public void PlaySplashAnimationEvent()
    {
        Water water = UnityEngine.Object.FindFirstObjectByType<Water>();
        if (water != null) water.Ripple(transform.position, true, true, 0.6f);

        // ==========================================
        // --- NEW: Play the sound exactly when the splash happens! ---
        // ==========================================
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(paddleSfx);
        }
    }

    public void SpawnHookAnimationEvent()
    {
        hook.SetActive(true);
        OnFishingStarted?.Invoke();

        // Audio: Cast rod swish & character effort grunt
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySfx(castRodSfx);
            SoundManager.Instance.PlaySfx(charEffortSfx);
        }
    }
}