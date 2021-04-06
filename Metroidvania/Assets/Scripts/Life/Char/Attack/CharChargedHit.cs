using UnityEngine;

public class CharChargedHit : CharHitBase {
    protected override int BaseDP => Params.HitDP_Charged;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed, bool isPlayerAttack, int additionalDP) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed, isPlayerAttack, additionalDP);

        SetInitPos (refPoint.position);

        var velocity = charHorizontalSpeed;
        if (direction == LifeEnum.HorizontalDirection.Left) {
            InversePSShape ();

            velocity = -velocity;
        }

        RB.velocity = new Vector3 (velocity, 0, 0);

        DestroySelf (true, true);
    }
}