using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharShootSMBBase : CharSMBBase {
    protected virtual Transform shootRefPoint => animUtils.refPoint_GeneralShoot;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        CharArrowBase arrowClone;
        Transform target = null;

        switch (animUtils.model.currentArrowType) {
            case CharEnum.ArrowType.Target:
                arrowClone = Instantiate (animUtils.targetArrowTemplate);
                target = animUtils.model.SearchShootTarget ();
                break;
            case CharEnum.ArrowType.Straight:
                arrowClone = Instantiate (animUtils.targetArrowTemplate);
                break;
            case CharEnum.ArrowType.Triple:
                arrowClone = Instantiate (animUtils.targetArrowTemplate);
                break;
            default:
                Log.PrintError ("currentArrowType = " + animUtils.model.currentHitType + " . No implementation in CharShootSMBBase. Please check.");
                return;
        }

        arrowClone.StartAttack (shootRefPoint, animUtils.model.facingDirection, target);
    }
}