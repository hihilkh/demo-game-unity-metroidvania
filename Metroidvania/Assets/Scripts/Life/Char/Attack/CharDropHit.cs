using HihiFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    protected override int BaseDP => Params.HitDP_Drop;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed, bool isPlayerAttack, int additionalDP) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed, isPlayerAttack, additionalDP);

        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, false, -1, false);

        AttackTrigger.HitDropHitSwitch += HitDropHitSwitch;
    }

    private void HitDropHitSwitch (MapSwitch mapSwitch) {
        mapSwitch.Trigger ();
    }
}