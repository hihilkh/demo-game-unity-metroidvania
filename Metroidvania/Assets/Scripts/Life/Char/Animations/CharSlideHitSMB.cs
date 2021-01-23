using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideHitSMB : CharHitSMBBase {
    protected override Transform generalHitRefPoint => animUtils.refPoint_SlideHit;

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (animator.GetBool (CharAnimConstant.SlidingBoolName)) {
            if (!animUtils.model.GetIsInStatus (CharEnum.Status.Sliding)) {
                Debug.LogError ("CharSlideHitSMB");
                animator.SetBool (CharAnimConstant.SlidingBoolName, false);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (!animUtils.model.GetIsInStatus (CharEnum.Status.Sliding)) {
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
