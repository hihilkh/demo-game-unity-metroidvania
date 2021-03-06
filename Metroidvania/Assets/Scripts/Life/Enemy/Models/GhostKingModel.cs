using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class GhostKingModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.GhostKing;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Flying;

    private enum AttackStatus {
        Attack,
        CoolDown,
        Float,
        BeforeAttack,
    }

    [SerializeField] private GameObject ringObject;
    [SerializeField] private GameObject attackRingObject;
    [SerializeField] private Transform spritesBaseTransform;
    [SerializeField] private Collider2D attackRingCollider;

    private AttackStatus _currentAttackStatus = AttackStatus.BeforeAttack;
    private AttackStatus CurrentAttackStatus {
        get { return _currentAttackStatus; }
        set {
            if (_currentAttackStatus != value) {
                changeStatusTime = Time.time;
                _currentAttackStatus = value;
            }
        }
    }

    private float targetFloatingHeight;
    private float changeStatusTime;
    private bool isNeedToChangeStatus = true;
    private bool isFloatingUp = false;
    private Vector2 normalizedAttackDirection;


    protected override bool Reset (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Reset (pos, direction);

        if (!hasInitBefore) {
            targetFloatingHeight = GetPos ().y;
        }

        SetAttackDisplay (false);

        return hasInitBefore;
    }

    private GhostKingParams GetParams () {
        if (Params is GhostKingParams) {
            return (GhostKingParams)Params;
        }

        Log.PrintError ("Params is not with type GhostKingParams. Please check.", LogTypes.Enemy);
        return null;
    }

    protected override void HandleCollision (CollisionAnalysis collisionAnalysis) {
        base.HandleCollision (collisionAnalysis);

        var collisionDetailsDict = collisionAnalysis.CollisionDetailsDict;

        switch (CurrentAttackStatus) {
            case AttackStatus.Attack:
                var collisionNormalList = new List<Vector2> ();
                if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Ground)) {
                    collisionNormalList.Add (collisionDetailsDict[LifeEnum.CollisionType.Ground][0].CollisionNormal);
                }
                if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Wall)) {
                    collisionNormalList.Add (collisionDetailsDict[LifeEnum.CollisionType.Wall][0].CollisionNormal);
                }
                if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Roof)) {
                    collisionNormalList.Add (collisionDetailsDict[LifeEnum.CollisionType.Roof][0].CollisionNormal);
                }

                isNeedToChangeStatus = CheckIsStopAttack (collisionNormalList);
                break;
            case AttackStatus.Float:
                if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Roof)) {
                    isNeedToChangeStatus = true;
                }
                break;
        }
    }

    protected override bool DecideAction () {
        if (!base.DecideAction ()) {
            return false;
        }

        var isChangeStatus = false;
        if (isNeedToChangeStatus) {
            isChangeStatus = true;
            isNeedToChangeStatus = false;
        } else {
            switch (CurrentAttackStatus) {
                case AttackStatus.Attack:
                    break;
                case AttackStatus.CoolDown:
                    if (Time.time - changeStatusTime >= GetParams ().AttackCoolDownPeriod) {
                        isChangeStatus = true;
                    }
                    break;
                case AttackStatus.Float:
                    FacingDirection = GetChasingCharHorizontalDirection ();

                    if (isFloatingUp && GetPos ().y >= targetFloatingHeight) {
                        isChangeStatus = true;
                    } else if (!isFloatingUp && GetPos ().y <= targetFloatingHeight) {
                        isChangeStatus = true;
                    }
                    break;
                case AttackStatus.BeforeAttack:
                    if (Time.time - changeStatusTime >= GetParams ().BeforeAttackPeriod) {
                        isChangeStatus = true;
                    }
                    break;
            }
        }

        if (isChangeStatus) {
            ChangeAttackStatus ();
        }

        return false;
    }

    #region AttackStatus action

    private void ChangeAttackStatus () {
        switch (CurrentAttackStatus) {
            case AttackStatus.Attack:
                StartCoolDown ();
                break;
            case AttackStatus.CoolDown:
                StartFloating ();
                break;
            case AttackStatus.Float:
                StartBeforeAttack ();
                break;
            case AttackStatus.BeforeAttack:
                StartAttack ();
                break;
        }
    }

    private void StartAttack () {
        CurrentAttackStatus = AttackStatus.Attack;

        normalizedAttackDirection = GetChasingCharDirection (true);
        OnFlyingInfoUpdated (normalizedAttackDirection, GetParams ().AttackMaxMovementSpeed, GetParams ().AttackAcceleration);

        if (FacingDirection == LifeEnum.HorizontalDirection.Left && normalizedAttackDirection.x > 0) {
            ChangeFacingDirection ();
        } else if (FacingDirection == LifeEnum.HorizontalDirection.Right && normalizedAttackDirection.x < 0) {
            ChangeFacingDirection ();
        }

        SetAttackDisplay (true);
        SetAnimatorTrigger (EnemyAnimConstant.AttackTriggerName);
    }

    private void StartFloating () {
        CurrentAttackStatus = AttackStatus.Float;
        var normalizedDirection = Vector2.up;
        isFloatingUp = true;
        if (GetPos ().y > targetFloatingHeight) {
            normalizedDirection = Vector2.down;
            isFloatingUp = false;
        }

        OnFlyingInfoUpdated (normalizedDirection, GetParams ().MaxMovementSpeed, GetParams ().Acceleration);
        SetAttackDisplay (false);
        StartMoving ();
    }

    private void StartCoolDown () {
        CurrentAttackStatus = AttackStatus.CoolDown;
        OnFlyingInfoUpdated (Vector2.one, 0, null);
    }

    private void StartBeforeAttack () {
        CurrentAttackStatus = AttackStatus.BeforeAttack;
        SetAnimatorTrigger (EnemyAnimConstant.IdleTriggerName);
    }

    private void SetAttackDisplay (bool isAttacking) {
        ringObject.SetActive (!isAttacking);
        attackRingObject.SetActive (isAttacking);

        if (isAttacking) {
            if (FacingDirection == LifeEnum.HorizontalDirection.Left) {
                spritesBaseTransform.right = new Vector2 (-normalizedAttackDirection.x, -normalizedAttackDirection.y);
            } else {
                spritesBaseTransform.right = normalizedAttackDirection;
            }
        } else {
            spritesBaseTransform.rotation = Quaternion.identity;
        }
    }

    private bool CheckIsStopAttack (List<Vector2> collisionNormalList) {
        if (collisionNormalList == null || collisionNormalList.Count <= 0) {
            return false;
        }

        foreach (var collisionNormal in collisionNormalList) {
            if (Vector2.Dot (collisionNormal, normalizedAttackDirection) < 0) {
                // That is, the attack is moving towards the collider
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Invincible

    protected override void StartInvincible () {
        base.StartInvincible ();

        attackRingCollider.enabled = false;
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        attackRingCollider.enabled = true;
    }

    #endregion
}