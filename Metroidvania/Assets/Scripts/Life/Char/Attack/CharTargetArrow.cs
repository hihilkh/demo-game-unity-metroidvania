using HihiFramework.Core;
using UnityEngine;

public class CharTargetArrow : CharArrowBase {
    protected override int DP => Params.ArrowDP_Target;

    private const float DefaultImpulseAngleInRadian = Mathf.PI / 6;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection, Vector2? targetPos, bool isPlayerAttack) {
        Init (facingDirection, isPlayerAttack);

        SetInitPos (refPoint.position);

        var impulse = CalculateInitialImpulse (facingDirection, targetPos);
        RB.AddForce (impulse, ForceMode2D.Impulse);
    }

    private void Update () {
        if (!HasHitAnything) {
            UpdateArrowPointingDirection ();
        }
    }

    #region Calculation

    private Vector2 CalculateInitialImpulse (LifeEnum.HorizontalDirection facingDirection, Vector2? targetPos) {
        var theta = CalculateInitialImpulseAngle (facingDirection, targetPos);

        var cos = Mathf.Cos (theta);
        var sin = Mathf.Sin (theta);

        return new Vector2 (Params.ArrowInitialSpeed_Target * cos, Params.ArrowInitialSpeed_Target * sin);
    }

    private float CalculateInitialImpulseAngle (LifeEnum.HorizontalDirection facingDirection, Vector2? optionalTargetPos) {
        // Remarks :
        // Details calculation refer to : https://www.youtube.com/watch?app=desktop&v=bqYtNrhdDAY
        // h of video -> -deltaY below
        // g of video -> -g below

        if (optionalTargetPos == null) {
            return GetDefaultImpulseAngle (facingDirection);
        }

        var targetPos = (Vector2)optionalTargetPos;

        var deltaX = targetPos.x - transform.position.x;
        var deltaY = targetPos.y - transform.position.y;

        if (facingDirection == LifeEnum.HorizontalDirection.Right && deltaX < 0) {
            Log.PrintWarning ("Wrong direction. Use default impulse.", LogTypes.Char);
            return GetDefaultImpulseAngle (facingDirection);
        } else if (facingDirection == LifeEnum.HorizontalDirection.Left && deltaX > 0) {
            Log.PrintWarning ("Wrong direction. Use default impulse.", LogTypes.Char);
            return GetDefaultImpulseAngle (facingDirection);
        }

        var dist = Vector2.Distance (transform.position, targetPos);
        var g = RB.gravityScale * Physics2D.gravity.y;

        var cosAngleSum = (deltaY - g * deltaX * deltaX / (Params.ArrowInitialSpeed_Target * Params.ArrowInitialSpeed_Target)) / dist;

        if (Mathf.Abs (cosAngleSum) > 1) {
            Log.Print ("Cannot reach target. Use default impulse.", LogTypes.Char);
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
                Log.PrintWarning ("Cannot get impulse within required angle range. Use default impulse.", LogTypes.Char);
                return GetDefaultImpulseAngle (facingDirection);
            }
        } else {
            while (theta < lowerLimit) {
                index++;
                theta = (GetGeneralSolutionOfCos (acos, index) + phi) / 2;
            }

            if (theta > upperLimit) {
                Log.PrintWarning ("Cannot get impulse within required angle range. Use default impulse.", LogTypes.Char);
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
