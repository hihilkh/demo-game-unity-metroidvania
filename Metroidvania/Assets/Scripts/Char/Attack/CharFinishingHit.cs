using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharFinishingHit : CharHitBase {
    // TODO : Put to CharParams
    private float additionalSpeed = 1f;

    public override void StartAttack (Transform refPoint, CharEnum.Direction direction, float charHorizontalSpeed) {
        transform.position = refPoint.position;

        var velocity = charHorizontalSpeed + additionalSpeed;
        if (direction == CharEnum.Direction.Left) {
            transform.eulerAngles = -transform.eulerAngles;

            var shape = ps.shape;
            shape.position = -shape.position;
            shape.rotation = -shape.rotation;

            var psr = ps.GetComponent<ParticleSystemRenderer> ();
            psr.flip = new Vector3 (0, 1, 0);

            velocity = -velocity;
        }

        rb.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}
