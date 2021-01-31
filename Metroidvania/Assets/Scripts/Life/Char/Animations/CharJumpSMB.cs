using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharJumpSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.UpdateVelocity (null, animUtils.model.GetCurrentJumpInitSpeed ());
        animUtils.ResetGravity ();
    }
}