using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideHitSMB : CharHitSMBBase {
    protected override Transform generalHitRefPoint => animUtils.refPoint_SlideHit;

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        if (animUtils.model.currentLocation != LifeEnum.Location.Wall) {
            animUtils.SetDefaultFace ();
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
