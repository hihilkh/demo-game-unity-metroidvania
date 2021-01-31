using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLandingSMB : CharSMBBase {
    private bool isAnimFinished = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAnimFinished = false;

        animUtils.UpdateVelocity (null, 0);
        animUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (!isAnimFinished) {
            if (stateInfo.normalizedTime >= 1) {
                isAnimFinished = true;

                if (animUtils.model.currentLocation == LifeEnum.Location.Air) {
                    return;
                }

                if (animUtils.model.GetIsInStatus (CharEnum.Status.Dashing)) {
                    return;
                }

                animUtils.model.StartIdleOrWalk ();
            }
        }
    }
}
