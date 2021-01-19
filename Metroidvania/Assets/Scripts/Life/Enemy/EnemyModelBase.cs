using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

// TODO : Use Update to control enemy actions
// TODO : Ensure during beat back and die, no other action
// TODO : Ensure no action go after die
public abstract class EnemyModelBase : LifeBase {

    private LifeEnum.Location _currentLocation;
    public override LifeEnum.Location currentLocation {
        get {
            return _currentLocation;
        }
        protected set {
            if (_currentLocation != value) {
                CurrentLocationChangedAction (_currentLocation, value);
                _currentLocation = value;
            }
        }
    }

    [SerializeField] private Animator animator;
    [SerializeField] private EnemyParams _enemyParams;
    public EnemyParams enemyParams { get => _enemyParams; }

    public event Action<LifeEnum.HorizontalDirection> facingDirectionChangedEvent;

    public abstract EnemyEnum.MovementType movementType { get; }
    protected EnemyEnum.Status currentStatus;

    // Delay Action Coroutines
    protected Coroutine delayJumpCoroutine;

    // Jump
    private bool isJustJumpedUp;
    private bool isJumpRecursively;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 beatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 WalkingBeatBackDirection_Right = new Vector2 (1, 0.577f).normalized;    // About 30 degree elevation
    private static Vector2 WalkingBeatBackDirection_Left = Vector2.Scale (WalkingBeatBackDirection_Right, new Vector2 (-1, 1));

    public override bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Init (pos, direction);

        if (hasInitBefore) {
            return hasInitBefore;
        }

        currentStatus = EnemyEnum.Status.Normal;
        SetJumpSettings ();

        delayJumpCoroutine = null;

        return hasInitBefore;
    }

    protected virtual void ClearAllDelayActions () {
        if (delayJumpCoroutine != null) {
            StopCoroutine (delayJumpCoroutine);
            delayJumpCoroutine = null;
        }
    }

    #region Animtor

    protected void SetAnimatorTrigger (string triggerName) {
        Log.PrintDebug (gameObject.name + " : SetAnimatorTrigger : " + triggerName, LogType.Enemy | LogType.Animation);
        animator.SetTrigger (triggerName);
    }

    protected void SetAnimatorBool (string boolName, bool value) {
        Log.PrintDebug (gameObject.name + " : SetAnimatorBool : " + boolName + " ; Value : " + value, LogType.Enemy | LogType.Animation);
        animator.SetBool (boolName, value);
    }

    #endregion

    #region HP related

    public override bool GetIsCurrentlyBeatingBack () {
        return (currentStatus & EnemyEnum.Status.BeatingBack) == EnemyEnum.Status.BeatingBack;
    }

    public override bool GetIsCurrentlyInvincible () {
        return (currentStatus & EnemyEnum.Status.Invincible) == EnemyEnum.Status.Invincible;
    }

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        var isAlive = base.Hurt (dp, hurtDirection);
        Log.Print (gameObject.name + " : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + currentHP, LogType.Enemy);
        if (isAlive) {
            StartBeatingBack (hurtDirection, false);
            StartCoroutine (SetInvincible (false));
        }

        return isAlive;
    }

    private void DieByTouchedDeathTag () {
        Die (facingDirection);
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print (gameObject.name + " : Die!", LogType.Enemy);

        StartBeatingBack (dieDirection, true);
        StartCoroutine (SetInvincible (true));
    }

    public virtual void DestroySelf () {
        if (baseTransform.gameObject != null) {
            Destroy (baseTransform.gameObject);
        }
    }

    protected void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection, bool isDie) {
        ClearAllDelayActions ();

        switch (movementType) {
            case EnemyEnum.MovementType.Walking:
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? WalkingBeatBackDirection_Left : WalkingBeatBackDirection_Right;
                break;
            case EnemyEnum.MovementType.Flying:
            default:
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? new Vector2 (-1, 0) : Vector2.one;
                break;
        }

        SetAnimatorTrigger (isDie ? EnemyAnimConstant.DieTriggerName : EnemyAnimConstant.BeatBackTriggerName);
        currentStatus = currentStatus | EnemyEnum.Status.BeatingBack;
    }

    protected void StopBeatingBack () {
        SetAnimatorTrigger (EnemyAnimConstant.DefaultTriggerName);
        currentStatus = currentStatus & ~EnemyEnum.Status.BeatingBack;
    }

    protected IEnumerator SetInvincible (bool isDie) {
        lifeCollision.SetLayer (true);
        currentStatus = currentStatus | EnemyEnum.Status.Invincible;

        if (isDie) {
            yield break;
        }

        SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, true);


        yield return new WaitForSeconds (enemyParams.invincibleTime);

        lifeCollision.SetLayer (false);
        currentStatus = currentStatus & ~EnemyEnum.Status.Invincible;
        SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, false);
    }

    #endregion

    #region Location

    protected virtual void CurrentLocationChangedAction (LifeEnum.Location fromLocation, LifeEnum.Location toLocation) {
        // Do nothing. For override.
    }

    #endregion

    #region Facing Direction

    protected void ChangeFacingDirection () {
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            facingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            facingDirection = LifeEnum.HorizontalDirection.Left;
        }

        facingDirectionChangedEvent?.Invoke (facingDirection);
    }

    #endregion

    #region Jump

    private void SetJumpSettings () {
        isJustJumpedUp = false;
        isJumpRecursively = enemyParams.recursiveJumpPeriod >= 0;

        if (isJumpRecursively) {
            JumpAfter (enemyParams.recursiveJumpPeriod);
        }
    }

    protected void JumpAfter (float second) {
        delayJumpCoroutine = StartCoroutine (DelayJump (second));
    }

    private IEnumerator DelayJump (float second) {
        yield return new WaitForSeconds (second);

        Jump ();
        delayJumpCoroutine = null;
    }

    protected void Jump () {
        Log.PrintDebug (gameObject.name + " : Jump", LogType.Enemy);

        if (currentLocation != LifeEnum.Location.Ground) {
            Log.Print (gameObject.name + " : Do not Jump. It is not in ground", LogType.Enemy);
            return;
        }

        isJustJumpedUp = true;
        currentLocation = LifeEnum.Location.Air;
        SetAnimatorTrigger (EnemyAnimConstant.JumpTriggerName);
    }

    private void StartFreeFall () {
        currentLocation = LifeEnum.Location.Air;

        SetAnimatorTrigger (EnemyAnimConstant.FreeFallTriggerName);
    }

    #endregion

    #region Collision

    protected override void RegisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent += TouchGround;
        lifeCollision.LeftGroundEvent += LeaveGround;
        ////lifeCollision.TouchedRoofEvent    // No action for touch roof
        ////lifeCollision.LeftRoofEvent       // No action for leave roof
        lifeCollision.TouchedWallEvent += TouchWall;
        lifeCollision.LeftWallEvent += LeaveWall;
        lifeCollision.TouchedDeathTagEvent += DieByTouchedDeathTag;
    }

    protected override void UnregisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent -= TouchGround;
        lifeCollision.LeftGroundEvent -= LeaveGround;
        ////lifeCollision.TouchedRoofEvent    // No action for touch roof
        ////lifeCollision.LeftRoofEvent       // No action for leave roof
        lifeCollision.TouchedWallEvent -= TouchWall;
        lifeCollision.LeftWallEvent -= LeaveWall;
        lifeCollision.TouchedDeathTagEvent -= DieByTouchedDeathTag;
    }

    private void TouchGround () {
        Log.PrintDebug (gameObject.name + " : TouchGround", LogType.Enemy);

        // Touch ground while init / set position
        if (currentLocation == LifeEnum.Location.Unknown) {
            currentLocation = LifeEnum.Location.Ground;
            return;
        }

        switch (currentLocation) {
            case LifeEnum.Location.Air:
                if (movementType == EnemyEnum.MovementType.Walking) {
                    if ((currentStatus & EnemyEnum.Status.BeatingBack) == EnemyEnum.Status.BeatingBack) {
                        StopBeatingBack ();
                    } else {
                        SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
                    }

                    if (isJumpRecursively) {
                        JumpAfter (enemyParams.recursiveJumpPeriod);
                    }
                }
                break;
            default:
                // Do nothing
                break;
        }

        currentLocation = LifeEnum.Location.Ground;
    }

    private void LeaveGround () {
        Log.PrintDebug (gameObject.name + " : LeaveGround", LogType.Enemy);

        currentLocation = LifeEnum.Location.Air;

        if (movementType == EnemyEnum.MovementType.Walking) {
            if (isJustJumpedUp) {
                isJustJumpedUp = false;
            } else {
                if (!GetIsCurrentlyBeatingBack ()) {
                    StartFreeFall ();
                }
            }
        }
    }

    private void TouchWall (LifeEnum.HorizontalDirection wallPosition, bool isSlippyWall) {
        Log.PrintDebug (gameObject.name + " : TouchWall : isSlippyWall = " + isSlippyWall, LogType.Char);

        if (wallPosition != facingDirection) {
            // Somehow touch wall which is back to facing direction
            return;
        }

        ChangeFacingDirection ();
    }

    private void LeaveWall (bool isSlippyWall) {
        Log.PrintDebug (gameObject.name + " : LeaveWall", LogType.Char);
    }

    #endregion
}