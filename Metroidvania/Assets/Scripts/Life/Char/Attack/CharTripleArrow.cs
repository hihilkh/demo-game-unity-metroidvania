using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Remarks: It is to control only one of the arrows of the Triple ArrowType
public class CharTripleArrow : CharArrowBase {
    protected override int dp => charParams.arrow_dp_Triple;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection, float shootingAngle) {
        Init (facingDirection);

        SetInitPos (refPoint.position);

        var angleRadian = shootingAngle * Mathf.Deg2Rad;
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
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
