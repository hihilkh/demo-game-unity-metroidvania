using HihiFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    protected override int DP => Params.HitDP_Drop;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed);

        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, false, -1, false);
        AttackTrigger.HitDropHitSwitch += HitDropHitSwitch;
    }

    private void HitDropHitSwitch (MapSwitch mapSwitch) {
        mapSwitch.Trigger ();
    }
}