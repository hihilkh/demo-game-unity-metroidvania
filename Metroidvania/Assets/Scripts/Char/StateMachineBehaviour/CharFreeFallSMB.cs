using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharFreeFallSMB : CharMovementSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        rb.velocity = new Vector3 (rb.velocity.x, 0);
        UpdateHorizontalVelocity ();    // Call this to prevent some case that rb.velocity.x is affected by Physics system at that moment (e.g. hit a wall and velocity.x become 0)
        ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateFacingDirection ();
    }
}
