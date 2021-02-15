using UnityEngine;

public class CharChargedHit : CharHitBase {
    protected override int DP => Params.HitDP_Charged;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed);

        SetInitPos (refPoint.position);

        var velocity = charHorizontalSpeed;
        if (direction == LifeEnum.HorizontalDirection.Left) {
            InversePSShape ();

            velocity = -velocity;
        }

        RB.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}