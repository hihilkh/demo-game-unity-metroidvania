using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLandingSMB : CharSMBBase {
    private bool isAnimFinished = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAnimFinished = false;

        animUtils.SetVelocity (null, 0);
        animUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // It may touch wall and change direction while landing, so it needs to update horizontal velocity and facing direction at OnStateUpdate
        animUtils.UpdateHorizontalVelocity ();
        animUtils.UpdateFacingDirection ();

        // TODO : cancel jump charge
        // TODO : cancel drop hit charge
        // TODO : cancel drop hit

        if (!isAnimFinished) {
            if (stateInfo.normalizedTime >= 1) {
                isAnimFinished = true;

                // Do not StartIdleOrWalk if there is next action, e.g., dash / free fall
                if (animator.GetNextAnimatorClipInfo (0).Length == 0) {
                    animUtils.model.StartIdleOrWalk ();
                }
            }
        }

    }
}
