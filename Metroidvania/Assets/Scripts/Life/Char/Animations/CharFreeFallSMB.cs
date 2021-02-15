using UnityEngine;

public class CharFreeFallSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.UpdateVelocityX (null);
        AnimUtils.ResetGravity ();
    }
}
