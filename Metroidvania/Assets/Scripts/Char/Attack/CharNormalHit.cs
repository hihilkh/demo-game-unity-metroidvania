﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharNormalHit : CharHitBase {
    // TODO : Put to CharParams
    private float additionalSpeed = 1f;

    public override void StartAttack (Vector3 startWorldPos, CharEnum.Direction direction, float charHorizontalSpeed) {
        transform.position = startWorldPos;

        var velocity = charHorizontalSpeed + additionalSpeed;
        if (direction == CharEnum.Direction.Left) {
            var psr = ps.GetComponent<ParticleSystemRenderer> ();
            psr.flip = new Vector3 (1, 0, 0);

            velocity = -velocity;
        }

        rb.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (PSNotAliveDestroyCoroutine ());
    }
}
