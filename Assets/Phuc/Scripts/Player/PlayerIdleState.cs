using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        player.StateText.text = "Idle State";
        player.animator.SetBool("isRunning", false);
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
