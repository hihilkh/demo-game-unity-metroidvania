using UnityEngine;

public class CharFinishingHit : CharHitBase {
    protected override int BaseDP => Params.HitDP_Finishing;

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed, bool isPlayerAttack, int additionalDP) {
        base.StartAttack (refPoint, direction, charHorizontalSpeed, isPlayerAttack, additionalDP);

        SetInitPos (refPoint.position);

        var velocity = charHorizontalSpeed;
        if (direction == LifeEnum.HorizontalDirection.Left) {
            transform.eulerAngles = -transform.eulerAngles;

            InversePSShape ();

            var psr = PS.GetComponent<ParticleSystemRenderer> ();
            psr.flip = new Vector3 (0, 1, 0);

            velocity = -velocity;
        }

        RB.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}
