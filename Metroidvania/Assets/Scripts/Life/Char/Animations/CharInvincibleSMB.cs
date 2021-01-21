using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInvincibleSMB : CharSMBBase {
    private const float MaxAlpha = 0.3f;
    private const float MinAlpha = 0f;
    private const float HalfPeriod = 0.25f; // In second

    private bool isAlphaDecreasing = true;
    private float startAlphaChangeTime = -1;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isAlphaDecreasing = true;
        startAlphaChangeTime = Time.time;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        var deltaTime = Time.time - startAlphaChangeTime;

        var from = isAlphaDecreasing ? MaxAlpha : MinAlpha;
        var to = isAlphaDecreasing ? MinAlpha : MaxAlpha;
        var alpha = Mathf.Lerp (from, to, Mathf.Min(deltaTime / HalfPeriod, 1));
        animUtils.SetCharAlpha (alpha);

        if (deltaTime >= HalfPeriod) {
            startAlphaChangeTime = Time.time;
            isAlphaDecreasing = !isAlphaDecreasing;
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        animUtils.SetCharAlpha (1);
    }
}
