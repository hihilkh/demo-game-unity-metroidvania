using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharHitBase : MonoBehaviour {

    [SerializeField] protected CharParams charParams;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected ParticleSystem ps;
    [SerializeField] protected CharAttackTrigger attackTrigger;

    protected LifeEnum.HorizontalDirection direction;
    protected abstract int dp { get; }

    public virtual void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        this.direction = direction;
        attackTrigger.HitEvent += Hit;
    }

    protected void SetInitPos (Vector3 pos) {
        // keep posZ so that it render on top
        transform.position = new Vector3 (pos.x, pos.y, transform.position.z);
    }

    protected IEnumerator PSNotAliveDestroyCoroutine () {
        yield return new WaitUntil (() => !ps.IsAlive ());

        DestroySelf ();
    }

    public virtual void DestroySelf () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    protected void InversePSShape () {
        var shape = ps.shape;
        shape.position = -shape.position;
        shape.rotation = -shape.rotation;
    }

    protected virtual void Hit (LifeBase lifeBase, Transform colliderTransform, bool isInvincible) {
        if (!isInvincible) {
            lifeBase.Hurt (dp, direction);
        }
    }
}
