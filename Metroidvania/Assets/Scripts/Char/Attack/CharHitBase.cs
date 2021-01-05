using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharHitBase : MonoBehaviour {

    [SerializeField] protected ParticleSystem ps;
    [SerializeField] protected Rigidbody2D rb;

    public abstract void StartAttack (Transform refPoint, CharEnum.Direction direction, float charHorizontalSpeed);

    protected IEnumerator PSNotAliveDestroyCoroutine () {
        yield return new WaitUntil (() => !ps.IsAlive ());

        DestroySelf ();
    }

    public virtual void DestroySelf () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D (Collider2D collision) {
        // TODO
    }
}
