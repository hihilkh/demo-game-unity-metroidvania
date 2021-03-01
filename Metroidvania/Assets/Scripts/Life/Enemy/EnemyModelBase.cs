using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO : Use Update to control enemy actions
// TODO : Ensure during beat back and die, no other action
// TODO : Ensure no action go after die
public abstract class EnemyModelBase : LifeBase , IMapTarget {

    #region Fields / Properties

    [SerializeField] private EnemyParams _params;
    public EnemyParams Params => _params;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform targetRefPoint;

    /// <summary>
    /// Input :<br />
    /// int : enemy id
    /// </summary>
    public static event Action<int> Died;
    public event Action<LifeEnum.HorizontalDirection> FacingDirectionChanged;


    protected override int PosZ => GameVariable.EnemyPosZ;
    protected override int InvincibleLayer => GameVariable.EnemyInvincibleLayer;
    protected override int TotalHP => Params.TotalHP;

    private LifeEnum.HorizontalDirection? _facingDirection = null;
    public override LifeEnum.HorizontalDirection FacingDirection {
        get {
            if (_facingDirection == null) {
                return default;
            } else {
                return (LifeEnum.HorizontalDirection)_facingDirection;
            }
        }
        protected set {
            if (_facingDirection != value) {
                _facingDirection = value;
                FacingDirectionChanged?.Invoke (value);
            }
        }
    }

    public abstract EnemyEnum.MovementType MovementType { get; }

    private LifeEnum.Location _currentLocation = LifeEnum.Location.Unknown;
    public override LifeEnum.Location CurrentLocation {
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
    protected EnemyEnum.Statuses CurrentStatuses { get; private set; }

    public override bool IsBeatingBack {
        get {
            return (CurrentStatuses & EnemyEnum.Statuses.BeatingBack) == EnemyEnum.Statuses.BeatingBack;
        }
        protected set {
            if (value) {
                CurrentStatuses = CurrentStatuses | EnemyEnum.Statuses.BeatingBack;
            } else {
                CurrentStatuses = CurrentStatuses & ~EnemyEnum.Statuses.BeatingBack;
            }
        }
    }

    public override bool IsInvincible {
        get {
            return (CurrentStatuses & EnemyEnum.Statuses.Invincible) == EnemyEnum.Statuses.Invincible;
        }
        protected set {
            if (value) {
                CurrentStatuses = CurrentStatuses | EnemyEnum.Statuses.Invincible;
            } else {
                CurrentStatuses = CurrentStatuses & ~EnemyEnum.Statuses.Invincible;
            }
        }
    }

    public override bool IsDying {
        get {
            return (CurrentStatuses & EnemyEnum.Statuses.Dying) == EnemyEnum.Statuses.Dying;
        }
        protected set {
            if (value) {
                CurrentStatuses = CurrentStatuses | EnemyEnum.Statuses.Dying;
            } else {
                CurrentStatuses = CurrentStatuses & ~EnemyEnum.Statuses.Dying;
            }
        }
    }

    protected override float InvinciblePeriod => Params.InvinciblePeriod;

    public int Id { get; private set; }
    public int CollisionDP => Params.CollisionDP;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 BeatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 WalkingBeatBackDirection_Right = new Vector2 (1, 0.577f).normalized;    // About 30 degree elevation
    private static Vector2 WalkingBeatBackDirection_Left = Vector2.Scale (WalkingBeatBackDirection_Right, new Vector2 (-1, 1));

    // Jump
    private bool IsJumpRecursively => Params.RecursiveJumpPeriod >= 0;
    private bool isJustJumpedUp = false;
    private bool isPreparingToRecursiveJump = false;
    private float startPrepareRecursiveJumpTime = -1;

    #endregion

    #region Initialization related

    private void Start () {
        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName) {
            Reset (1, BaseTransform.position, LifeEnum.HorizontalDirection.Right);
        }
    }

    /// <summary>
    /// If not yet initialized, it will initialize. Otherwise, it will reset.
    /// </summary>
    /// <returns>has initialized before</returns>
    public bool Reset (MapData.EnemyData data) {
        return Reset (data.id, data.pos, data.direction);
    }

    /// <summary>
    /// If not yet initialized, it will initialize. Otherwise, it will reset.
    /// </summary>
    /// <returns>has initialized before</returns>
    private bool Reset (int id, Vector2 pos, LifeEnum.HorizontalDirection direction) {
        // Remarks :
        // Currently Reset() may not work properly (and current approach would always dispose and re-create enemy if needed, but not reset)
        // If want to use reset, need to check all the cases (e.g. enemy died/beating back/invincible/jumping and reset)

        var hasInitBefore = base.Reset (pos, direction);

        if (!hasInitBefore) {
            this.Id = id;
        }

        CurrentStatuses = EnemyEnum.Statuses.Normal;
        CheckAndPrepareRecursiveJump ();

        return hasInitBefore;
    }

    #endregion

    protected override void Update () {
        if (!IsInitialized) {
            return;
        }

        base.Update ();

        DecideAction ();
    }

    private void LateUpdate () {
        ResetUntriggeredAnimatorTriggers ();
    }

    #region DecideAction

    /// <summary>
    /// Call at every frame after the script is initialized
    /// </summary>
    /// <returns>Can do action or not</returns>
    protected virtual bool DecideAction () {
        if (IsBeatingBack || IsDying) {
            return false;
        }

        // Jump
        if (isPreparingToRecursiveJump) {
            if (Time.time - startPrepareRecursiveJumpTime >= Params.RecursiveJumpPeriod) {
                if (Jump ()) {
                    isPreparingToRecursiveJump = false;
                } else {
                    // Try to jump again at next period
                    startPrepareRecursiveJumpTime = Time.time;
                }
            }
        }

        return true;
    }

    #endregion

    #region Animtor

    protected void SetAnimatorTrigger (string triggerName) {
        Log.PrintDebug (gameObject.name + " : SetAnimatorTrigger : " + triggerName, LogTypes.Enemy | LogTypes.Animation);
        animator.SetTrigger (triggerName);
    }

    protected void SetAnimatorBool (string boolName, bool value) {
        Log.PrintDebug (gameObject.name + " : SetAnimatorBool : " + boolName + " ; Value : " + value, LogTypes.Enemy | LogTypes.Animation);
        animator.SetBool (boolName, value);
    }

    private void ResetUntriggeredAnimatorTriggers () {
        animator.ResetTrigger (EnemyAnimConstant.LandingTriggerName);
    }

    #endregion

    #region HP related

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        var isAlive = base.Hurt (dp, hurtDirection);

        Log.Print (gameObject.name + " : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + CurrentHP, LogTypes.Enemy);

        return isAlive;
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print (gameObject.name + " : Die!", LogTypes.Enemy);
        SetAnimatorTrigger (EnemyAnimConstant.DieTriggerName);
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        base.StartBeatingBack (hurtDirection);

        switch (MovementType) {
            case EnemyEnum.MovementType.Walking:
                BeatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? WalkingBeatBackDirection_Left : WalkingBeatBackDirection_Right;
                break;
            case EnemyEnum.MovementType.Flying:
            default:
                BeatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? new Vector2 (-1, 0) : Vector2.one;
                break;
        }

        // If dying, dominated by die animation
        if (!IsDying) {
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
        if (!IsDying) {
            SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, true);
        }
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        SetAnimatorBool (EnemyAnimConstant.InvincibleBoolName, false);
    }

    public virtual void DestroySelf (bool isDie) {
        if (isDie) {
            Died?.Invoke (Id);
        }

        if (BaseTransform.gameObject != null) {
            Destroy (BaseTransform.gameObject);
        }
    }

    #endregion

    #region Location

    protected virtual void CurrentLocationChangedAction (LifeEnum.Location fromLocation, LifeEnum.Location toLocation) {
        if (MovementType == EnemyEnum.MovementType.Walking) {
            if (fromLocation == LifeEnum.Location.Unknown && toLocation == LifeEnum.Location.Air) {
                StartFreeFall ();
            }
        }
    }

    #endregion

    #region Facing Direction

    protected void ChangeFacingDirection () {
        if (FacingDirection == LifeEnum.HorizontalDirection.Left) {
            FacingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            FacingDirection = LifeEnum.HorizontalDirection.Left;
        }
    }

    #endregion

    #region Jump

    private void CheckAndPrepareRecursiveJump () {
        if (MovementType == EnemyEnum.MovementType.Walking && IsJumpRecursively) {
            isPreparingToRecursiveJump = true;
            startPrepareRecursiveJumpTime = Time.time;
        }
    }

    /// <returns>Is jump success</returns>
    protected bool Jump () {
        Log.PrintDebug (gameObject.name + " : Jump", LogTypes.Enemy);

        if (CurrentLocation != LifeEnum.Location.Ground) {
            Log.Print (gameObject.name + " : Do not Jump. It is not in ground", LogTypes.Enemy);
            return false;
        }

        isJustJumpedUp = true;
        CurrentLocation = LifeEnum.Location.Air;
        SetAnimatorTrigger (EnemyAnimConstant.JumpTriggerName);

        return true;
    }

    private void StartFreeFall () {
        CurrentLocation = LifeEnum.Location.Air;

        SetAnimatorTrigger (EnemyAnimConstant.FreeFallTriggerName);
    }

    #endregion

    #region Collision Events

    protected override void HandleCollision (CollisionAnalysis collisionAnalysis) {
        var collisionDetailsDict = collisionAnalysis.CollisionDetailsDict;

        var isNowTouchingGround = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Ground);
        var isNowTouchingWall = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Wall);

        // Only get the first collision details among Wall and SlippyWall collision
        var wallPosition = LifeEnum.HorizontalDirection.Left;
        if (isNowTouchingWall) {
            var wallCollisionDetails = collisionDetailsDict[LifeEnum.CollisionType.Wall][0];
            wallPosition = wallCollisionDetails.CollisionNormal.x < 0 ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
        }

        if (IsDying) {
            CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
            return;
        }

        // Check for death
        if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Death)) {
            Die (FacingDirection);
            CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
            return;
        }

        // Check for touching wall
        if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
            Log.PrintDebug (gameObject.name + " : Touch wall", LogTypes.Enemy | LogTypes.Collision);
            if (wallPosition == FacingDirection) {
                // Change facing direction only when the enemy originally face towards wall
                ChangeFacingDirection ();
            }
        }

        // Changing current location
        switch (CurrentLocation) {
            case LifeEnum.Location.Unknown:
                if (isNowTouchingGround) {
                    Log.PrintDebug (gameObject.name + " : Touching ground while Location = Unknown", LogTypes.Enemy | LogTypes.Collision);
                    CurrentLocation = LifeEnum.Location.Ground;
                }
                break;
            case LifeEnum.Location.Ground:
                if (collisionAnalysis.GroundCollisionChangedType == LifeEnum.CollisionChangedType.Exit) {
                    // Ground -> Air
                    Log.PrintDebug (gameObject.name + " : Leave ground", LogTypes.Enemy | LogTypes.Collision);
                    CurrentLocation = LifeEnum.Location.Air;

                    if (MovementType == EnemyEnum.MovementType.Walking) {
                        if (IsBeatingBack) {
                            // If it is beating back, behaviour is dominated by beat back handling
                            break;
                        }

                        if (isJustJumpedUp) {
                            // Dominated by jump handling
                            isJustJumpedUp = false;
                        } else {
                            StartFreeFall ();
                        }
                    }
                }
                break;
            case LifeEnum.Location.Air:
                if (collisionAnalysis.GroundCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                    // Air -> Ground
                    Log.PrintDebug (gameObject.name + " : Touch ground", LogTypes.Enemy | LogTypes.Collision);
                    CurrentLocation = LifeEnum.Location.Ground;

                    if (MovementType == EnemyEnum.MovementType.Walking) {
                        if (IsDying) {
                            // Do nothing
                            break;
                        }

                        if (IsBeatingBack) {
                            StopBeatingBack ();
                        } else {
                            SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
                        }

                        CheckAndPrepareRecursiveJump ();
                    }
                }
                break;
        }
    }

    protected override void AddCollisionEventHandlers () {
        //CollisionScript.TouchedGround += TouchedGroundHandler;
        //CollisionScript.LeftGround += LeftGroundHandler;
        //CollisionScript.TouchedWall += TouchedWallHandler;
        //CollisionScript.LeftWall += LeftWallHandler;
        //CollisionScript.TouchedDeathTag += TouchedDeathTagHandler;
    }

    protected override void RemoveCollisionEventHandlers () {
        //CollisionScript.TouchedGround -= TouchedGroundHandler;
        //CollisionScript.LeftGround -= LeftGroundHandler;
        //CollisionScript.TouchedWall -= TouchedWallHandler;
        //CollisionScript.LeftWall -= LeftWallHandler;
        //CollisionScript.TouchedDeathTag -= TouchedDeathTagHandler;
    }

    //private void TouchedGroundHandler () {
    //    Log.PrintDebug (gameObject.name + " : TouchGround", LogTypes.Enemy);

    //    // Touch ground while init / set position
    //    if (CurrentLocation == LifeEnum.Location.Unknown) {
    //        CurrentLocation = LifeEnum.Location.Ground;
    //        return;
    //    }

    //    switch (CurrentLocation) {
    //        case LifeEnum.Location.Air:
    //            if (MovementType == EnemyEnum.MovementType.Walking) {
    //                if (IsDying) {
    //                    // Do nothing
    //                } else if (IsBeatingBack) {
    //                    StopBeatingBack ();
    //                } else {
    //                    SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
    //                }

    //                CheckAndPrepareRecursiveJump ();
    //            }
    //            break;
    //        default:
    //            // Do nothing
    //            break;
    //    }

    //    CurrentLocation = LifeEnum.Location.Ground;
    //}

    //private void LeftGroundHandler () {
    //    Log.PrintDebug (gameObject.name + " : LeaveGround", LogTypes.Enemy);

    //    CurrentLocation = LifeEnum.Location.Air;

    //    if (MovementType == EnemyEnum.MovementType.Walking) {
    //        if (isJustJumpedUp) {
    //            isJustJumpedUp = false;
    //        } else {
    //            if (!IsBeatingBack) {
    //                StartFreeFall ();
    //            }
    //        }
    //    }
    //}

    //private void TouchedWallHandler (LifeEnum.HorizontalDirection wallPosition, bool isSlippyWall) {
    //    Log.PrintDebug (gameObject.name + " : TouchWall : isSlippyWall = " + isSlippyWall, LogTypes.Char);

    //    if (wallPosition != FacingDirection) {
    //        // Somehow touch wall which is back to facing direction
    //        return;
    //    }

    //    ChangeFacingDirection ();
    //}

    //private void LeftWallHandler (bool isSlippyWall) {
    //    Log.PrintDebug (gameObject.name + " : LeaveWall", LogTypes.Char);
    //}

    //private void TouchedDeathTagHandler () {
    //    Die (FacingDirection);
    //}

    #endregion

    #region IMapTarget

    public Vector2 GetTargetPos () {
        return targetRefPoint.position;
    }

    #endregion

    #region MapDisposableBase

    protected override void Dispose () {
        DestroySelf (false);
    }

    #endregion
}