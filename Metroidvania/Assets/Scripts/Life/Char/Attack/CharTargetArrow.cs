﻿using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharTargetArrow : CharArrowBase {
    protected override int dp => charParams.arrow_dp_Target;

    private const float DefaultImpulseAngleInRadian = Mathf.PI / 6;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection, Transform target) {
        Init (facingDirection);

        SetInitPos (refPoint.position);

        var impulse = CalculateInitialImpulse (facingDirection, target);
        rb.AddForce (impulse, ForceMode2D.Impulse);
    }

    private void Update () {
        if (!hasHitAnything) {
            UpdateArrowPointingDirection ();
        }
    }

    #region Calculation

    private Vector2 CalculateInitialImpulse (LifeEnum.HorizontalDirection facingDirection, Transform target) {
        var theta = CalculateInitialImpulseAngle (facingDirection, target);

        var cos = Mathf.Cos (theta);
        var sin = Mathf.Sin (theta);

        return new Vector2 (charParams.arrowInitialSpeed_Target * cos, charParams.arrowInitialSpeed_Target * sin);
    }

    private float CalculateInitialImpulseAngle (LifeEnum.HorizontalDirection facingDirection, Transform target) {
        // Remarks :
        // Details calculation refer to : https://www.youtube.com/watch?app=desktop&v=bqYtNrhdDAY
        // h of video -> -deltaY below
        // g of video -> -g below

        if (target == null) {
            return GetDefaultImpulseAngle (facingDirection);
        }

        var deltaX = target.position.x - transform.position.x;
        var deltaY = target.position.y - transform.position.y;

        if (facingDirection == LifeEnum.HorizontalDirection.Right && deltaX < 0) {
            Log.PrintWarning ("Wrong direction. Use default impulse.", LogType.Char);
            return GetDefaultImpulseAngle (facingDirection);
        } else if (facingDirection == LifeEnum.HorizontalDirection.Left && deltaX > 0) {
            Log.PrintWarning ("Wrong direction. Use default impulse.", LogType.Char);
            return GetDefaultImpulseAngle (facingDirection);
        }

        var dist = Vector2.Distance (transform.position, target.position);
        var g = rb.gravityScale * Physics2D.gravity.y;

        var cosAngleSum = (deltaY - g * deltaX * deltaX / (charParams.arrowInitialSpeed_Target * charParams.arrowInitialSpeed_Target)) / dist;

        if (Mathf.Abs (cosAngleSum) > 1) {
            Log.Print ("Cannot reach target. Use default impulse.", LogType.Char);
            return GetDefaultImpulseAngle (facingDirection);
        }

        // choose an angle within :
        // Right -> -PI/2 ~ PI/2 ; Prefer smaller angle
        // Left -> PI/2 ~ 3PI/2 ; Prefer larger angle
        var acos = Mathf.Acos (cosAngleSum);
        var phi = Mathf.Atan2 (deltaX, -deltaY);

        var index = 0;
        var theta = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;

        var lowerLimit = facingDirection == LifeEnum.HorizontalDirection.Right ? -Mathf.PI / 2 : Mathf.PI / 2;
        var upperLimit = facingDirection == LifeEnum.HorizontalDirection.Right ? Mathf.PI / 2 : 3 * Mathf.PI / 2;

        if (theta > upperLimit) {
            while (theta > upperLimit) {
                index--;
                theta = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;
            }

            if (theta < lowerLimit) {
                Log.PrintWarning ("Cannot get impulse within required angle range. Use default impulse.", LogType.Char);
                return GetDefaultImpulseAngle (facingDirection);
            }
        } else {
            while (theta < lowerLimit) {
                index++;
                theta = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;
            }

            if (theta > upperLimit) {
                Log.PrintWarning ("Cannot get impulse within required angle range. Use default impulse.", LogType.Char);
                return GetDefaultImpulseAngle (facingDirection);
            }
        }

        // Try to get a more prefered angle
        if (facingDirection == LifeEnum.HorizontalDirection.Right) {
            index--;
            var temp = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;
            if (temp >= lowerLimit) {
                theta = temp;
            }
        } else {
            index++;
            var temp = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;
            if (temp <= upperLimit) {
                theta = temp;
            }
        }

        return theta;
    }

    private float GetDefaultImpulseAngle (LifeEnum.HorizontalDirection facingDirection) {
        if (facingDirection == LifeEnum.HorizontalDirection.Right) {
            return DefaultImpulseAngleInRadian;
        } else {
            return Mathf.PI - DefaultImpulseAngleInRadian;
        }
    }

    private float GetGeneralSolutionOfCos (float acos, int index) {
        if (index % 2 == 0) {
            return acos + 2 * Mathf.PI * (index / 2);
        } else {
            return 2 * Mathf.PI * ((index + 1) / 2) - acos;
        }
    }

    #endregion
}