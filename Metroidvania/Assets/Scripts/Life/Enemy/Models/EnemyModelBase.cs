using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class EnemyModelBase : LifeBase , IMapTarget {

    #region Fields / Properties

    [SerializeField] private EnemyParams _params;
    public EnemyParams Params => _params;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAnimSMBUtils animUtils;
    [SerializeField] private Transform targetRefPoint;
    [SerializeField] private EnemyCharDetection charDetection;

    /// <summary>
    /// Input :<br />
    /// int : enemy id
    /// </summary>
    public static event Action<int> Died;
    public event Action<LifeEnum.HorizontalDirection> FacingDirectionChanged;

    /// <summary>
    /// Input :<br />
    /// Vector2 : normalized direction
    /// float : max speed
    /// float? : acceleration
    /// </summary>
    public event Action<Vector2, float, float?> FlyingInfoUpdated;

    public abstract EnemyEnum.EnemyType EnemyType { get; }
    public abstract EnemyEnum.MovementType MovementType { get; }

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
        get { return GetIsInStatuses (EnemyEnum.Statuses.BeatingBack); }
        protected set { SetStatuses (EnemyEnum.Statuses.BeatingBack, value); }
    }

    public override bool IsInvincible {
        get { return GetIsInStatuses (EnemyEnum.Statuses.Invincible); }
        protected set { SetStatuses (EnemyEnum.Statuses.Invincible, value); }
    }

    public override bool IsDying {
        get { return GetIsInStatuses (EnemyEnum.Statuses.Dying); }
        protected set { SetStatuses (EnemyEnum.Statuses.Dying, value); }
    }

    protected override float InvinciblePeriod => Params.InvinciblePeriod;

    public int Id { get; private set; }

    private bool isIdling = false;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 BeatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 WalkingBeatBackDirection_Right = new Vector2 (1, 0.577f).normalized;    // About 30 degree elevation
    private static Vector2 WalkingBeatBackDirection_Left = Vector2.Scale (WalkingBeatBackDirection_Right, new Vector2 (-1, 1));
    private bool isJustBeatingBack = false;

    // Jump
    protected bool IsJustJumpedUp { get; set; } = false;
    private bool isPreparingToRecursiveJump = false;
    private float startPrepareRecursiveJumpTime = -1;

    // Chase Char
    private float startChasingCharTime = -100;

    #endregion

    #region Initialization related

    private void Start () {
        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName ||
            SceneManager.GetActiveScene ().name == GameVariable.SandboxSceneName) {
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

        var hasInitBefore = Reset (pos, direction);

        if (!hasInitBefore) {
            this.Id = id;
            
            if (Params.IsDetectChar) {
                charDetection.CharDetected += CharDetectedHandler;
                charDetection.CharLost += CharLostHandler;
                charDetection.SetActive (true);
            } else {
                charDetection.SetActive (false);
            }
        }

        CurrentStatuses = EnemyEnum.Statuses.Normal;
        PrepareRecursiveJump ();
        StartIdleOrMove ();

        return hasInitBefore;
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        if (IsInitialized) {
            if (Params.IsDetectChar) {
                charDetection.CharDetected -= CharDetectedHandler;
                charDetection.CharLost -= CharLostHandler;
            }
        }
    }

    #endregion

    protected override void Update () {
        if (!IsInitialized) {
            return;
        }

        base.Update ();

        DecideAction ();

        isJustBeatingBack = false;
    }

    #region Statuses

    private void SetStatuses (EnemyEnum.Statuses statuses, bool isOn) {
        if (isOn) {
            CurrentStatuses = CurrentStatuses | statuses;
        } else {
            CurrentStatuses = CurrentStatuses & ~statuses;
        }
    }

    /// <summary>
    /// If <paramref name="statuses"/> is composite, it will return true only when CurrentStatuses contains all those status
    /// </summary>
    /// <param name="statuses"></param>
    /// <returns></returns>
    protected bool GetIsInStatuses (EnemyEnum.Statuses statuses) {
        return (CurrentStatuses & statuses) == statuses;
    }

    public void SetCheckChasingStatus (bool isCheckChasing) {
        SetStatuses (EnemyEnum.Statuses.CheckChasing, isCheckChasing);
    }

    #endregion

    #region DecideAction

    /// <summary>
    /// Call at every frame after the script is initialized
    /// </summary>
    /// <returns>Can do action or not</returns>
    protected virtual bool DecideAction () {
        if (IsBeatingBack || IsDying) {
            return false;
        }

        if (isIdling) {
            if (GetIsInStatuses (EnemyEnum.Statuses.DetectedChar)) {
                StartIdleOrMove ();
            }

            return false;
        }

        if (!GetIsInStatuses (EnemyEnum.Statuses.DetectedChar)) {
            if (!isIdling) {
                switch (MovementType) {
                    case EnemyEnum.MovementType.Walking:
                        // Do not set idle if the walking enemy is not on ground (e.g. jumping)
                        if (CurrentLocation == LifeEnum.Location.Ground) {
                            StartIdling ();
                        }
                        break;
                    case EnemyEnum.MovementType.Flying:
                    default:
                        StartIdling ();
                        break;
                }
            }

            return false;
        }

        CheckAndUpdateChaseCharDetails ();
        var isTriggeredJump = CheckAndDoRecursiveJump ();
        if (isTriggeredJump) {
            return false;
        }

        return true;
    }

    #endregion

    #region Idle / Move

    protected void StartIdleOrMove () {
        if (Params.IsDetectChar) {
            if (GetIsInStatuses (EnemyEnum.Statuses.DetectedChar)) {
                StartMoving ();
            } else {
                StartIdling ();
            }
        } else {
            SetStatuses (EnemyEnum.Statuses.DetectedChar, true);
            StartMoving ();
        }
        
    }
    protected void StartIdling () {
        isIdling = true;
        SetAnimatorTrigger (EnemyAnimConstant.IdleTriggerName);
    }

    protected void StartMoving () {
        isIdling = false;
        SetAnimatorTrigger (EnemyAnimConstant.MoveTriggerName);
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

    #endregion

    #region HP related

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection, bool isFireAttack = false) {
        var isAlive = base.Hurt (dp, hurtDirection, isFireAttack);

        Log.Print (gameObject.name + " : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + CurrentHP, LogTypes.Enemy);

        return isAlive;
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print (gameObject.name + " : Die!", LogTypes.Enemy);
        SetAnimatorTrigger (EnemyAnimConstant.DieTriggerName);
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        if (Params.BeatBackInitSpeed <= 0) {
            return;
        }

        isJustBeatingBack = true;

        base.StartBeatingBack (hurtDirection);

        switch (MovementType) {
            case EnemyEnum.MovementType.Walking:
                BeatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? WalkingBeatBackDirection_Left : WalkingBeatBackDirection_Right;
                break;
            case EnemyEnum.MovementType.Flying:
            default:
                BeatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? Vector2.left : Vector2.right;
                break;
        }

        // If dying, dominated by die animation
        if (!IsDying) {
            SetAnimatorTrigger (EnemyAnimConstant.BeatBackTriggerName);
        }
    }

    protected override void StopBeatingBack () {
        base.StopBeatingBack ();

        StartIdleOrMove ();
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

        if (MovementType == EnemyEnum.MovementType.Flying && IsBeatingBack) {
            StopBeatingBack ();
        }
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

    private void PrepareRecursiveJump () {
        if (MovementType == EnemyEnum.MovementType.Walking && Params.IsJumpRecursively) {
            isPreparingToRecursiveJump = true;
            startPrepareRecursiveJumpTime = Time.time;
        }
    }

    /// <returns>Is triggered jump</returns>
    protected virtual bool CheckAndDoRecursiveJump () {
        var isTriggeredJump = false;

        if (isPreparingToRecursiveJump) {
            if (Time.time - startPrepareRecursiveJumpTime >= Params.RecursiveJumpPeriod) {
                if (Jump ()) {
                    isPreparingToRecursiveJump = false;
                    isTriggeredJump = true;
                } else {
                    // Try to jump again at next period
                    startPrepareRecursiveJumpTime = Time.time;
                }
            }
        }

        return isTriggeredJump;
    }

    /// <returns>Is jump success</returns>
    protected virtual bool Jump () {
        Log.PrintDebug (gameObject.name + " : Jump", LogTypes.Enemy);

        if (CurrentLocation != LifeEnum.Location.Ground) {
            Log.Print (gameObject.name + " : Do not Jump. It is not in ground", LogTypes.Enemy);
            return false;
        }

        IsJustJumpedUp = true;
        SetAnimatorTrigger (EnemyAnimConstant.JumpTriggerName);

        return true;
    }

    private void StartFreeFall () {
        SetAnimatorTrigger (EnemyAnimConstant.FreeFallTriggerName);
    }

    #endregion

    #region Char detection

    private void CharDetectedHandler () {
        SetStatuses (EnemyEnum.Statuses.DetectedChar, true);
    }

    private void CharLostHandler () {
        if (Params.WillLoseTrackOfChar) {
            SetStatuses (EnemyEnum.Statuses.DetectedChar, false);
        }
    }

    #endregion

    #region Char chasing

    protected void OnFlyingInfoUpdated (Vector2 normalizedChasingDirection, float maxSpeed, float? acceleration) {
        FlyingInfoUpdated?.Invoke (normalizedChasingDirection, maxSpeed, acceleration);
    }

    protected virtual void CheckAndUpdateChaseCharDetails () {
        if (!Params.IsChaseChar) {
            return;
        }

        if (!GetIsInStatuses (EnemyEnum.Statuses.CheckChasing)) {
            return;
        }

        if (Time.time - startChasingCharTime < Params.ChangeChaseCharDirPeriod) {
            return;
        }

        Vector2 normalizedChasingDirection;
        switch (MovementType) {
            case EnemyEnum.MovementType.Walking:
                var horizontalDirection = GetChasingCharHorizontalDirection ();
                normalizedChasingDirection = horizontalDirection == LifeEnum.HorizontalDirection.Left ? Vector2.left : Vector2.right;
                break;
            case EnemyEnum.MovementType.Flying:
            default:
                normalizedChasingDirection = GetChasingCharDirection (true);
                float? acceleration = null;
                if (Params.IsSpeedAccelerate) {
                    acceleration = Params.Acceleration;
                }
                OnFlyingInfoUpdated (normalizedChasingDirection, Params.MaxMovementSpeed, acceleration);
                break;
        }

        startChasingCharTime = Time.time;

        if (FacingDirection == LifeEnum.HorizontalDirection.Left && normalizedChasingDirection.x > 0) {
            ChangeFacingDirection ();
        } else if (FacingDirection == LifeEnum.HorizontalDirection.Right && normalizedChasingDirection.x < 0) {
            ChangeFacingDirection ();
        }
    }

    protected Vector2 GetChasingCharDirection (bool isNormalized) {
        var charModel = GameUtils.FindOrSpawnChar ();
        var distVector = (Vector2)charModel.GetPos () - (Vector2)GetPos ();

        if (isNormalized) {
            return distVector.normalized;
        } else {
            return distVector;
        }
    }

    protected LifeEnum.HorizontalDirection GetChasingCharHorizontalDirection () {
        var charModel = GameUtils.FindOrSpawnChar ();
        if (charModel.GetPos ().x >= GetPos ().x) {
            return LifeEnum.HorizontalDirection.Right;
        } else {
            return LifeEnum.HorizontalDirection.Left;
        }
    }

    #endregion

    #region Collision Related

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

        if (isJustBeatingBack) {
            Log.PrintDebug (gameObject.name + " : It is just triggered beating back. Wait a frame for beat back to take effect.", LogTypes.Enemy | LogTypes.Collision);
            CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
            return;
        }

        // Check for touching wall
        if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
            TouchWallAction (wallPosition);
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

                        if (IsJustJumpedUp) {
                            // Dominated by jump handling
                            IsJustJumpedUp = false;
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
                            if (animUtils.RB.velocity.y > GameVariable.EpsilonForPhysicsChecking) {
                                Log.PrintWarning (gameObject.name + " : The walking movement type enemy is trying to move away from the ground due to beating back, so do not do any touch ground action.", LogTypes.Enemy | LogTypes.Collision);
                            } else {
                                StopBeatingBack ();
                            }
                        } else {
                            SetAnimatorTrigger (EnemyAnimConstant.LandingTriggerName);
                        }

                        PrepareRecursiveJump ();
                    }
                }
                break;
        }
    }

    protected virtual void TouchWallAction (LifeEnum.HorizontalDirection wallPosition) {
        Log.PrintDebug (gameObject.name + " : Touch wall", LogTypes.Enemy | LogTypes.Collision);
        if (!Params.IsChaseChar && MovementType == EnemyEnum.MovementType.Walking) {
            if (wallPosition == FacingDirection) {
                // Change facing direction only when the enemy originally face towards wall
                ChangeFacingDirection ();
            }
        }
    }

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