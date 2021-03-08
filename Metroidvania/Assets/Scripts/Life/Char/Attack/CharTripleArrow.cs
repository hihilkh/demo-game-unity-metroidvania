using UnityEngine;

// Remarks: It is to control only one of the arrows of the Triple ArrowType
public class CharTripleArrow : CharArrowBase {
    protected override int BaseDP => Params.ArrowDP_Triple;

    public void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection facingDirection, float shootingAngle, bool isPlayerAttack, int additionalDP, bool isFireArrow) {
        Init (facingDirection, isPlayerAttack, additionalDP, isFireArrow);

        SetInitPos (refPoint.position);

        var angleRadian = shootingAngle * Mathf.Deg2Rad;
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            angleRadian = Mathf.PI - angleRadian;
        }

        var cos = Mathf.Cos (angleRadian);
        var sin = Mathf.Sin (angleRadian);

        RB.AddForce (new Vector2 (Params.ArrowInitialSpeed_Triple * cos, Params.ArrowInitialSpeed_Triple * sin), ForceMode2D.Impulse);
    }

    private void Update () {
        if (!HasHitAnything) {
            UpdateArrowPointingDirection ();
        }
    }
}
