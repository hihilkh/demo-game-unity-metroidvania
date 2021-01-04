using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharHitSMBBase : CharSMBBase {
    protected virtual Transform startHitRefPoint => animUtils.refPoint_GroundHit;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        var clone = GameObject.Instantiate<CharNormalHit> (animUtils.normalHitTemplate);
        clone.StartAttack (startHitRefPoint.position, animUtils.model.facingDirection, animUtils.GetVelocityXByCurrentHorizontalSpeed(true));
    }
}