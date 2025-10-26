using UnityEngine;

public class PlayerInteractState : PlayerBaseState
{
    private bool isInteracting;

    public override void EnterState(PlayerStateManager player)
    {
        player.StateText.text = "Interact State";
        player.animator.SetTrigger("interact");
        isInteracting = true;

        Debug.Log("Entered Interact State");

        // Subscribe to event
        player.OnInteractPressed -= HandleInteract;
        player.OnInteractPressed += HandleInteract;
    }

    public override void UpdateState(PlayerStateManager player)
    {
        // While interacting, you can run logic like checking proximity or timers
        if (!isInteracting)
        {
            player.SwitchState(player.IdleState);
        }
    }

    public override void ExitState(PlayerStateManager player)
    {
        player.OnInteractPressed -= HandleInteract;
    }

    private void HandleInteract()
    {
        Debug.Log("Player pressed interact again inside InteractState");
        isInteracting = false;
    }
}
