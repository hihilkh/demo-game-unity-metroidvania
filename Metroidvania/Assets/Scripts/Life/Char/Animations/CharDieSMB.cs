using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharDieSMB : CharSMBBase {
    private float startDieTime = -1;
    private bool hasFinishedDying = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        animator.speed = 0;
        animUtils.rb.bodyType = RigidbodyType2D.Static;
        startDieTime = Time.time;
        hasFinishedDying = false;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (hasFinishedDying) {
            return;
        }

        var deltaTime = Time.time - startDieTime;
        var period = animUtils.model.GetParams ().dyingPeriod;

        var alpha = Mathf.Lerp (1, 0, Mathf.Min (deltaTime / period, 1));
        animUtils.SetCharAlpha (alpha);

        if (deltaTime >= period) {
            hasFinishedDying = true;
        }
    }
}
