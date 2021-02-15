using UnityEngine;

public class CharJumpSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.UpdateVelocity (null, AnimUtils.Model.GetCurrentJumpInitSpeed ());
        AnimUtils.ResetGravity ();
    }
}