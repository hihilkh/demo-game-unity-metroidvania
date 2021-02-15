using UnityEngine;

public class EnemyJumpSMB : EnemySMBBase {
    private bool isAnimFinished = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAnimFinished = false;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (!isAnimFinished) {
            if (stateInfo.normalizedTime >= 1) {
                isAnimFinished = true;

                if (animator.GetNextAnimatorClipInfo (0).Length == 0) {
                    AnimUtils.RB.velocity = new Vector2 (AnimUtils.RB.velocity.x, AnimUtils.Model.Params.JumpInitSpeed);
                }
            }
        }
    }
}