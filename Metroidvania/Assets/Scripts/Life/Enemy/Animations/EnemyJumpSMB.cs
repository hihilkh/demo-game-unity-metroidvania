﻿using UnityEngine;

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
                    var velocity = AnimUtils.Model.Params.JumpInitVelocity;
                    if (AnimUtils.Model.FacingDirection == LifeEnum.HorizontalDirection.Left) {
                        velocity = Vector2.Scale (velocity, new Vector2 (-1, 1));
                    }
                    AnimUtils.RB.velocity = velocity;

                    AnimUtils.Model.AudioUtils.PlayJumpSfx ();
                }
            }
        }
    }
}