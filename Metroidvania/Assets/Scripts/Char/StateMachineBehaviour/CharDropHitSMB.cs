using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharDropHitSMB : CharMovementSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        rb.velocity = new Vector3 (rb.velocity.x, model.characterParams.dropHitVelocity);
        RemoveGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateHorizontalVelocity ();

        // DropHitting would not allow any other user input, so no need to update facing direction
        // UpdateFacingDirection ();
    }
}
