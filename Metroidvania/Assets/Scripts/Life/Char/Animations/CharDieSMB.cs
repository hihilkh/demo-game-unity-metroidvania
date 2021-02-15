using UnityEngine;

public class CharDieSMB : CharSMBBase {
    private float startDieTime = -1;
    private bool hasFinishedDying = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        AnimUtils.SetDieFeature (true);

        startDieTime = Time.time;
        hasFinishedDying = false;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (hasFinishedDying) {
            return;
        }

        if (!AnimUtils.Model.IsDying) {
            hasFinishedDying = true;
            return;
        }

        var deltaTime = Time.time - startDieTime;
        var period = AnimUtils.Model.Params.DyingPeriod;

        var alpha = Mathf.Lerp (1, 0, Mathf.Min (deltaTime / period, 1));
        AnimUtils.SetCharAlpha (alpha);

        if (deltaTime >= period) {
            hasFinishedDying = true;
        }
    }
}
