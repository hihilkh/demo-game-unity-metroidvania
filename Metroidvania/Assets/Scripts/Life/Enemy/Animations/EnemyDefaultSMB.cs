using UnityEngine;

public class EnemyDefaultSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        // TODO : Flying movement type
        switch (AnimUtils.Model.MovementType) {
            case EnemyEnum.MovementType.Walking:
                AnimUtils.UpdateHorizontalVelocity ();
                break;
        }
    }
}
