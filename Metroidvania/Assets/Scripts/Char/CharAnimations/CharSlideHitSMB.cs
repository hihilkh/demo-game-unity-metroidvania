using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideHitSMB : CharHitSMB {
    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        if (animUtils.model.currentLocation != CharEnum.Location.Wall) {
            animUtils.SetDefaultFace ();
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
