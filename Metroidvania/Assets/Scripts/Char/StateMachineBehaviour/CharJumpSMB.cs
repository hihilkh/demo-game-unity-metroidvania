using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharJumpSMB : CharMovementSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        var jumpInitSpeed = model.isJumpCharged ? model.characterParams.chargeJumpInitSpeed : model.characterParams.normalJumpInitSpeed;

        rb.velocity = new Vector3 (rb.velocity.x, jumpInitSpeed);
        ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateHorizontalVelocity ();
        UpdateFacingDirection ();

        // TODO : Jump charge / cancel jump charge
        // TODO : Drop Hit charging
    }
}