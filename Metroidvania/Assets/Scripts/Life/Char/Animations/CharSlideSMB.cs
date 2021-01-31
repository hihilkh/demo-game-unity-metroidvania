using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);
        
        animUtils.UpdateVelocity (0, animUtils.model.param.slideDownVelocity);
        animUtils.RemoveGravity ();     // Slide down with constant speed

        animator.SetBool (CharAnimConstant.SlidingBoolName, true);
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (animator.GetBool (CharAnimConstant.SlidingBoolName)) {
            if (!animUtils.model.GetIsInStatus (CharEnum.Status.Sliding)) {
                Debug.LogError ("CharSlideSMB");
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
