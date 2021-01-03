using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharIdleSMB : CharSMBBase {
    // Remarks : As it is idle, just update the movement velocity and facing direction while enter
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.UpdateHorizontalVelocity ();
        animUtils.UpdateFacingDirection ();
        animUtils.ResetGravity ();
    }
}