using HihiFramework.Core;
using UnityEngine;

public class CharSlideHitSMB : CharHitSMBBase {
    protected override Transform GeneralHitRefPoint => AnimUtils.RefPoint_SlideHit;

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // Set Sliding Bool for animation
        if (animator.GetBool (CharAnimConstant.SlidingBoolName)) {
            if (!AnimUtils.Model.GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                Log.PrintDebug ("Set sliding bool of animator to false while slide hitting.", LogTypes.Animation | LogTypes.Char);
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
