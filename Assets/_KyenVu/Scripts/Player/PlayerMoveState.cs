using Phuc.SoundSystem;
using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        player.StateText.text = "Move State";
    }

    public override void UpdateState(PlayerStateManager player)
    {
        Vector2 input = player.moveInput;

        // Move character using Rigidbody2D
        player.rb.linearVelocity = input.normalized * player.moveSpeed;

        // Update Blend Tree parameters
        if (input != Vector2.zero)
        {
            // Only update X and Y when there is input so the character stays facing the last moved direction when idle
            player.animator.SetFloat("MoveX", input.x);
            player.animator.SetFloat("MoveY", input.y);
        }

        // Update magnitude to drive the transition between Idle and Move animations
        player.animator.SetFloat("MoveMagnitude", input.magnitude);

        // Switch back to Idle if no input
        if (input == Vector2.zero)
        {
            player.SwitchState(player.IdleState);
        }
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.animator.SetFloat("MoveMagnitude", 0f);
        player.rb.linearVelocity = Vector2.zero;
    }
}