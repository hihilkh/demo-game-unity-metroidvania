using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharJumpSMB : CharMovementSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        var jumpInitSpeed = model.isJumpCharged ? model.characterParams.chargeJumpInitSpeed : model.characterParams.normalJumpInitSpeed;

        rb.velocity = new Vector3 (rb.velocity.x, jumpInitSpeed);
        UpdateHorizontalVelocity ();    // Call this to prevent some case that rb.velocity.x is affected by Physics system at that moment (e.g. hit a wall and velocity.x become 0)
        ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateFacingDirection ();

        // TODO : Jump charge / cancel jump charge
        // TODO : Drop Hit charging
    }
}