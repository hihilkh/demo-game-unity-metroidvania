using System.Collections;
using System.Collections.Generic;
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
                    animUtils.rb.velocity = new Vector2 (animUtils.rb.velocity.x, animUtils.model.GetParams ().jumpInitSpeed);
                }
            }
        }
    }
}