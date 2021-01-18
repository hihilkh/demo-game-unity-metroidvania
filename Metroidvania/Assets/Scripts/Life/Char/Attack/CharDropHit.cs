using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    protected override int dp => charParams.hit_dp_Drop;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed);

        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, -1, false);
    }
}