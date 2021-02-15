using UnityEngine;

public class CharBeatBackSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.ResetGravity ();

        AnimUtils.RB.velocity = AnimUtils.Model.BeatBackDirection * AnimUtils.Model.Params.BeatBackInitSpeed;
    }
}
