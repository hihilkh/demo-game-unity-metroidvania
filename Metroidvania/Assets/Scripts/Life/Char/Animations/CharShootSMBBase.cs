using HihiFramework.Core;
using UnityEngine;

public class CharShootSMBBase : CharSMBBase {
    protected virtual Transform ShootRefPoint => AnimUtils.RefPoint_GeneralShoot;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        var isPlayerAttack = AnimUtils.Model.CharType == CharEnum.CharType.Player;
        var additionalDP = AnimUtils.Model.AdditionalDP;

        switch (AnimUtils.Model.CurrentArrowType) {
            case CharEnum.ArrowType.Target:
                var targetArrowClone = Instantiate (AnimUtils.TargetArrowTemplate);
                var targetPos = AnimUtils.Model.SearchShootTargetPos ();
                targetArrowClone.StartAttack (ShootRefPoint, AnimUtils.Model.FacingDirection, targetPos, isPlayerAttack, additionalDP);
                break;
            case CharEnum.ArrowType.Straight:
                var straightArrowClone = Instantiate (AnimUtils.StraightArrowTemplate);
                straightArrowClone.StartAttack (ShootRefPoint, AnimUtils.Model.FacingDirection, isPlayerAttack, additionalDP);
                break;
            case CharEnum.ArrowType.Triple:
                if (AnimUtils.Model.Params.TripleArrowShootingAngleList == null || AnimUtils.Model.Params.TripleArrowShootingAngleList.Count <= 0) {
                    Log.PrintError ("tripleArrowShootingAngleList has not yet set. Please check.", LogTypes.Animation);
                    return;
                }

                foreach (var angle in AnimUtils.Model.Params.TripleArrowShootingAngleList) {
                    var tripleArrowClone = Instantiate (AnimUtils.TripleArrowTemplate);
                    tripleArrowClone.StartAttack (ShootRefPoint, AnimUtils.Model.FacingDirection, angle, isPlayerAttack, additionalDP);
                }
                break;
            default:
                Log.PrintError ("currentArrowType = " + AnimUtils.Model.CurrentHitType + " . No implementation in CharShootSMBBase. Please check.", LogTypes.Animation);
                return;
        }
    }
}