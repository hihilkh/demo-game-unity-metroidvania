using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public abstract class CharArrowBase : MonoBehaviour {
    [SerializeField] private CharParams _params;
    protected CharParams Params => _params;
    [SerializeField] private SpriteRenderer _arrowSprite;
    protected SpriteRenderer ArrowSprite => _arrowSprite;
    [SerializeField] private Rigidbody2D _rb;
    protected Rigidbody2D RB => _rb;
    [SerializeField] private CharAttackTrigger _attackTrigger;
    protected CharAttackTrigger AttackTrigger => _attackTrigger;

    protected bool HasHitAnything { get; private set; } = false;
    private const float VanishPeriod = 1f;

    protected LifeEnum.HorizontalDirection Direction { get; private set; }
    protected abstract int BaseDP { get; }

    private int totalDP = 0;

    protected void Init (LifeEnum.HorizontalDirection direction, bool isPlayerAttack, int additionalDP) {
        if (isPlayerAttack) {
            gameObject.layer = GameVariable.PlayerAttackLayer;
        } else {
            gameObject.layer = GameVariable.EnemyAttackLayer;
        }

        Direction = direction;
        totalDP = BaseDP + additionalDP;

        AttackTrigger.HitLife += HitLifeHandler;
        AttackTrigger.HitEnvironment += HitEnvironmentHandler;
        AttackTrigger.HitArrowSwitch += HitArrowSwitchHandler;
    }

    protected void SetInitPos (Vector3 pos) {
        // keep posZ so that it render on top
        transform.position = new Vector3 (pos.x, pos.y, transform.position.z);
    }

    protected void UpdateArrowPointingDirection () {
        transform.right = RB.velocity.normalized;
    }

    private void Hit (Transform target) {
        HasHitAnything = true;
        FrameworkUtils.InsertChildrenToParent (target, gameObject, false);
        RB.bodyType = RigidbodyType2D.Kinematic;
        RB.velocity = Vector2.zero;

        StartCoroutine (Vanish ());
    }

    private IEnumerator Vanish () {
        var startTime = Time.time;

        while (Time.time - startTime < VanishPeriod) {
            var progress = (Time.time - startTime) / VanishPeriod;
            ArrowSprite.color = new Color (ArrowSprite.color.r, ArrowSprite.color.g, ArrowSprite.color.b, 1 - progress);
            yield return null;
        }

        DestroySelf ();
    }

    private void DestroySelf () {
        if (gameObject != null) {
            Destroy (gameObject);
        }
    }

    #region Events

    private void HitLifeHandler (LifeBase lifeBase, Transform colliderTransform, bool isHurt) {
        if (HasHitAnything) {
            return;
        }

        if (isHurt) {
            lifeBase.Hurt (totalDP, Direction);
        }

        Hit (colliderTransform);
    }

    private void HitEnvironmentHandler (Transform colliderTransform) {
        if (HasHitAnything) {
            return;
        }

        Hit (colliderTransform);
    }

    private void HitArrowSwitchHandler (MapSwitch mapSwitch) {
        if (HasHitAnything) {
            return;
        }

        Hit (mapSwitch.transform);
        mapSwitch.Trigger ();
    }

    #endregion
}