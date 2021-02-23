using UnityEngine;

public class CharLandingSMB : CharSMBBase {
    private bool isAnimFinished = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAnimFinished = false;

        AnimUtils.UpdateVelocity (null, 0);
        AnimUtils.ResetGravity ();
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (!isAnimFinished) {
            if (stateInfo.normalizedTime >= 1) {
                isAnimFinished = true;

                if (AnimUtils.Model.CurrentLocation == LifeEnum.Location.Air) {
                    return;
                }

                if (AnimUtils.Model.GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                    return;
                }

                AnimUtils.Model.LandingFinishedAction ();
            }
        }
    }
}
