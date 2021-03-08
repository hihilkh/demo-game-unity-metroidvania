using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public abstract class CharArrowBase : MonoBehaviour {
    [SerializeField] private CharParams _params;
    protected CharParams Params => _params;
    [SerializeField] private SpriteRenderer _arrowSpriteRenderer;
    protected SpriteRenderer ArrowSpriteRenderer => _arrowSpriteRenderer;
    [SerializeField] private Rigidbody2D _rb;
    protected Rigidbody2D RB => _rb;
    [SerializeField] private CharAttackTrigger _attackTrigger;
    protected CharAttackTrigger AttackTrigger => _attackTrigger;

    [SerializeField] private Sprite _normalArrowSprite;
    private Sprite normalArrowSprite => _normalArrowSprite;

    [SerializeField] private Sprite _fireArrowSprite;
    private Sprite fireArrowSprite => _fireArrowSprite;

    protected bool HasHitAnything { get; private set; } = false;
    private const float NormalArrowVanishPeriod = 1f;

    protected LifeEnum.HorizontalDirection Direction { get; private set; }
    protected abstract int BaseDP { get; }

    private int totalDP = 0;
    private bool isFireArrow = false;

    protected void Init (LifeEnum.HorizontalDirection direction, bool isPlayerAttack, int additionalDP, bool isFireArrow) {
        if (isPlayerAttack) {
            gameObject.layer = GameVariable.PlayerAttackLayer;
        } else {
            gameObject.layer = GameVariable.EnemyAttackLayer;
        }

        Direction = direction;
        totalDP = BaseDP + additionalDP;

        this.isFireArrow = isFireArrow;
        if (isFireArrow) {
            ArrowSpriteRenderer.sprite = fireArrowSprite;
        } else {
            ArrowSpriteRenderer.sprite = normalArrowSprite;
        }

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

        // Prevent inverted appearance
        var rotationZ = FrameworkUtils.Clamp0360Angle (transform.rotation.eulerAngles.z);
        if (rotationZ > 90 && rotationZ < 270) {
            transform.localScale = new Vector3 (transform.localScale.x, -1, transform.localScale.z);
        } else {
            transform.localScale = new Vector3 (transform.localScale.x, 1, transform.localScale.z);
        }
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

        var vanishPeriod = isFireArrow ? GameVariable.FireAttackNoOfTrigger * GameVariable.FireAttackTriggerPeriod : NormalArrowVanishPeriod;
        while (Time.time - startTime < vanishPeriod) {
            var progress = (Time.time - startTime) / vanishPeriod;
            ArrowSpriteRenderer.color = new Color (ArrowSpriteRenderer.color.r, ArrowSpriteRenderer.color.g, ArrowSpriteRenderer.color.b, 1 - progress);
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

    private void HitLifeHandler (LifeBase lifeBase, bool isHurt) {
        if (HasHitAnything) {
            return;
        }

        if (isHurt) {
            lifeBase.Hurt (totalDP, Direction, isFireArrow);
        }

        Hit (lifeBase.DisplayBaseTransform);
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