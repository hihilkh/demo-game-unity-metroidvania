using HihiFramework.Core;
using UnityEngine;

public class CharHitSMBBase : CharSMBBase {
    protected CharHitBase HitClone { get; private set; }
    protected virtual Transform GeneralHitRefPoint => AnimUtils.RefPoint_GeneralHit;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Transform refPoint;

        switch (AnimUtils.Model.CurrentHitType) {
            case CharEnum.HitType.Normal:
                HitClone = Instantiate (AnimUtils.NormalHitTemplate);
                refPoint = GeneralHitRefPoint;
                break;
            case CharEnum.HitType.Charged:
                HitClone = Instantiate (AnimUtils.ChargedHitTemplate);
                refPoint = GeneralHitRefPoint;
                break;
            case CharEnum.HitType.Finishing:
                HitClone = Instantiate (AnimUtils.FinishingHitTemplate);
                refPoint = GeneralHitRefPoint;
                break;
            case CharEnum.HitType.Drop:
                HitClone = Instantiate (AnimUtils.DropHitTemplate);
                refPoint = AnimUtils.RefPoint_DropHit;
                break;
            default:
                Log.PrintError ("currentHitType = " + AnimUtils.Model.CurrentHitType + " . No implementation in CharHitSMBBase. Please check.", LogTypes.Animation);
                return;
        }

        HitClone.StartAttack (refPoint, AnimUtils.Model.FacingDirection, AnimUtils.GetVelocityXByCurrentHorizontalSpeed (true));
    }
}