using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    private Vector2 moveInput;
    private bool canMove = true;
    private bool isFishing = false;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
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
            Debug.Log("Fishing");
            isFishing = true;
            canMove = false;
        }
    }

    private void OnFinishFish()
    {
        if (isFishing)
        {
            Debug.Log("Finished Fishing");
            isFishing = false;
            canMove = true;
        }
    }

    private void OnAttackLeft()
    {
        if (isFishing)
            Debug.Log("AttackLeft");
    }

    private void OnAttackRight()
    {
        if (isFishing)
            Debug.Log("AttackRight");
    }

    private void OnParry()
    {
        if (isFishing)
            Debug.Log("Parry");
    }
}
