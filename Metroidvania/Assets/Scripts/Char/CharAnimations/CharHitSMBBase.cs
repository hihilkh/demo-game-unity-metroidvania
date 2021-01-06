using HIHIFramework.Core;
using UnityEngine;

public class CharHitSMBBase : CharSMBBase {
    protected CharHitBase hitClone;
    protected virtual Transform generalHitRefPoint => animUtils.refPoint_GeneralHit;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Transform refPoint;

        switch (animUtils.model.currentHitType) {
            case CharEnum.HitType.Normal:
                hitClone = Instantiate (animUtils.normalHitTemplate);
                refPoint = generalHitRefPoint;
                break;
            case CharEnum.HitType.Charged:
                hitClone = Instantiate (animUtils.chargedHitTemplate);
                refPoint = generalHitRefPoint;
                break;
            case CharEnum.HitType.Finishing:
                hitClone = Instantiate (animUtils.finishingHitTemplate);
                refPoint = generalHitRefPoint;
                break;
            case CharEnum.HitType.Drop:
                hitClone = Instantiate (animUtils.dropHitTemplate);
                refPoint = animUtils.refPoint_DropHit;
                break;
            default:
                Log.PrintError ("currentHitType = " + animUtils.model.currentHitType + " . No implementation in CharHitSMBBase. Please check.");
                return;
        }

        hitClone.StartAttack (refPoint, animUtils.model.facingDirection, animUtils.GetVelocityXByCurrentHorizontalSpeed (true));
    }
}