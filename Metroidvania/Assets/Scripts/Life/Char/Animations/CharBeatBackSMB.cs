using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharBeatBackSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.ResetGravity ();

        animUtils.rb.velocity = animUtils.model.beatBackDirection * animUtils.model.GetParams ().beatBackInitSpeed;
        animUtils.SetFace (CharEnum.FaceType.Confused);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        animUtils.SetDefaultFace ();
    }
}
