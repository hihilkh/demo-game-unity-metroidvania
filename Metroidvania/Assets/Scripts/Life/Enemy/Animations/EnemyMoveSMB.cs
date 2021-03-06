using HihiFramework.Core;
using UnityEngine;

public class EnemyMoveSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.Model.SetCheckChasingStatus (true);

        if (AnimUtils.Model.MovementType == EnemyEnum.MovementType.Walking) {
            AnimUtils.UpdateHorizontalVelocity ();
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        AnimUtils.Model.SetCheckChasingStatus (false);
    }
}
