using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharJumpSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.SetVelocity (null, animUtils.model.GetCurrentJumpInitSpeed ());
        animUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // It is to prevent the case that after Jump animation started,
        // it first touch wall and then leave ground
        // (so that it does not trigger sliding and will jump vertically because physics make the horizontal velocity to be zero)
        animUtils.UpdateHorizontalVelocity ();

        animUtils.UpdateFacingDirection ();
    }
}