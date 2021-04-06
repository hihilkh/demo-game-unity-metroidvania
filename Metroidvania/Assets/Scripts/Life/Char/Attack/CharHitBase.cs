using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharHitBase : SpecialSceneEventDisposableBase {

    [SerializeField] private CharParams _params;
    protected CharParams Params => _params;
    [SerializeField] private Rigidbody2D _rb;
    protected Rigidbody2D RB => _rb;
    [SerializeField] private ParticleSystem _ps;
    protected ParticleSystem PS => _ps;
    [SerializeField] private CharAttackTrigger _attackTrigger;
    protected CharAttackTrigger AttackTrigger => _attackTrigger;
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private AudioSource audioSource;

    protected LifeEnum.HorizontalDirection Direction { get; private set; }
    protected abstract int BaseDP { get; }

    private int totalDP = 0;

    private Coroutine destroySelfCoroutine = null;

    public virtual void StartAttack (Transform refPoint, LifeEnum.HorizontalDirection direction, float charHorizontalSpeed, bool isPlayerAttack, int additionalDP) {
        if (isPlayerAttack) {
            gameObject.layer = GameVariable.PlayerAttackLayer;
        } else {
            gameObject.layer = GameVariable.EnemyAttackLayer;
        }

        Direction = direction;
        totalDP = BaseDP + additionalDP;

        AttackTrigger.HitLife += HitLifeHandler;
    }

    protected void SetInitPos (Vector3 pos) {
        // keep posZ so that it render on top
        transform.position = new Vector3 (pos.x, pos.y, transform.position.z);
    }

    private IEnumerator PSNotAliveDestroyCoroutine (bool isWaitForAudio) {
        yield return new WaitUntil (() => !PS.IsAlive ());

        destroySelfCoroutine = null;
        DestroySelf (false, isWaitForAudio);
    }

    private IEnumerator SfxNotPlayingDestroyCoroutine () {
        yield return new WaitUntil (() => !audioSource.isPlaying);

        destroySelfCoroutine = null;
        DestroySelf (false, false);
    }

    protected virtual void DisableSelf () {
        PS.Stop ();
        PS.Clear ();
        hitCollider.enabled = false;
    }

    public virtual void DestroySelf (bool isWaitForPS, bool isWaitForAudio) {
        if (isWaitForPS) {
            destroySelfCoroutine = StartCoroutine (PSNotAliveDestroyCoroutine (isWaitForAudio));
        } else if (isWaitForAudio) {
            DisableSelf ();     // Prevent any interaction
            destroySelfCoroutine = StartCoroutine (SfxNotPlayingDestroyCoroutine ());
        } else {
            if (destroySelfCoroutine != null) {
                StopCoroutine (destroySelfCoroutine);
                destroySelfCoroutine = null;
            }

            if (gameObject != null) {
                Destroy (gameObject);
            }
        }
    }

    protected void InversePSShape () {
        var shape = PS.shape;
        shape.position = -shape.position;
        shape.rotation = -shape.rotation;
    }

    #region Events

    protected virtual void HitLifeHandler (LifeBase lifeBase, bool isHurt) {
        if (isHurt) {
            lifeBase.Hurt (totalDP, Direction);
        }
    }

    #endregion

    #region MapDisposableBase

    protected override void Dispose () {
        DestroySelf (false, false);
    }

    #endregion
}
