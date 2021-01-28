using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO : Use Update to control enemy actions
// TODO : Ensure during beat back and die, no other action
// TODO : Ensure no action go after die
public abstract class EnemyModelBase : LifeBase {

    [SerializeField] private EnemyParams _param;
    public EnemyParams param => _param;
    [SerializeField] private Animator animator;

    public event Action<LifeEnum.HorizontalDirection> facingDirectionChangedEvent;
    public event Action diedEvent;

    protected override int posZ => GameVariable.EnemyPosZ;
    protected override int invincibleLayer => GameVariable.EnemyInvincibleLayer;
    protected override int totalHP => param.totalHP;

    private LifeEnum.HorizontalDirection? _facingDirection = null;
    public override LifeEnum.HorizontalDirection facingDirection {
        get { return (LifeEnum.HorizontalDirection)_facingDirection; }
        protected set {
            if (_facingDirection != value) {
                _facingDirection = value;
                facingDirectionChangedEvent?.Invoke (value);
            }
        }
    }

    public abstract EnemyEnum.MovementType movementType { get; }

    private LifeEnum.Location _currentLocation = LifeEnum.Location.Unknown;
    public override LifeEnum.Location currentLocation {
        get {
            return _currentLocation;
        }
        protected set {
            if (_currentLocation != value) {
                // Set _currentLocation first to prevent infinite loop
                var previousLocation = _currentLocation;
                _currentLocation = value;
                CurrentLocationChangedAction (previousLocation, _currentLocation);
            }
        }
    }

    // Status
    protected EnemyEnum.Status currentStatus;

    public override bool isBeatingBack {
        get {
            return (currentStatus & EnemyEnum.Status.BeatingBack) == EnemyEnum.Status.BeatingBack;
        }
        protected set {
            if (value) {
                currentStatus = currentStatus | EnemyEnum.Status.BeatingBack;
            } else {
                currentStatus = currentStatus & ~EnemyEnum.Status.BeatingBack;
            }
        }
    }

    public override bool isInvincible {
        get {
            return (currentStatus & EnemyEnum.Status.Invincible) == EnemyEnum.Status.Invincible;
        }
        protected set {
            if (value) {
                currentStatus = currentStatus | EnemyEnum.Status.Invincible;
            } else {
                currentStatus = currentStatus & ~EnemyEnum.Status.Invincible;
            }
        }
    }

    public override bool isDying {
        get {
            return (currentStatus & EnemyEnum.Status.Dying) == EnemyEnum.Status.Dying;
        }
        protected set {
            if (value) {
                currentStatus = currentStatus | EnemyEnum.Status.Dying;
            } else {
                currentStatus = currentStatus & ~EnemyEnum.Status.Dying;
            }
        }
    }

    protected override float invinciblePeriod => param.invinciblePeriod;

    public int id { get; private set; }
    public int collisionDP => param.collisionDP;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 beatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 WalkingBeatBackDirection_Right = new Vector2 (1, 0.577f).normalized;    // About 30 degree elevation
    private static Vector2 WalkingBeatBackDirection_Left = Vector2.Scale (WalkingBeatBackDirection_Right, new Vector2 (-1, 1));

    // Jump
    private bool isJumpRecursively => param.recursiveJumpPeriod >= 0;
    private bool isJustJumpedUp = false;
    private bool isPreparingToRecursiveJump = false;
    private float startPrepareRecursiveJumpTime = -1;

    private void Start () {
        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName) {
            Init (1, baseTransform.position, LifeEnum.HorizontalDirection.Right);
        }
    }

    public bool Init (MapData.EnemyData data) {
        return Init (data.id, data.GetPos(), data.GetDirection());
    }

    private bool Init (int id, Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Init (pos, direction);

        if (hasInitBefore) {
            return hasInitBefore;
        }

        this.id = id;
        currentStatus = EnemyEnum.Status.Normal;
        CheckAndPrepareRecursiveJump ();

        return hasInitBefore;
    }

    private void Update () {
        if (!isInitialized) {
            return;
        }

        if (isBeatingBack || isDying) {
            return;
        }

        DecideAction ();
    }

    /// <summary>
    /// Call at every frame after the script is initialized (except while beating back or dying)
    /// </summary>
    protected virtual void DecideAction () {
        // Jump
        if (isPreparingToRecursiveJump) {
            if (Time.time - startPrepareRecursiveJumpTime >= param.recursiveJumpPeriod) {
                if (Jump ()) {
                    isPreparingToRecursiveJump = false;
                } else {
                    // Try to jump again at next period
                    startPrepareRecursiveJumpTime = Time.time;
                }
            }
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

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        var isAlive = base.Hurt (dp, hurtDirection);

        Log.Print (gameObject.name + " : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + currentHP, LogType.Enemy);

        return isAlive;
    }

    private void DieByTouchedDeathTag () {
        Die (facingDirection);
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print (gameObject.name + " : Die!", LogType.Enemy);
        SetAnimatorTrigger (EnemyAnimConstant.DieTriggerName);
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        base.StartBeatingBack (hurtDirection);

        switch (movementType) {
            case EnemyEnum.MovementType.Walking:
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? WalkingBeatBackDirection_Left : WalkingBeatBackDirection_Right;
                break;
            case EnemyEnum.MovementType.Flying:
            default:
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? new Vector2 (-1, 0) : Vector2.one;
                break;
        }

        // If dying, dominated by die animation
        if (!isDying) {
            SetAnimatorTrigger (EnemyAnimConstant.BeatBackTriggerName);
        }
    }

    protected override void StopBeatingBack () {
        base.StopBeatingBack ();

        SetAnimatorTrigger (EnemyAnimConstant.DefaultTriggerName);
    }

    protected override void StartInvincible () {
        base.StartInvincible ();

        // If dying, dominated by die animation
        if (!isDying) {
            SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, true);
        }
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, false);
    }

    public virtual void DestroySelf () {
        diedEvent?.Invoke ();

        if (baseTransform.gameObject != null) {
            Destroy (baseTransform.gameObject);
        }
    }

    #endregion

    #region Location

    protected virtual void CurrentLocationChangedAction (LifeEnum.Location fromLocation, LifeEnum.Location toLocation) {
        if (movementType == EnemyEnum.MovementType.Walking) {
            if (fromLocation == LifeEnum.Location.Unknown && toLocation == LifeEnum.Location.Air) {
                StartFreeFall ();
            }
        }
    }

    #endregion

    #region Facing Direction

    protected void ChangeFacingDirection () {
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            facingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            facingDirection = LifeEnum.HorizontalDirection.Left;
        }
    }

    #endregion

    #region Jump

    private void CheckAndPrepareRecursiveJump () {
        if (movementType == EnemyEnum.MovementType.Walking && isJumpRecursively) {
            isPreparingToRecursiveJump = true;
            startPrepareRecursiveJumpTime = Time.time;
        }
    }

    /// <returns>Is jump success</returns>
    protected bool Jump () {
        Log.PrintDebug (gameObject.name + " : Jump", LogType.Enemy);

        if (currentLocation != LifeEnum.Location.Ground) {
            Log.Print (gameObject.name + " : Do not Jump. It is not in ground", LogType.Enemy);
            return false;
        }

        isJustJumpedUp = true;
        currentLocation = LifeEnum.Location.Air;
        SetAnimatorTrigger (EnemyAnimConstant.JumpTriggerName);

        return true;
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
        lifeCollision.TouchedWallEvent += TouchWall;
        lifeCollision.LeftWallEvent += LeaveWall;
        lifeCollision.TouchedDeathTagEvent += DieByTouchedDeathTag;
    }

    protected override void UnregisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent -= TouchGround;
        lifeCollision.LeftGroundEvent -= LeaveGround;
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
                    if (isDying) {
                        // Do nothing
                    } else if (isBeatingBack) {
                        StopBeatingBack ();
                    } else {
                        SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
                    }

                    CheckAndPrepareRecursiveJump ();
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
                if (!isBeatingBack) {
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