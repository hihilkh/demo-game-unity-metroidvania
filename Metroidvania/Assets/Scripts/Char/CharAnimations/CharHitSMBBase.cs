using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharHitSMBBase : CharSMBBase {
    protected virtual Transform startHitRefPoint => animUtils.refPoint_GroundHit;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        CharHitBase clone;
        Vector3 startPos;

        switch (animUtils.model.currentHitType) {
            case CharEnum.HitType.Normal:
                clone = Instantiate (animUtils.normalHitTemplate);
                startPos = startHitRefPoint.position;
                break;
            case CharEnum.HitType.Charged:
                clone = Instantiate (animUtils.chargedHitTemplate);
                startPos = startHitRefPoint.position;
                break;
            case CharEnum.HitType.Finishing:
            case CharEnum.HitType.Drop:
            default:
                Log.PrintError ("currentHitType = " + animUtils.model.currentHitType + " . No implementation in CharHitSMBBase. Please check.");
                return;
        }

        clone.StartAttack (startPos, animUtils.model.facingDirection, animUtils.GetVelocityXByCurrentHorizontalSpeed (true));
    }
}