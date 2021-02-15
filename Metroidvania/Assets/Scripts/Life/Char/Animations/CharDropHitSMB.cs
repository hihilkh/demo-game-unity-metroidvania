using UnityEngine;

public class CharDropHitSMB : CharHitSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.UpdateVelocity (0, AnimUtils.Model.Params.DropHitVelocity);
        AnimUtils.RemoveGravity ();
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        HitClone?.DestroySelf ();
    }
}
