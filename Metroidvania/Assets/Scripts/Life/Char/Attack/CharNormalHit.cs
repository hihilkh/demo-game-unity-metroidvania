using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharNormalHit : CharHitBase {

    public override void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        SetInitPos (refPoint.position);

        var velocity = charHorizontalSpeed;
        if (direction == LifeEnum.HorizontalDirection.Left) {
            InversePSShape ();

            var psr = ps.GetComponent<ParticleSystemRenderer> ();
            psr.flip = new Vector3 (1, 0, 0);

            velocity = -velocity;
        }

        rb.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}
