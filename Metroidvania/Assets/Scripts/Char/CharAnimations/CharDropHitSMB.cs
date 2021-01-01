using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharDropHitSMB : CharSMBBase {
    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animUtils.SetVelocity (0, animUtils.model.characterParams.dropHitVelocity);
        animUtils.RemoveGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        // DropHitting would not allow any other user input, so no need to update facing direction
        // UpdateFacingDirection ();
    }
}
