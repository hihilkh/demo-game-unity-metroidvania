using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharJumpSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.SetVelocity (null, animUtils.model.GetCurrentJumpInitSpeed ());
        animUtils.UpdateHorizontalVelocity ();    // Call this to prevent some case that rb.velocity.x is affected by Physics system at that moment (e.g. hit a wall and velocity.x become 0)
        animUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        animUtils.UpdateFacingDirection ();

        // TODO : Jump charge / cancel jump charge
        // TODO : Drop Hit charging
    }
}