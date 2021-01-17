using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class EnemyModelBase : LifeBase {
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyParams _enemyParams;
    public EnemyParams enemyParams { get => _enemyParams; }

    public event Action<LifeEnum.HorizontalDirection> facingDirectionChangedEvent;

    public abstract EnemyEnum.MovementType movementType { get; }

    private bool isJustJumpedUp;

    public override bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Init (pos, direction);

        if (hasInitBefore) {
            return hasInitBefore;
        }

        isJustJumpedUp = false;

        return hasInitBefore;
    }

    #region Animtor

    private void SetAnimatorTrigger (string trigger) {
        Log.PrintDebug (gameObject.name + " : SetAnimatorTrigger : " + trigger, LogType.Enemy | LogType.Animation);
        animator.SetTrigger (trigger);
    }

    #endregion

    #region HP related

    public override bool Hurt (int dp) {
        var isAlive = base.Hurt (dp);

        if (isAlive) {
            // TODO : Hurt Animation
        }

        return isAlive;
    }

    protected override void Die () {
        base.Die ();

        // TODO
    }

    #endregion

    #region Movement Related

    #region Jump

    protected void Jump () {
        Log.PrintDebug (gameObject.name + " : Jump", LogType.Enemy);

        if (currentLocation != LifeEnum.Location.Ground) {
            Log.PrintWarning (gameObject.name + " : Jump failed. It is not in ground", LogType.Enemy);
            return;
        }

        isJustJumpedUp = true;
        currentLocation = LifeEnum.Location.Air;
        SetAnimatorTrigger (EnemyAnimConstant.JumpTriggerName);
    }

    #endregion

    #endregion

    #region Collision

    protected override void RegisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent += TouchGround;
        lifeCollision.LeftGroundEvent += LeaveGround;
        ////lifeCollision.TouchedRoofEvent    // No action for touch roof
        ////lifeCollision.LeftRoofEvent       // No action for leave roof
        //lifeCollision.TouchedWallEvent += TouchWall;
        //lifeCollision.LeftWallEvent += LeaveWall;
        //lifeCollision.TouchedDeathTagEvent += Die;
    }

    protected override void UnregisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent -= TouchGround;
        lifeCollision.LeftGroundEvent -= LeaveGround;
        ////lifeCollision.TouchedRoofEvent    // No action for touch roof
        ////lifeCollision.LeftRoofEvent       // No action for leave roof
        //lifeCollision.TouchedWallEvent -= TouchWall;
        //lifeCollision.LeftWallEvent -= LeaveWall;
        //lifeCollision.TouchedDeathTagEvent -= Die;
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
                SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
                break;
            default:
                // Do nothing
                break;
        }
    }

    private void LeaveGround () {
        Log.PrintDebug (gameObject.name + " : LeaveGround", LogType.Enemy);

        currentLocation = LifeEnum.Location.Air;

        if (movementType == EnemyEnum.MovementType.Walking) {
            if (isJustJumpedUp) {
                isJustJumpedUp = false;
            } else {
                StartFreeFall ();
            }
        }
    }

    private void StartFreeFall () {
        // TODO 
    }

    #endregion
}
