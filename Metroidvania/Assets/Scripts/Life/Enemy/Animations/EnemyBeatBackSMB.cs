using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBeatBackSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.rb.velocity = animUtils.model.beatBackDirection * animUtils.model.enemyParams.beatBackInitSpeed;
    }
}
