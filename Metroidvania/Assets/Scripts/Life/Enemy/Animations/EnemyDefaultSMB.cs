using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDefaultSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        // TODO : Flying movement type
        switch (animUtils.model.movementType) {
            case EnemyEnum.MovementType.Walking:
                animUtils.UpdateHorizontalVelocity ();
                break;
        }
    }
}
