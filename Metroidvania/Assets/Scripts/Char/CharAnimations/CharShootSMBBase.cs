using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharShootSMBBase : CharSMBBase {
    protected virtual Transform shootRefPoint => animUtils.refPoint_GeneralShoot;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Transform target = null;

        switch (animUtils.model.currentArrowType) {
            case CharEnum.ArrowType.Target:
                var targetArrowClone = Instantiate (animUtils.targetArrowTemplate);
                target = animUtils.model.SearchShootTarget ();
                targetArrowClone.StartAttack (shootRefPoint, animUtils.model.facingDirection, target);
                break;
            case CharEnum.ArrowType.Straight:
                var straightArrowClone = Instantiate (animUtils.straightArrowTemplate);
                straightArrowClone.StartAttack (shootRefPoint, animUtils.model.facingDirection);
                break;
            case CharEnum.ArrowType.Triple:
                if (animUtils.model.charParams.tripleArrowShootingAngleList == null || animUtils.model.charParams.tripleArrowShootingAngleList.Count <= 0) {
                    Log.PrintError ("tripleArrowShootingAngleList has not yet set. Please check.");
                    return;
                }

                foreach (var angle in animUtils.model.charParams.tripleArrowShootingAngleList) {
                    var tripleArrowClone = Instantiate (animUtils.tripleArrowTemplate);
                    tripleArrowClone.StartAttack (shootRefPoint, animUtils.model.facingDirection, angle);
                }
                break;
            default:
                Log.PrintError ("currentArrowType = " + animUtils.model.currentHitType + " . No implementation in CharShootSMBBase. Please check.");
                return;
        }
    }
}