using UnityEngine;

public class EnemyIdleSMB : EnemySMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.SetIdleVelocity ();
    }
}
