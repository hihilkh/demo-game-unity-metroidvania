using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharHitBase : MonoBehaviour {

    [SerializeField] private CharParams _params;
    protected CharParams Params => _params;
    [SerializeField] private Rigidbody2D _rb;
    protected Rigidbody2D RB => _rb;
    [SerializeField] private ParticleSystem _ps;
    protected ParticleSystem PS => _ps;
    [SerializeField] private CharAttackTrigger _attackTrigger;
    protected CharAttackTrigger AttackTrigger => _attackTrigger;

    protected LifeEnum.HorizontalDirection Direction { get; private set; }
    protected abstract int DP { get; }

    public virtual void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed) {
        Direction = direction;
        AttackTrigger.HitLife += HitLifeHandler;
    }

    protected void SetInitPos (Vector3 pos) {
        // keep posZ so that it render on top
        transform.position = new Vector3 (pos.x, pos.y, transform.position.z);
    }

    protected IEnumerator PSNotAliveDestroyCoroutine () {
        yield return new WaitUntil (() => !PS.IsAlive ());

        DestroySelf ();
    }

    public virtual void DestroySelf () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    protected void InversePSShape () {
        var shape = PS.shape;
        shape.position = -shape.position;
        shape.rotation = -shape.rotation;
    }

    #region Events

    protected virtual void HitLifeHandler (LifeBase lifeBase, Transform colliderTransform, bool isHurt) {
        if (isHurt) {
            lifeBase.Hurt (DP, Direction);
        }
    }

    #endregion
}
