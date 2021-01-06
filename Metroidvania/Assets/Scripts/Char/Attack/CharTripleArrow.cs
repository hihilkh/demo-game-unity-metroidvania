using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remarks: It is to control only one of the arrows of the Triple ArrowType
public class CharTripleArrow : CharArrowBase {
    public void StartAttack (Transform refPoint, CharEnum.Direction facingDirection, float shootingAngle) {
        SetInitPos (refPoint.position);

        var angleRadian = shootingAngle * Mathf.Deg2Rad;
        if (facingDirection == CharEnum.Direction.Left) {
            angleRadian = Mathf.PI - angleRadian;
        }

        var cos = Mathf.Cos (angleRadian);
        var sin = Mathf.Sin (angleRadian);

        rb.AddForce (new Vector2 (charParams.arrowInitialSpeed_Triple * cos, charParams.arrowInitialSpeed_Triple * sin), ForceMode2D.Impulse);
    }

    private void Update () {
        if (!hasHitAnything) {
            UpdateArrowPointingDirection ();
        }
    }
}
