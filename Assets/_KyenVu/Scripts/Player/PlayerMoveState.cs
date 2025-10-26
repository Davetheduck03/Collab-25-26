using UnityEngine;

public class PlayerMoveState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        player.StateText.text = "Move State";
        player.animator.SetBool("isRunning", true);
    }

    public override void UpdateState(PlayerStateManager player)
    {
        Vector2 input = player.moveInput;

        // Move character using Rigidbody2D
        player.rb.linearVelocity = input.normalized * player.moveSpeed;

        // Flip character sprite based on direction
        if (input.x != 0)
            player.animator.transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);

        // Switch back to Idle if no input
        if (input == Vector2.zero)
        {
            player.SwitchState(player.IdleState);
        }
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.animator.SetBool("isRunning", false);
        player.rb.linearVelocity = Vector2.zero;
    }
}
