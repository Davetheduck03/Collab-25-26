using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateManager : MonoBehaviour
{
    public PlayerBaseState currentState;
    public PlayerIdleState IdleState = new PlayerIdleState();
    public PlayerMoveState MoveState = new PlayerMoveState();
    public PlayerInteractState InteractState = new PlayerInteractState();

    [Header("Input Settings")]
    public PlayerInput playerInput;
    [HideInInspector] public Vector2 moveInput;

    [Header("Movement")]
    public Rigidbody2D rb;
    public float moveSpeed = 5f;

    [Header("Animation")]
    public Animator animator;

    [Header("UI")]
    public TMP_Text StateText;

    [Header("Health Setting")]
    public int HP = 3;
    public int maxHP = 5;

    // Interaction
    [HideInInspector] public I_Interactable currentInteractable;
    public event Action OnInteractPressed;

    private void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
    }

    private void Start()
    {
        currentState = IdleState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        currentState.UpdateState(this);
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Move")
        {
            moveInput = context.ReadValue<Vector2>();
        }

        if (context.action.name == "Interact" /*&& context.performed*/)
        {
            OnInteractPressed?.Invoke();
            Debug.Log("Player pressed interact");

            if (currentInteractable != null)
            {
                currentInteractable.Interact();
                SwitchState(InteractState); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        I_Interactable interactable = collision.GetComponent<I_Interactable>();
        if (interactable != null)
        {
            Debug.Log("Player entered interactable: " + interactable);
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        I_Interactable interactable = collision.GetComponent<I_Interactable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable = null;
        }
    }

    public void SwitchState(PlayerBaseState state)
    {
        currentState.ExitState(this);
        currentState = state;
        currentState.EnterState(this);
    }
}
