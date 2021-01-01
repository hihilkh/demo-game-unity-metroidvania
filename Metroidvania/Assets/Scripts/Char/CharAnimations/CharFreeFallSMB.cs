using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharFreeFallSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.SetVelocity (null, 0);
        animUtils.UpdateHorizontalVelocity ();    // Call this to prevent some case that rb.velocity.x is affected by Physics system at that moment (e.g. hit a wall and velocity.x become 0)
        animUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        animUtils.UpdateFacingDirection ();
    }
}
