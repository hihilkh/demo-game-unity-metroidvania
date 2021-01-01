using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharWalkSMB : CharSMBBase {
    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        animUtils.UpdateHorizontalVelocity ();
        animUtils.UpdateFacingDirection ();

        // TODO : Jump charge
    }
}