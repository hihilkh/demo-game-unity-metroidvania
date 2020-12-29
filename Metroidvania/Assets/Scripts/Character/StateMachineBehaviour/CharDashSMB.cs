using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharDashSMB : CharMovementSMBBase {
    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateHorizontalVelocity ();
        UpdateFacingDirection ();

        // TODO : VFX
    }
}