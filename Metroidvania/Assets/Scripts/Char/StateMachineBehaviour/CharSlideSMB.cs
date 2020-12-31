using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSlideSMB : CharMovementSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animator.SetBool (CharAnimConstant.SlidingBoolName, true);

        // Slide down with constant speed
        rb.velocity = new Vector3 (0, model.characterParams.slideDownVelocity);
        RemoveGravity ();

        SetFace (CharEnum.FaceType.Normal_Inversed);
        UpdateFacingDirection (true);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        if (model.currentLocation != CharEnum.Location.Wall) {
            SetDefaultFace ();
            animator.SetBool (CharAnimConstant.SlidingBoolName, false);
        }
    }
}
