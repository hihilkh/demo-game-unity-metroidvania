using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    protected override int dp => charParams.hitDP_Drop;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed);

        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, false, -1, false);
        attackTrigger.HitDropHitSwitchEvent += HitDropHitSwitch;
    }

    private void HitDropHitSwitch (MapSwitch mapSwitch) {
        mapSwitch.Trigger ();
    }
}