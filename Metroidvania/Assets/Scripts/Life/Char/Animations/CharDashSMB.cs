using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharDashSMB : CharSMBBase {

    // Simple trick to prevent turning off thruster PS while the next action is also Dash
    // (Next Dash's OnStateEnter would come earlier than current Dash's OnStateExit)
    private int dashCount = 0;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.SetVelocity (null, 0);
        animUtils.RemoveGravity ();

        if (dashCount == 0) {
            animUtils.SetThrusterPS (true);
        }

        dashCount++;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        animUtils.UpdateFacingDirection ();
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        dashCount--;

        if (dashCount <= 0) {
            animUtils.SetThrusterPS (false);
            dashCount = 0;
        }
    }
}