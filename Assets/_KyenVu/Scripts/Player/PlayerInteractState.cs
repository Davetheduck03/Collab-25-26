using UnityEngine;

public class PlayerInteractState : PlayerBaseState
{
    public override void EnterState(PlayerStateManager player)
    {
        player.StateText.text = "Interact State";
        player.animator.SetTrigger("interact");
        
        // Ensure the player completely stops moving
        player.rb.linearVelocity = Vector2.zero; 
        
        Debug.Log("Entered Interact State. Player is frozen until a UI panel closes.");
    }

    public override void UpdateState(PlayerStateManager player)
    {
        // We do absolutely nothing here! 
        // The player cannot move or exit until player.EndInteraction() is called.
    }

    public override void ExitState(PlayerStateManager player)
    {
        Debug.Log("Exiting Interact State.");
    }
}