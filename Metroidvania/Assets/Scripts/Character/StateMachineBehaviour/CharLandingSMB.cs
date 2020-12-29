using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharLandingSMB : CharMovementSMBBase {
    private bool isAnimFinished = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAnimFinished = false;

        rb.velocity = new Vector3 (rb.velocity.x, 0);
        ResetGravity ();
    }
    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        UpdateHorizontalVelocity ();
        UpdateFacingDirection ();

        // TODO : cancel jump charge
        // TODO : cancel drop hit charge
        // TODO : cancel drop hit

        if (!isAnimFinished) {
            if (stateInfo.normalizedTime >= 1) {
                isAnimFinished = true;

                if (model.currentHorizontalSpeed == CharEnum.HorizontalSpeed.Zero) {
                    model.StartIdling ();
                } else {
                    model.StartWalking ();
                }
            }
        }

    }
}
