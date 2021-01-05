using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharDropHit : CharHitBase {
    public override void StartAttack (Transform refPoint, CharEnum.Direction direction, float charHorizontalSpeed) {
        GameUtils.InsertChildrenToParent (refPoint, gameObject, true, -1, false);
    }
}