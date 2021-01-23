using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharWalkSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.ResetGravity ();
    }
}