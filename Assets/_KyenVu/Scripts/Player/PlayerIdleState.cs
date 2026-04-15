using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {

        // Set magnitude to 0 to trigger the Idle blend/state
        player.animator.SetFloat("MoveMagnitude", 0f);
        player.rb.linearVelocity = Vector2.zero;
    }

    public override void UpdateState(PlayerStateManager player)
    {
        if (player.moveInput != Vector2.zero)
        {
            player.SwitchState(player.MoveState);
        }
    }

    public override void ExitState(PlayerStateManager player)
    {
    }
}