using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharWalkSMB : CharMovementSMBBase {
    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateHorizontalVelocity ();
        UpdateFacingDirection ();

        // TODO : Jump charge
    }
}