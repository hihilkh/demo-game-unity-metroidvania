using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharBeatBackSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.ResetGravity ();

        animUtils.rb.velocity = animUtils.model.beatBackDirection * animUtils.model.param.beatBackInitSpeed;
    }
}
