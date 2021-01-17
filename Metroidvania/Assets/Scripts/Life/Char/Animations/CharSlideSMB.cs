using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animator.SetBool (CharAnimConstant.SlidingBoolName, true);

        
        animUtils.SetVelocity (0, animUtils.model.charParams.slideDownVelocity);
        animUtils.RemoveGravity ();     // Slide down with constant speed

        animUtils.SetFace (CharEnum.FaceType.Normal_Inversed);
        animUtils.UpdateFacingDirection (true);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        if (animUtils.model.currentLocation != LifeEnum.Location.Wall) {
            animUtils.SetDefaultFace ();
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
