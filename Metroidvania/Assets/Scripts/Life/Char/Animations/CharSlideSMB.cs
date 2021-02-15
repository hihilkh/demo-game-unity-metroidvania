using UnityEngine;

public class CharSlideSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);
        
        AnimUtils.UpdateVelocity (0, AnimUtils.Model.Params.SlideDownVelocity);
        AnimUtils.RemoveGravity ();     // Slide down with constant speed

        animator.SetBool (CharAnimConstant.SlidingBoolName, true);
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (animator.GetBool (CharAnimConstant.SlidingBoolName)) {
            if (!AnimUtils.Model.GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                animator.SetBool (CharAnimConstant.SlidingBoolName, false);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (!AnimUtils.Model.GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
