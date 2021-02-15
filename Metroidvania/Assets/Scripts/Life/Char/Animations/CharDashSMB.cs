using UnityEngine;

public class CharDashSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.UpdateVelocity (null, 0);
        AnimUtils.RemoveGravity ();
    }
}