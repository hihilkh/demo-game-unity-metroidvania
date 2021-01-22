﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideShootSMB : CharGeneralShootSMB {
    protected override Transform shootRefPoint => animUtils.refPoint_SlideShoot;

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        if (!animUtils.model.GetIsInStatus (CharEnum.Status.Sliding)) {
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
