using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        FrameworkUtils.InsertChildrenToParent (refPoint, gameObject, false, -1, false);
    }
}