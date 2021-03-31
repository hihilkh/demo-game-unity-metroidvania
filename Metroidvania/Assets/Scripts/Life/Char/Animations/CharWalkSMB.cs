using UnityEngine;

public class CharWalkSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.ResetGravity ();

        AnimUtils.Model.PlayMovementSfx (AnimUtils.Model.Params.WalkAudioClip);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        AnimUtils.Model.StopMovementSfx (AnimUtils.Model.Params.WalkAudioClip);
    }
}