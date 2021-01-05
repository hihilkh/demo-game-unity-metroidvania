using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharHitBase : MonoBehaviour {

    [SerializeField] protected ParticleSystem ps;
    [SerializeField] protected Rigidbody2D rb;

    public abstract void StartAttack (Vector3 startWorldPos, CharEnum.Direction direction, float charHorizontalSpeed);

    protected IEnumerator PSNotAliveDestroyCoroutine () {
        yield return new WaitUntil (() => !ps.IsAlive ());

        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D (Collider2D collision) {
        // TODO
    }
}
