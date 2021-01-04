using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharNormalHit : MonoBehaviour {
    // TODO : Put to CharParams
    private float additionalSpeed = 1f;

    [SerializeField] private ParticleSystem ps;
    [SerializeField] private Rigidbody2D rb;

    public void StartAttack (Vector3 worldPos, CharEnum.Direction direction, float CharSpeed) {
        transform.position = worldPos;

        var velocity = CharSpeed + additionalSpeed;
        if (direction == CharEnum.Direction.Left) {
            var psr = ps.GetComponent<ParticleSystemRenderer> ();
            psr.flip = new Vector3 (1, 0, 0);
            velocity = -velocity;
        }

        rb.velocity = new Vector3 (velocity, 0, 0);

        StartCoroutine (DestroyCoroutine ());
    }

    private IEnumerator DestroyCoroutine () {
        yield return new WaitUntil (() => !ps.IsAlive ());

        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    private void OnTriggerEnter2D (Collider2D collision) {
        // TODO
    }
}
