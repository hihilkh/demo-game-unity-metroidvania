using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharChargedHit : CharHitBase {
    public override void StartAttack (Transform refPoint, CharEnum.Direction direction, float charHorizontalSpeed) {
        SetInitPos (refPoint.position);

        var velocity = charHorizontalSpeed;
        if (direction == CharEnum.Direction.Left) {
            InversePSShape ();

            velocity = -velocity;
        }

        rb.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}