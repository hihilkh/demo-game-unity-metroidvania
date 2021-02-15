using UnityEngine;

public class EnemyBeatBackSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.RB.velocity = AnimUtils.Model.BeatBackDirection * AnimUtils.Model.Params.BeatBackInitSpeed;
    }
}
