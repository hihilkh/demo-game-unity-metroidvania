using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharDashSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.UpdateHorizontalVelocity ();

        // TODO : VFX
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        animUtils.UpdateFacingDirection ();

    }
}