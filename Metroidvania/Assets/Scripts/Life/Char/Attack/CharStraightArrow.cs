using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharStraightArrow : CharArrowBase {
    public void StartAttack (Transform refPoint, CharEnum.HorizontalDirection facingDirection) {
        SetInitPos (refPoint.position);

        var impulseX = charParams.arrowInitialSpeed_Straight;
        if (facingDirection == CharEnum.HorizontalDirection.Left) {
            impulseX = -impulseX;
        }

        rb.AddForce (new Vector2 (impulseX, 0), ForceMode2D.Impulse);
        UpdateArrowPointingDirection ();
    }
}