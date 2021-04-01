﻿using UnityEngine;

public class EnemyDieSMB : EnemyBeatBackSMB {
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

                AnimUtils.Model.DestroySelf (true);
            }
        }
    }

    protected override void PlaySfx () {
        AnimUtils.Model.AudioUtils.PlayDieSfx ();
    }
}