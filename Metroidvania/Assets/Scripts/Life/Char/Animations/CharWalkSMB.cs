using UnityEngine;

public class CharWalkSMB : CharSMBBase {

    private const float PlayWalkSfxNormalizedPeriod = 0.5f;
    private const float PlayWalkSfxNormalizedOffset = 0.5f;
    private const float PlayWalkSfxNormalizedTolerantDelay = 0.1f;

    private float lastPlayedWalkSfxTime = -1;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.ResetGravity ();
        lastPlayedWalkSfxTime = -1;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (stateInfo.normalizedTime < lastPlayedWalkSfxTime + PlayWalkSfxNormalizedTolerantDelay) {
            return;
        }

        var normalizedTimeWithOffset = stateInfo.normalizedTime - PlayWalkSfxNormalizedOffset;

        if (normalizedTimeWithOffset < 0) {
            return;
        }

        var remainder = normalizedTimeWithOffset % PlayWalkSfxNormalizedPeriod;
        if (remainder < PlayWalkSfxNormalizedTolerantDelay) {
            AnimUtils.Model.AudioUtils.PlayWalkSfx ();
            lastPlayedWalkSfxTime = stateInfo.normalizedTime;
        }
    }
}