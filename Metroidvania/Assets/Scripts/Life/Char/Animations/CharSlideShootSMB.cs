using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideShootSMB : CharGeneralShootSMB {
    protected override Transform shootRefPoint => animUtils.refPoint_SlideShoot;

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (animator.GetBool (CharAnimConstant.SlidingBoolName)) {
            if (!animUtils.model.GetIsInStatus (CharEnum.Status.Sliding)) {
                Debug.LogError ("CharSlideShootSMB");
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
