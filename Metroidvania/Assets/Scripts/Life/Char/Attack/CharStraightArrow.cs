﻿using UnityEngine;

public class CharStraightArrow : CharArrowBase {
    protected override int DP => Params.ArrowDP_Straight;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection, bool isPlayerAttack) {
        Init (facingDirection, isPlayerAttack);

        SetInitPos (refPoint.position);

        var impulseX = Params.ArrowInitialSpeed_Straight;
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            impulseX = -impulseX;
        }

        RB.AddForce (new Vector2 (impulseX, 0), ForceMode2D.Impulse);
        UpdateArrowPointingDirection ();
    }
}