using HihiFramework.Core;
using UnityEngine;

public class EnemyBeatBackSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.RB.velocity = AnimUtils.Model.BeatBackDirection * AnimUtils.Model.Params.BeatBackInitSpeed;

        if (AnimUtils.Model.MovementType == EnemyEnum.MovementType.Flying) {
            AnimUtils.StartUpdateToIdleVelocity ();
        }

        PlaySfx ();
    }

    protected virtual void PlaySfx () {
        AnimUtils.Model.AudioUtils.PlayBeatBackSfx ();
    }
}
