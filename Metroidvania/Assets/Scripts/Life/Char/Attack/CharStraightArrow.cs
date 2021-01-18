using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharStraightArrow : CharArrowBase {
    protected override int dp => charParams.arrow_dp_Straight;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection) {
        Init (facingDirection);

        SetInitPos (refPoint.position);

        var impulseX = charParams.arrowInitialSpeed_Straight;
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            impulseX = -impulseX;
        }

        rb.AddForce (new Vector2 (impulseX, 0), ForceMode2D.Impulse);
        UpdateArrowPointingDirection ();
    }
}