using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharModel : LifeBase, IMapTarget {

    #region private class / enum

    private enum HandleCollisionAnimation {
        None,
        Sliding,
        IdleOrWalkOrFreeFall,
        Landing,
    }

    private class HandleCollisionResult {
        public bool IsIgnoreCommand;
        public bool IsJustChangedDirection;
        public bool IsJustStoppedDashing;

        public HandleCollisionResult () {
            IsIgnoreCommand = false;
            IsJustChangedDirection = false;
            IsJustStoppedDashing = false;
        }
    }

    #endregion

    #region Fields / Properties

    [SerializeField] private CharParams _params;
    public CharParams Params => _params;
    [SerializeField] private CharController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private CharCameraModel cameraModel;
    [SerializeField] private Transform targetRefPoint;
    [SerializeField] private Transform collectCollectableRefPoint;

    // event
    public event Action Resetting;
    public event Action<CharEnum.BodyParts> ObtainedBodyPartsChanged;
    public event Action StatusesChanged;
    public event Action FacingDirectionChanged;
    public event Action MovingDirectionChanged;
    public event Action HorizontalSpeedChanged;
    public event Action Died;

    private readonly Dictionary<CharEnum.InputSituation, CharEnum.Command> commandSettings = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();

    private MapManager mapManager;

    // Body Parts
    public CharEnum.BodyParts ObtainedBodyParts { get; private set; }

    protected override int PosZ => GameVariable.CharPosZ;
    protected override int InvincibleLayer => GameVariable.PlayerInvincibleLayer;
    protected override int TotalHP => Params.TotalHP;

    // Statuses
    private CharEnum.Statuses _currentStatuses = CharEnum.Statuses.Normal;
    protected CharEnum.Statuses CurrentStatuses {
        get {
            return _currentStatuses;
        }
        set {
            if (_currentStatuses != value) {
                _currentStatuses = value;
                StatusesChanged?.Invoke ();
            }
        }
    }

    public override bool IsBeatingBack {
        get { return GetIsInStatuses (CharEnum.Statuses.BeatingBack); }
        protected set { SetStatuses (CharEnum.Statuses.BeatingBack, value); }
    }

    public override bool IsInvincible {
        get { return GetIsInStatuses (CharEnum.Statuses.Invincible); }
        protected set { SetStatuses (CharEnum.Statuses.Invincible, value); }
    }

    public override bool IsDying {
        get { return GetIsInStatuses (CharEnum.Statuses.Dying); }
        protected set { SetStatuses (CharEnum.Statuses.Dying, value); }
    }

    protected override float InvinciblePeriod => Params.InvinciblePeriod;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 BeatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 BeatBackDirection_Right = new Vector2 (1, 1).normalized;    // About 45 degree elevation
    private static Vector2 BeatBackDirection_Left = Vector2.Scale (BeatBackDirection_Right, new Vector2 (-1, 1));

    // HP Recovery
    private Coroutine hpRecoveryCoroutine;

    // Character Situation
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
                SetAnimatorTrigger (CharAnimConstant.StopUpperPartTriggerName);
                FacingDirectionChanged?.Invoke ();
            }
        }
    }

    private LifeEnum.HorizontalDirection? _movingDirection = null;
    public LifeEnum.HorizontalDirection MovingDirection {
        get {
            if (_movingDirection == null) {
                return default;
            } else {
                return (LifeEnum.HorizontalDirection)_movingDirection;
            }
        }
        protected set {
            if (_movingDirection != value) {
                _movingDirection = value;
                MovingDirectionChanged?.Invoke ();
            }
        }
    }

    private CharEnum.HorizontalSpeed? _currentHorizontalSpeed = null;
    public CharEnum.HorizontalSpeed CurrentHorizontalSpeed {
        get {
            if (_currentHorizontalSpeed == null) {
                return default;
            } else {
                return (CharEnum.HorizontalSpeed)_currentHorizontalSpeed;
            }
        }
        protected set {
            if (_currentHorizontalSpeed != value) {
                _currentHorizontalSpeed = value;
                HorizontalSpeedChanged?.Invoke ();
            }
        }
    }

    public CharEnum.HitType? CurrentHitType { get; private set; }
    public CharEnum.ArrowType? CurrentArrowType { get; private set; }
    private bool isAllowMove;
    private bool isAllowAirJump;
    private bool isWaitingLandingToStopChar;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;
    //private bool isIgnoreUserInputInThisFrame;

    // Command Control
    private CharEnum.InputSituation? currentInputSituation;
    private CharEnum.Command? currentCommand;
    private bool isIgnoreHold;
    private bool isIgnoreRelease;

    // Jump Command Control
    private bool isJustJumpedUp;

    // Dash Command Control
    private Coroutine dashCoroutine;
    private Coroutine dashCoolDownCoroutine;

    // Hit / Arrow Command Control
    private Coroutine attackCoolDownCoroutine;

    // Collision
    //private bool isJustTouchWall;
    //private bool isTouchingWall;
    private HandleCollisionResult currentHandleCollisionResult;

    // Mission Event
    private Action missionEventInputFinishedAction = null;

    #endregion

    #region Initialization related

    protected override void Awake () {
        base.Awake ();

        ObtainedBodyParts = UserManager.GetObtainedBodyParts ();

        controller.Tapped += TappedHandler;
        controller.StartedHold += StartedHoldHandler;
        controller.StoppedHold += StoppedHoldHandler;

        DontDestroyOnLoad (this);
    }

    private void Start () {
        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName) {
            Reset (BaseTransform.position, Params.InitDirection);
            SetAllowMove (true);
        }
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        controller.Tapped -= TappedHandler;
        controller.StartedHold -= StartedHoldHandler;
        controller.StoppedHold -= StoppedHoldHandler;
    }

    public void EnterGameScene (MapManager mapManager, MapData.Boundary boundary) {
        gameObject.SetActive (true);
        this.mapManager = mapManager;
        cameraModel.SetMissionBoundaries (boundary.lowerBound, boundary.upperBound);
        cameraModel.SetAudioListener (true);
    }

    public void LeaveGameScene () {
        this.mapManager = null;
        cameraModel.UnsetMissionBoundaries ();
        cameraModel.SetAudioListener (false);
        gameObject.SetActive (false);

        missionEventInputFinishedAction = null;
    }

    /// <summary>
    /// If not yet initialized, it will initialize. Otherwise, it will reset.
    /// </summary>
    /// <returns>has initialized before</returns>
    public bool Reset (MapData.EntryData entryData) {
        cameraModel.ResetPos ();
        return Reset (entryData.pos, entryData.direction);
    }

    /// <summary>
    /// If not yet initialized, it will initialize. Otherwise, it will reset.
    /// </summary>
    /// <returns>has initialized before</returns>
    new public bool Reset (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        Resetting?.Invoke ();

        var hasInitBefore = base.Reset (pos, direction);

        ResetFlags ();
        SetAllowMove (false);

        return hasInitBefore;
    }

    private void ResetFlags () {
        CurrentStatuses = CharEnum.Statuses.Normal;

        CurrentHitType = null;
        CurrentArrowType = null;
        isAllowAirJump = true;
        isWaitingLandingToStopChar = false;

        isJustJumpedUp = false;

        if (dashCoroutine != null) {
            StopCoroutine (dashCoroutine);
            dashCoroutine = null;
        }

        if (attackCoolDownCoroutine != null) {
            StopCoroutine (attackCoolDownCoroutine);
            attackCoolDownCoroutine = null;
        }

        //isTouchingWall = false;

        SetCurrentCommandStatus (null, null);

        SetHPRecovery (false);

        StopInvincible ();

        ResetAllUpdateControlFlags ();
    }

    private void ResetAllUpdateControlFlags () {
        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;

        currentInputSituation = null;
        currentCommand = null;
        isIgnoreHold = false;
        isIgnoreRelease = false;

        //isJustTouchWall = false;
        //isIgnoreUserInputInThisFrame = false;
    }

    protected override IEnumerator ResetCurrentLocation () {
        yield return StartCoroutine (base.ResetCurrentLocation ());

        // To prevent error if the character is initialized in air. Mainly for development convenience.
        if (CurrentLocation == LifeEnum.Location.Air) {
            ResetAnimatorTrigger (CharAnimConstant.IdleTriggerName);
            ResetAnimatorTrigger (CharAnimConstant.WalkTriggerName);
            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
            StartFreeFall ();
        }
    }

    protected override void SetPosAndDirection (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        base.SetPosAndDirection (pos, direction);
        MovingDirection = FacingDirection;
    }

    #endregion

    // Remarks :
    // Currently all physics is with sharp changes, so they are stick on Update().
    // Change to stick on FixedUpdate() if continuous changes is needed.
    protected override void Update () {
        // Ordering:
        // 1. base.Update () (HandleCollision)
        // 2. check isAllowMove (After HandleCollision because sometimes it would set isAllowMove = false but have changes due to collision)
        // 3. Handle command
        // 4. Reset flags

        base.Update ();

        if (!isAllowMove) {
            return;
        }

        var situation = GetCurrentInputSituation ();
        HandleCommand (situation, currentHandleCollisionResult);

        // TODO : Check old logic :
        // slippy wall keep dashing towards it

        // TODO : Check new logic :
        // different Dash + TouchWall combination
        // isIgnoreUserInputInThisFrame

        //HandleCommand (situation);

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;
        //isJustTouchWall = false;
        //isIgnoreUserInputInThisFrame = false;

        if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            isIgnoreHold = false;
            isIgnoreRelease = false;
        }
    }

    private void LateUpdate () {
        ResetUntriggeredAnimatorTriggers ();
    }

    public void SetAllowMove (bool isAllowMove) {
        this.isAllowMove = isAllowMove;

        SetAllowUserControl (isAllowMove);

        switch (CurrentLocation) {
            case LifeEnum.Location.Air:
                if (isAllowMove) {
                    CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
                } else {
                    CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
                }
                StartFreeFall ();
                break;
            case LifeEnum.Location.Ground:
            default:
                if (isAllowMove) {
                    StartWalking ();
                } else {
                    StartIdling ();
                }
                break;
        }
    }

    private void SetStatuses (CharEnum.Statuses statuses, bool isOn) {
        if (isOn) {
            CurrentStatuses = CurrentStatuses | statuses;
        } else {
            CurrentStatuses = CurrentStatuses & ~statuses;
        }
    }

    /// <summary>
    /// If <paramref name="statuses"/> is composite, it will return true only when currentStatus contains all those status
    /// </summary>
    /// <param name="statuses"></param>
    /// <returns></returns>
    public bool GetIsInStatuses (CharEnum.Statuses statuses) {
        return (CurrentStatuses & statuses) == statuses;
    }

    #region Body Parts

    public void ObtainBodyPart (CharEnum.BodyParts part) {
        if (!ObtainedBodyParts.HasFlag (part)) {
            ObtainedBodyParts = ObtainedBodyParts | part;

            ObtainedBodyPartsChanged?.Invoke (ObtainedBodyParts);
        }
    }

    #endregion

    #region Situation and Command

    #region CommandSettings

    public void SetCommandSettings (Dictionary<CharEnum.InputSituation, CharEnum.Command> settings) {
        commandSettings.Clear ();

        if (settings == null) {
            return;
        }

        foreach (var pair in settings) {
            commandSettings.Add (pair.Key, pair.Value);
        }
    }

    public CharEnum.Command? GetCommandBySituation (CharEnum.InputSituation situation) {
#if UNITY_EDITOR
        if (Params.IsUseDebugCommandSettings) {
            switch (situation) {
                case CharEnum.InputSituation.GroundTap:
                    return Params.GroundTapCommand;
                case CharEnum.InputSituation.GroundHold:
                    return Params.GroundHoldCommand;
                case CharEnum.InputSituation.GroundRelease:
                    return Params.GroundReleaseCommand;
                case CharEnum.InputSituation.AirTap:
                    return Params.AirTapCommand;
                case CharEnum.InputSituation.AirHold:
                    return Params.AirHoldCommand;
                case CharEnum.InputSituation.AirRelease:
                    return Params.AirReleaseCommand;
            }

            return null;
        }
#endif

        if (commandSettings.ContainsKey (situation)) {
            return commandSettings[situation];
        }

        return null;
    }

    #endregion

    private CharEnum.InputSituation? GetCurrentInputSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return CurrentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundTap : CharEnum.InputSituation.AirTap;
        } else if (isHolding) {
            return CurrentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundHold : CharEnum.InputSituation.AirHold;
        } else {
            return CurrentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundRelease : CharEnum.InputSituation.AirRelease;
        }
    }

    private void HandleCommand (CharEnum.InputSituation? optionalSituation, HandleCollisionResult handleCollisionResult) {
        if (optionalSituation == null || handleCollisionResult.IsIgnoreCommand) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        var situation = (CharEnum.InputSituation)optionalSituation;

        #region CommandInputSubEvent

        if (missionEventInputFinishedAction != null) {
            if (MissionEventManager.CurrentMissionSubEvent != null && MissionEventManager.CurrentMissionSubEvent is CommandInputSubEvent) {
                var targetSituation = ((CommandInputSubEvent)MissionEventManager.CurrentMissionSubEvent).InputSituation;

                if (targetSituation == situation) {
                    missionEventInputFinishedAction?.Invoke ();
                    missionEventInputFinishedAction = null;
                } else {
                    // Some cases would need to discard action
                    switch (targetSituation) {
                        case CharEnum.InputSituation.GroundTap:
                        case CharEnum.InputSituation.AirTap:
                        case CharEnum.InputSituation.GroundHold:
                        case CharEnum.InputSituation.AirHold:
                            if (situation != targetSituation) {
                                Log.Print ("Discard action due to Command Input Mission Event", LogTypes.Char | LogTypes.MissionEvent);
                                SetCurrentCommandStatus (null, null);
                                return;
                            }
                            break;
                        case CharEnum.InputSituation.GroundRelease:
                        case CharEnum.InputSituation.AirRelease:
                            if (situation == CharEnum.InputSituation.GroundTap || situation == CharEnum.InputSituation.AirTap) {
                                Log.Print ("Discard action due to Command Input Mission Event", LogTypes.Char | LogTypes.MissionEvent);
                                SetCurrentCommandStatus (null, null);
                                return;
                            }
                            break;
                    }
                }
            }
        }

        #endregion

        #region Cases to ignore

        if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
            if (isIgnoreHold) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        } else if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            if (isIgnoreRelease) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        }

        if (GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
            Log.Print ("Ignore InputSituation due to drop hitting. Situation = " + situation, LogTypes.Char);
            SetCurrentCommandStatus (null, null);

            if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
                isIgnoreHold = true;
                isIgnoreRelease = true;
            }
            return;
        }

        #endregion

        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Now / LastFrame : InputSituation : " + situation + " / " + currentInputSituation + " ; Command : " + command + " / " + currentCommand, LogTypes.Char);

        #region Finish the hold command

        if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
                switch (currentCommand) {
                    case CharEnum.Command.Dash:
                        if (handleCollisionResult.IsJustStoppedDashing) {
                            break;
                        }

                        var isNeedDashCoolDown = false;
                        var isNeedToChangeMovementAnim = false;

                        if (command == CharEnum.Command.Dash) {
                            // Hold Dash -> Release Dash
                            isNeedDashCoolDown = false;
                            isNeedToChangeMovementAnim = false;
                        } else if (command == CharEnum.Command.Jump) {
                            if (CheckIsAllowJump ()) {
                                // Hold Dash -> Release Jump
                                isNeedDashCoolDown = false;
                                isNeedToChangeMovementAnim = false;
                            } else {
                                // Hold Dash -> Release Jump (but cannot jump)
                                isNeedDashCoolDown = true;
                                isNeedToChangeMovementAnim = true;
                            }
                        } else {
                            // Hold Dash -> other release command / empty release command
                            isNeedDashCoolDown = true;
                            isNeedToChangeMovementAnim = true;
                        }

                        handleCollisionResult.IsJustStoppedDashing = StopDashing (CurrentHorizontalSpeed, isNeedDashCoolDown, isNeedToChangeMovementAnim);
                        break;
                }
            }
        }

        #endregion

        if (command == null) {
            SetCurrentCommandStatus (situation, null);
            return;
        }

        #region Details Command Cases

        var isTriggeredCommand = false;

        switch (command) {
            case CharEnum.Command.Jump:
                if (!CheckIsAllowJump ()) {
                    break;
                }

                isTriggeredCommand = true;
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        StartCoroutine (Jump ());
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        if (!GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
                            JumpCharge ();
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        if (currentCommand == CharEnum.Command.Jump) {
                            // Corresponding GroundHold / AirHold command is also Jump (that means this release command should be a charged jump)
                            if (GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
                                StartCoroutine (Jump ());
                            } else {
                                // If isJumpCharged = false, the JumpCharge is somehow cancelled. So do not do any action
                                isTriggeredCommand = false;
                            }
                        } else {
                            // That mean this release command is a non charged jump
                            StartCoroutine (Jump ());
                        }
                        break;
                }
                break;
            case CharEnum.Command.Dash:
                if (GetIsInStatuses (CharEnum.Statuses.DashCollingDown)) {
                    break;
                }

                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        if (currentInputSituation == situation) {
                            // Already dashing

                            if (!handleCollisionResult.IsJustStoppedDashing) {
                                isTriggeredCommand = true;
                            }
                        } else {
                            if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {     // Ensure not trigger hold dash while doing one tap dash
                                StartDashing (false);
                                isTriggeredCommand = true;
                            }
                        }

                        if (!isTriggeredCommand) {
                            isIgnoreHold = true;
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                }
                break;
            case CharEnum.Command.Hit:
                if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
                    break;
                }

                isTriggeredCommand = true;
                CharEnum.HitType? hitType = null;
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        hitType = CharEnum.HitType.Normal;
                        break;
                    case CharEnum.InputSituation.GroundHold:
                        hitType = CharEnum.HitType.Charged;
                        isIgnoreHold = true;
                        break;
                    case CharEnum.InputSituation.AirHold:
                        if (!GetIsInStatuses (CharEnum.Statuses.DropHitCharging)) {
                            DropHitCharge ();
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                        hitType = CharEnum.HitType.Finishing;
                        break;
                    case CharEnum.InputSituation.AirRelease:
                        if (currentCommand == CharEnum.Command.Hit) {
                            // AirHold command is also Hit
                            hitType = CharEnum.HitType.Drop;
                        } else {
                            hitType = CharEnum.HitType.Finishing;
                        }
                        break;
                }

                if (hitType != null) {
                    Hit ((CharEnum.HitType)hitType);
                }

                break;
            case CharEnum.Command.Arrow:
                if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
                    break;
                }

                isTriggeredCommand = true;
                CharEnum.ArrowType? arrowType = null;
                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        arrowType = CharEnum.ArrowType.Target;
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        arrowType = CharEnum.ArrowType.Straight;
                        isIgnoreHold = true;
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        arrowType = CharEnum.ArrowType.Triple;
                        break;
                }

                if (arrowType != null) {
                    ShootArrow ((CharEnum.ArrowType)arrowType);
                }

                break;
            case CharEnum.Command.Turn:
                if (handleCollisionResult.IsJustChangedDirection) {
                    break;
                }

                if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                    StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
                }

                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.GroundRelease:
                        ChangeFacingDirection (true);
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.InputSituation.AirTap:
                    case CharEnum.InputSituation.AirRelease:
                        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                            var wallDirection = FacingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                            RepelFromWall (wallDirection, true, true);
                        } else {
                            ChangeFacingDirection (false);
                        }
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        Log.PrintWarning ("No action of Turn command is defined for holding. Please check.", LogTypes.Char);
                        break;
                }

                break;
        }

        SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);

        #endregion

    }

    //private void HandleCommand (CharEnum.InputSituation? optionalSituation) {
    //    if (optionalSituation == null) {
    //        SetCurrentCommandStatus (null, null);
    //        return;
    //    }

    //    var situation = (CharEnum.InputSituation)optionalSituation;

    //    if (missionEventInputFinishedAction != null) {
    //        if (MissionEventManager.CurrentMissionSubEvent != null && MissionEventManager.CurrentMissionSubEvent is CommandInputSubEvent) {
    //            var targetSituation = ((CommandInputSubEvent)MissionEventManager.CurrentMissionSubEvent).InputSituation;

    //            if (targetSituation == situation) {
    //                missionEventInputFinishedAction?.Invoke ();
    //                missionEventInputFinishedAction = null;
    //            } else {
    //                // Some cases would need to discard action
    //                switch (targetSituation) {
    //                    case CharEnum.InputSituation.GroundTap:
    //                    case CharEnum.InputSituation.AirTap:
    //                    case CharEnum.InputSituation.GroundHold:
    //                    case CharEnum.InputSituation.AirHold:
    //                        if (situation != targetSituation) {
    //                            Log.Print ("Discard action due to Command Input Mission Event", LogTypes.Char | LogTypes.MissionEvent);
    //                            SetCurrentCommandStatus (null, null);
    //                            return;
    //                        }
    //                        break;
    //                    case CharEnum.InputSituation.GroundRelease:
    //                    case CharEnum.InputSituation.AirRelease:
    //                        if (situation == CharEnum.InputSituation.GroundTap || situation == CharEnum.InputSituation.AirTap) {
    //                            Log.Print ("Discard action due to Command Input Mission Event", LogTypes.Char | LogTypes.MissionEvent);
    //                            SetCurrentCommandStatus (null, null);
    //                            return;
    //                        }
    //                        break;
    //                }
    //            }
    //        }
    //    }

    //    if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
    //        if (isIgnoreHold) {
    //            SetCurrentCommandStatus (null, null);
    //            return;
    //        } else if (currentInputSituation != CharEnum.InputSituation.GroundHold && currentInputSituation != CharEnum.InputSituation.AirHold) {
    //            // (The situation of just triggered hold)

    //            if (isIgnoreUserInputInThisFrame) {
    //                // Ignore user input if just trigger hold
    //                // TODO : It may lead to some wired cases, e.g.:
    //                // trigger hold while touch wall -> hold action and release action failed, but
    //                // triggered hold and touch wall later -> hold action and release action success
    //                SetCurrentCommandStatus (null, null);
    //                return;
    //            }
    //        }
    //    } else {
    //        if (isIgnoreUserInputInThisFrame) {
    //            SetCurrentCommandStatus (null, null);
    //            return;
    //        }
    //    }

    //    if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
    //        if (isIgnoreRelease) {
    //            SetCurrentCommandStatus (null, null);
    //            return;
    //        }
    //    }

    //    if (GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
    //        Log.Print ("Ignore InputSituation due to drop hitting. Situation = " + situation, LogTypes.Char);
    //        SetCurrentCommandStatus (null, null);

    //        if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
    //            isIgnoreHold = true;
    //            isIgnoreRelease = true;
    //        }
    //        return;
    //    }

    //    var command = GetCommandBySituation (situation);
    //    Log.PrintDebug ("Handle/Current : InputSituation : " + situation + " / " + currentInputSituation + " ; Command : " + command + " / " + currentCommand, LogTypes.Char);

    //    // Fisish the hold command
    //    if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
    //        if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
    //            switch (currentCommand) {
    //                case CharEnum.Command.Dash:
    //                    if (command == CharEnum.Command.Dash) {
    //                        StopDashing (CurrentHorizontalSpeed, false, false);
    //                    } else if (command == CharEnum.Command.Jump) {
    //                        if (CheckIsAllowJump ()) {
    //                            StopDashing (CurrentHorizontalSpeed, false, false);
    //                        } else {
    //                            StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
    //                        }
    //                    } else {
    //                        StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    if (command == null) {
    //        SetCurrentCommandStatus (situation, null);
    //        return;
    //    }

    //    command = (CharEnum.Command)command;
    //    var isTriggeredCommand = false;

    //    switch (command) {
    //        case CharEnum.Command.Jump:
    //            if (!CheckIsAllowJump ()) {
    //                break;
    //            }

    //            isTriggeredCommand = true;
    //            switch (situation) {
    //                case CharEnum.InputSituation.GroundTap:
    //                case CharEnum.InputSituation.AirTap:
    //                    StartCoroutine (Jump ());
    //                    break;
    //                case CharEnum.InputSituation.GroundHold:
    //                case CharEnum.InputSituation.AirHold:
    //                    if (!GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
    //                        JumpCharge ();
    //                    }
    //                    break;
    //                case CharEnum.InputSituation.GroundRelease:
    //                case CharEnum.InputSituation.AirRelease:
    //                    var checkSituation = (situation == CharEnum.InputSituation.GroundRelease) ? CharEnum.InputSituation.GroundHold : CharEnum.InputSituation.AirHold;
    //                    if (GetCommandBySituation (checkSituation) == CharEnum.Command.Jump) {
    //                        // That mean this release command should be a charged jump
    //                        if (GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
    //                            StartCoroutine (Jump ());
    //                        } else {
    //                            // If isJumpCharged = false, the JumpCharge is somehow cancelled. So do not do any action
    //                            isTriggeredCommand = false;
    //                        }
    //                    } else {
    //                        // That mean this release command is a non charged jump
    //                        StartCoroutine (Jump ());
    //                    }
    //                    break;
    //            }
    //            break;
    //        case CharEnum.Command.Dash:
    //            if (GetIsInStatuses (CharEnum.Statuses.DashCollingDown)) {
    //                break;
    //            }

    //            switch (situation) {
    //                case CharEnum.InputSituation.GroundTap:
    //                case CharEnum.InputSituation.AirTap:
    //                    if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //                        StartDashing (true);
    //                        isTriggeredCommand = true;
    //                    }
    //                    break;
    //                case CharEnum.InputSituation.GroundHold:
    //                case CharEnum.InputSituation.AirHold:
    //                    if (currentInputSituation == situation) {    // Already dashing
    //                        if (!isJustTouchWall) {
    //                            isTriggeredCommand = true;
    //                        }
    //                    } else {
    //                        if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) { // Ensure not trigger hold dash while doing one tap dash
    //                            StartDashing (false);
    //                            isTriggeredCommand = true;
    //                        }
    //                    }
    //                    break;
    //                case CharEnum.InputSituation.GroundRelease:
    //                case CharEnum.InputSituation.AirRelease:
    //                    if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //                        StartDashing (true);
    //                        isTriggeredCommand = true;
    //                    }
    //                    break;
    //            }
    //            break;
    //        case CharEnum.Command.Hit:
    //            if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
    //                break;
    //            }

    //            CharEnum.HitType? hitType = null;
    //            switch (situation) {
    //                case CharEnum.InputSituation.GroundTap:
    //                case CharEnum.InputSituation.AirTap:
    //                    hitType = CharEnum.HitType.Normal;
    //                    break;
    //                case CharEnum.InputSituation.GroundHold:
    //                    hitType = CharEnum.HitType.Charged;
    //                    isIgnoreHold = true;
    //                    break;
    //                case CharEnum.InputSituation.AirHold:
    //                    if (!GetIsInStatuses (CharEnum.Statuses.DropHitCharging)) {
    //                        DropHitCharge ();
    //                    }
    //                    break;
    //                case CharEnum.InputSituation.GroundRelease:
    //                    hitType = CharEnum.HitType.Finishing;
    //                    break;
    //                case CharEnum.InputSituation.AirRelease:
    //                    if (currentCommand == CharEnum.Command.Hit) {  // That means, AirHold command is also Hit
    //                        hitType = CharEnum.HitType.Drop;
    //                    } else {
    //                        hitType = CharEnum.HitType.Finishing;
    //                    }
    //                    break;
    //            }

    //            if (hitType != null) {
    //                Hit ((CharEnum.HitType)hitType);
    //            }
    //            isTriggeredCommand = true;

    //            break;
    //        case CharEnum.Command.Arrow:
    //            if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
    //                break;
    //            }

    //            CharEnum.ArrowType? arrowType = null;
    //            switch (situation) {
    //                case CharEnum.InputSituation.GroundTap:
    //                case CharEnum.InputSituation.AirTap:
    //                    arrowType = CharEnum.ArrowType.Target;
    //                    break;
    //                case CharEnum.InputSituation.GroundHold:
    //                case CharEnum.InputSituation.AirHold:
    //                    arrowType = CharEnum.ArrowType.Straight;
    //                    isIgnoreHold = true;
    //                    break;
    //                case CharEnum.InputSituation.GroundRelease:
    //                case CharEnum.InputSituation.AirRelease:
    //                    arrowType = CharEnum.ArrowType.Triple;
    //                    break;
    //            }

    //            if (arrowType != null) {
    //                ShootArrow ((CharEnum.ArrowType)arrowType);
    //            }
    //            isTriggeredCommand = true;

    //            break;
    //        case CharEnum.Command.Turn:
    //            // Do not do turn action if touching wall and on ground because changing moving direction is trigged while touch wall
    //            if (isTouchingWall && CurrentLocation == LifeEnum.Location.Ground) {
    //                break;
    //            }

    //            if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //                StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
    //            }

    //            switch (situation) {
    //                case CharEnum.InputSituation.GroundTap:
    //                case CharEnum.InputSituation.GroundRelease:
    //                    ChangeFacingDirection (true);
    //                    isTriggeredCommand = true;
    //                    break;
    //                case CharEnum.InputSituation.AirTap:
    //                case CharEnum.InputSituation.AirRelease:
    //                    if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
    //                        var wallDirection = FacingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
    //                        RepelFromWall (wallDirection, true);
    //                    } else {
    //                        ChangeFacingDirection (false);
    //                    }
    //                    isTriggeredCommand = true;
    //                    break;
    //                case CharEnum.InputSituation.GroundHold:
    //                case CharEnum.InputSituation.AirHold:
    //                    Log.PrintWarning ("No action of Turn command is defined for holding. Please check.", LogTypes.Char);
    //                    break;
    //            }

    //            break;
    //    }

    //    if (!isTriggeredCommand) {
    //        if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
    //            isIgnoreHold = true;
    //        }
    //    }
    //    SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);
    //}

    private void SetCurrentCommandStatus (CharEnum.InputSituation? situation, CharEnum.Command? command) {
        currentInputSituation = situation;
        currentCommand = command;

        //if (situation != null && command != null) {
        //    Log.PrintDebug ("SetCurrentCommandStatus  : InputSituation : " + currentInputSituation + "   Command : " + currentCommand, LogTypes.Char);
        //}
    }

    /// <param name="isOnlyStopHoldingDash"><b>true</b> means allow one shot dash to keep on</param>
    /// <returns>isJustStoppedDashing</returns>
    private bool BreakInProgressAction (bool isOnlyStopHoldingDash, bool isChangeMovementAnimIfStoppedDashing) {

        StopJumpCharge ();
        StopDropHitCharge ();
        StopDropHit ();

        var isJustStoppedDashing = false;

        if (!isOnlyStopHoldingDash) {
            isJustStoppedDashing = StopDashing (CharEnum.HorizontalSpeed.Walk, true, isChangeMovementAnimIfStoppedDashing);
        }

        if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
            if (isOnlyStopHoldingDash) {
                if (currentCommand == CharEnum.Command.Dash) {
                    isJustStoppedDashing = StopDashing (CharEnum.HorizontalSpeed.Walk, true, isChangeMovementAnimIfStoppedDashing);
                }
            }

            isIgnoreHold = true;
            isIgnoreRelease = true;
        }

        return isJustStoppedDashing;
    }

    #endregion

    #region Animtor

    private void SetAnimatorTrigger (string trigger) {
        Log.PrintDebug ("Char SetAnimatorTrigger : " + trigger, LogTypes.Char | LogTypes.Animation);
        animator.SetTrigger (trigger);
    }

    private void ResetAnimatorTrigger (string trigger) {
        Log.PrintDebug ("Char ResetAnimatorTrigger : " + trigger, LogTypes.Char | LogTypes.Animation);
        animator.ResetTrigger (trigger);
    }

    private void ResetUntriggeredAnimatorTriggers () {
        animator.ResetTrigger (CharAnimConstant.LandingTriggerName);
    }

    protected void SetAnimatorBool (string boolName, bool value) {
        Log.PrintDebug ("Char SetAnimatorBool : " + boolName + " ; Value : " + value, LogTypes.Char | LogTypes.Animation);
        animator.SetBool (boolName, value);
    }

    #endregion

    #region HP recovery

    private void SetHPRecovery (bool isActive) {
        if (isActive) {
            if (hpRecoveryCoroutine == null) {
                hpRecoveryCoroutine = StartCoroutine (HPRecoveryCoroutine ());
            }
        } else {
            if (hpRecoveryCoroutine != null) {
                StopCoroutine (hpRecoveryCoroutine);
                hpRecoveryCoroutine = null;
            }
        }
    }

    private IEnumerator HPRecoveryCoroutine () {
        while (CurrentHP != TotalHP) {
            yield return new WaitForSeconds (Params.HPRecoveryPeriod);

            CurrentHP++;
        }

        SetHPRecovery (false);
    }

    #endregion

    #region Idle / Walk
    public void LandingFinishedAction () {
        isWaitingLandingToStopChar = false;
        StartIdleOrWalk ();
    }

    private void StartIdleOrWalk () {
        switch (CurrentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                StartIdling ();
                break;
            case CharEnum.HorizontalSpeed.Walk:
                StartWalking ();
                break;
            default:
                Log.PrintWarning ("Current horizontal speed is : " + CurrentHorizontalSpeed + " . It does not match idle or walk. Force to change to walk. Please check.", LogTypes.Char);
                StartWalking ();
                break;
        }
    }

    private void StartIdling () {
        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        SetAnimatorTrigger (CharAnimConstant.IdleTriggerName);
    }

    private void StartWalking () {
        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;

        SetAnimatorTrigger (CharAnimConstant.WalkTriggerName);
    }

    #endregion

    #region Dash

    private void StartDashing (bool isOneShot) {
        Log.Print ("Dash : isOneShot = " + isOneShot, LogTypes.Char);

        StopDashing (CurrentHorizontalSpeed, false, false);  // To ensure do not trigger 2 dash coroutines at the same time

        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            SetStatuses (CharEnum.Statuses.Sliding, false);
        }

        if (isOneShot) {
            dashCoroutine = StartCoroutine (OneShotDashCoroutine ());
        } else {
            SetDashing ();
        }
    }

    /// <returns>isReallyStoppedDashing</returns>
    private bool StopDashing (CharEnum.HorizontalSpeed speedAfterStopDashing, bool isNeedDashCoolDown, bool isNeedToChangeMovementAnim) {
        // Remarks:
        // There is a case that calling StopDashing() while it is already in dash cool down stage.
        // Below is to ensure no dash cool down if isNeedDashCoolDown = false
        if (!isNeedDashCoolDown) {
            if (dashCoolDownCoroutine != null) {
                StopCoroutine (dashCoolDownCoroutine);
                SetStatuses (CharEnum.Statuses.DashCollingDown, false);
                dashCoolDownCoroutine = null;
            }
        }

        if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
            return false;
        }

        if (dashCoroutine != null) {
            StopCoroutine (dashCoroutine);
            dashCoroutine = null;
        }

        SetStatuses (CharEnum.Statuses.Dashing, false);
        CurrentHorizontalSpeed = speedAfterStopDashing;
        if (isNeedToChangeMovementAnim) {
            if (CurrentLocation == LifeEnum.Location.Ground) {
                StartIdleOrWalk ();
            } else {
                StartFreeFall ();
            }
        }

        if (isNeedDashCoolDown) {
            if (dashCoolDownCoroutine != null) {
                StopCoroutine (dashCoolDownCoroutine);
            }
            dashCoolDownCoroutine = StartCoroutine (DashCoolDownCoroutine ());
        }

        return true;
    }

    private void SetDashing () {
        SetStatuses (CharEnum.Statuses.Dashing, true);
        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Dash;
        AlignMovingWithFacingDirection ();

        SetAnimatorTrigger (CharAnimConstant.DashTriggerName);
    }

    private IEnumerator OneShotDashCoroutine () {
        SetDashing ();

        var startTime = Time.time;

        while (Time.time - startTime < Params.OneShotDashPeriod) {
            //if (isJustTouchWall) {
            //    break;
            //}

            yield return null;
        }

        // If touching wall, sliding animation dominate the animation
        //StopDashing (CharEnum.HorizontalSpeed.Walk, true, isJustTouchWall ? false : true);

        StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
    }

    private IEnumerator DashCoolDownCoroutine () {
        SetStatuses (CharEnum.Statuses.DashCollingDown, true);

        yield return new WaitForSeconds (Params.DashCoolDownPeriod);

        SetStatuses (CharEnum.Statuses.DashCollingDown, false);
        dashCoolDownCoroutine = null;
    }

    #endregion

    #region Jump

    public float GetCurrentJumpInitSpeed () {
        return GetIsInStatuses (CharEnum.Statuses.JumpCharging) ? Params.ChargeJumpInitSpeed : Params.NormalJumpInitSpeed;
    }

    private bool CheckIsAllowJump () {
        if (CurrentLocation == LifeEnum.Location.Ground) {
            return true;
        }

        return isAllowAirJump;
    }

    private IEnumerator Jump () {
        Log.Print ("Jump : isCharged = " + GetIsInStatuses (CharEnum.Statuses.JumpCharging), LogTypes.Char);

        StopDashing (CurrentHorizontalSpeed, false, false);

        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            SetStatuses (CharEnum.Statuses.Sliding, false);
        }

        if (CurrentHorizontalSpeed == CharEnum.HorizontalSpeed.Idle) {
            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
        }
        AlignMovingWithFacingDirection ();

        if (CurrentLocation == LifeEnum.Location.Ground) {
            isJustJumpedUp = true;
        } else {
            isAllowAirJump = false;
        }

        SetAnimatorTrigger (CharAnimConstant.JumpTriggerName);

        // Remarks : WaitForEndOfFrame in order to let CharJumpSMB know the jump is charged
        yield return new WaitForEndOfFrame ();
        StopJumpCharge ();
    }

    private void JumpCharge () {
        if (GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
            return;
        }

        Log.PrintDebug ("JumpCharge", LogTypes.Char);
        SetStatuses (CharEnum.Statuses.JumpCharging, true);
    }

    private void StopJumpCharge () {
        if (!GetIsInStatuses (CharEnum.Statuses.JumpCharging)) {
            return;
        }

        Log.PrintDebug ("StopJumpCharge", LogTypes.Char);
        SetStatuses (CharEnum.Statuses.JumpCharging, false);
    }

    #endregion

    #region Hit

    private void Hit (CharEnum.HitType hitType) {
        if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger Hit action. Please check.", LogTypes.Char);
            return;
        }

        if (GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger Hit action. Please check.", LogTypes.Char);
            return;
        }

        Log.Print ("Hit : HitType = " + hitType, LogTypes.Char);
        CurrentHitType = hitType;

        switch (hitType) {
            case CharEnum.HitType.Normal:
            case CharEnum.HitType.Charged:
            case CharEnum.HitType.Finishing:
                SetAnimatorTrigger (CharAnimConstant.HitTriggerName);
                attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (hitType));
                break;
            case CharEnum.HitType.Drop:
                DropHit ();
                break;
        }
    }

    private void DropHit () {
        Log.Print ("Start DropHit", LogTypes.Char);

        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
            var wallDirection = FacingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
            RepelFromWall (wallDirection, true, false);
        }

        StopDropHitCharge ();
        SetStatuses (CharEnum.Statuses.DropHitting, true);

        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        SetAnimatorTrigger (CharAnimConstant.DropHitTriggerName);
    }

    private void StopDropHit () {
        if (!GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
            return;
        }

        Log.Print ("Stop DropHit", LogTypes.Char);

        SetStatuses (CharEnum.Statuses.DropHitting, false);

        attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (CharEnum.HitType.Drop));
    }

    private void DropHitCharge () {
        if (GetIsInStatuses (CharEnum.Statuses.DropHitCharging)) {
            return;
        }

        SetStatuses (CharEnum.Statuses.DropHitCharging, true);
    }

    private void StopDropHitCharge () {
        if (!GetIsInStatuses (CharEnum.Statuses.DropHitCharging)) {
            return;
        }

        SetStatuses (CharEnum.Statuses.DropHitCharging, false);
    }

    private IEnumerator HitCoolDownCoroutine (CharEnum.HitType hitType) {
        SetStatuses (CharEnum.Statuses.AttackCoolingDown, true);

        var hitCoolDownPeriod = 0f;

        switch (hitType) {
            case CharEnum.HitType.Normal:
                hitCoolDownPeriod = Params.HitCoolDownPeriod_Normal;
                break;
            case CharEnum.HitType.Charged:
                hitCoolDownPeriod = Params.HitCoolDownPeriod_Charged;
                break;
            case CharEnum.HitType.Finishing:
                hitCoolDownPeriod = Params.HitCoolDownPeriod_Finishing;
                break;
            case CharEnum.HitType.Drop:
                hitCoolDownPeriod = Params.HitCoolDownPeriod_Drop;
                break;
            default:
                Log.PrintWarning ("Not yet set hit cool down period for HitType : " + hitType + " . Assume cool down period to be 0s", LogTypes.Char);
                break;
        }

        yield return new WaitForSeconds (hitCoolDownPeriod);

        CurrentHitType = null;
        SetStatuses (CharEnum.Statuses.AttackCoolingDown, false);
        attackCoolDownCoroutine = null;
    }

    #endregion

    #region Arrow

    private void ShootArrow (CharEnum.ArrowType arrowType) {
        if (GetIsInStatuses (CharEnum.Statuses.AttackCoolingDown)) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger shoot arrow action. Please check.", LogTypes.Char);
            return;
        }

        if (GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger shoot arrow action. Please check.", LogTypes.Char);
            return;
        }

        Log.Print ("Shoot arrow : ArrowType = " + arrowType, LogTypes.Char);
        CurrentArrowType = arrowType;

        switch (arrowType) {
            case CharEnum.ArrowType.Target:
            case CharEnum.ArrowType.Straight:
            case CharEnum.ArrowType.Triple:
                SetAnimatorTrigger (CharAnimConstant.ShootTriggerName);
                attackCoolDownCoroutine = StartCoroutine (ArrowCoolDownCoroutine (arrowType));
                break;
        }
    }

    private IEnumerator ArrowCoolDownCoroutine (CharEnum.ArrowType arrowType) {
        SetStatuses (CharEnum.Statuses.AttackCoolingDown, true);

        var arrowCoolDownPeriod = 0f;

        switch (arrowType) {
            case CharEnum.ArrowType.Target:
                arrowCoolDownPeriod = Params.ArrowCoolDownPeriod_Target;
                break;
            case CharEnum.ArrowType.Straight:
                arrowCoolDownPeriod = Params.ArrowCoolDownPeriod_Straight;
                break;
            case CharEnum.ArrowType.Triple:
                arrowCoolDownPeriod = Params.ArrowCoolDownPeriod_Triple;
                break;
            default:
                Log.PrintWarning ("Not yet set arrow cool down period for ArrowType : " + arrowType + " . Assume cool down period to be 0s", LogTypes.Char);
                break;
        }

        yield return new WaitForSeconds (arrowCoolDownPeriod);

        CurrentArrowType = null;
        SetStatuses (CharEnum.Statuses.AttackCoolingDown, false);
        attackCoolDownCoroutine = null;
    }

    public Vector2? SearchShootTargetPos () {
        if (mapManager == null) {
            Log.Print ("No ShootTarget found : mapManager == null", LogTypes.Char | LogTypes.MapData);
            return null;
        }

        var allTargets = mapManager.ArrowTargetList;
        if (allTargets == null || allTargets.Count <= 0) {
            Log.Print ("No ShootTarget found : no targets in map", LogTypes.Char | LogTypes.MapData);
            return null;
        }

        Vector2? result = null;
        var resultDistanceSquare = -1f;
        var from = GetTargetPos ();

        foreach (var target in allTargets) {
            var targetPos = target.GetTargetPos ();

            // Check direction
            var directionRefValue = FacingDirection == LifeEnum.HorizontalDirection.Right ? 1 : -1;
            var deltaX = targetPos.x - from.x;
            if (Mathf.Sign (deltaX) != directionRefValue) {
                continue;
            }

            // Check gradient
            var deltaY = targetPos.y - from.y;
            if (Mathf.Abs (deltaX * Params.TargetArrowMaxInitShootGradient) < Mathf.Abs (deltaY)) {
                continue;
            }

            // Check max distance
            var distanceSquare = deltaX * deltaX + deltaY * deltaY;
            if (distanceSquare > Params.TargetArrowMaxTargetDistanceSquare) {
                continue;
            }

            // Check smallest distance
            if (resultDistanceSquare < 0 || resultDistanceSquare > distanceSquare) {
                result = targetPos;
                resultDistanceSquare = distanceSquare;
            }
        }

        if (result != null) {
            Log.Print ("ShootTarget found : " + result, LogTypes.Char | LogTypes.MapData);
        } else {
            Log.Print ("No ShootTarget found : no suitable target", LogTypes.Char | LogTypes.MapData);
        }

        return result;
    }

    #endregion

    #region Change Direction

    private void ChangeFacingDirection (bool isAlignMovingDirection) {
        Log.PrintDebug ("ChangeFacingDirection : isAlignMovingDirection = " + isAlignMovingDirection, LogTypes.Char);
        if (FacingDirection == LifeEnum.HorizontalDirection.Left) {
            FacingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            FacingDirection = LifeEnum.HorizontalDirection.Left;
        }

        if (isAlignMovingDirection) {
            MovingDirection = FacingDirection;
        }
    }

    private void ChangeMovingDirection () {
        // Remarks : Changing moving direction must also align facing direction

        Log.PrintDebug ("ChangeMovingDirection", LogTypes.Char);
        if (MovingDirection == LifeEnum.HorizontalDirection.Left) {
            MovingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            MovingDirection = LifeEnum.HorizontalDirection.Left;
        }

        FacingDirection = MovingDirection;
    }

    private void SetMovingDirection (LifeEnum.HorizontalDirection direction) {
        // Remarks : Changing moving direction must also align facing direction
        MovingDirection = direction;
        FacingDirection = MovingDirection;
    }

    private void AlignMovingWithFacingDirection () {
        MovingDirection = FacingDirection;
    }

    #endregion

    #region HP

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        var isAlive = base.Hurt (dp, hurtDirection);

        Log.Print ("Char : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + CurrentHP, LogTypes.Char);

        if (isAlive) {
            SetHPRecovery (true);
        }

        return isAlive;
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print ("Char : Die!", LogTypes.Char);

        SetHPRecovery (false);

        SetAnimatorTrigger (CharAnimConstant.DieTriggerName);
        StartCoroutine (WaitAndFinishDying ());
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        base.StartBeatingBack (hurtDirection);

        BreakInProgressAction (false, false);
        SetAllowUserControl (false, true);

        // If dying, dominated by die animation
        if (!IsDying) {
            if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                var wallDirection = FacingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                RepelFromWall (wallDirection, true, true);
            } else {
                BeatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? BeatBackDirection_Left : BeatBackDirection_Right;
                MovingDirection = hurtDirection;

                SetAnimatorTrigger (CharAnimConstant.BeatBackTriggerName);
            }
        }


        StartCoroutine (WaitAndFinishBeatingBack ());
    }

    protected override void StopBeatingBack () {
        base.StopBeatingBack ();

        SetAllowUserControl (true, true);
    }

    protected override void StartInvincible () {
        base.StartInvincible ();

        // If dying, dominated by die animation
        if (!IsDying) {
            SetAnimatorBool (CharAnimConstant.InvincibleBoolName, true);
        }
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        SetAnimatorBool (CharAnimConstant.InvincibleBoolName, false);
    }

    private IEnumerator WaitAndFinishBeatingBack () {
        yield return new WaitForSeconds (Params.BeatBackPeriod);

        StopBeatingBack ();
    }

    private IEnumerator WaitAndFinishDying () {
        yield return new WaitForSeconds (Params.DyingPeriod);

        Died?.Invoke ();
    }
    #endregion

    #region Collision Event

    protected override void HandleCollision (CollisionAnalysis collisionAnalysis) {
        var collisionDetailsDict = collisionAnalysis.CollisionDetailsDict;

        var isNowTouchingGround = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Ground);
        var isNowTouchingWall = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Wall);
        var isNowTouchingWallSlippy = false;

        // Only get the first collision details among Wall and SlippyWall collision
        var wallPosition = LifeEnum.HorizontalDirection.Left;
        if (isNowTouchingWall) {
            var wallCollisionDetails = collisionDetailsDict[LifeEnum.CollisionType.Wall][0];
            wallPosition = wallCollisionDetails.CollisionNormal.x < 0 ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
            if ((bool)wallCollisionDetails.AdditionalDetails == true) {
                isNowTouchingWallSlippy = true;
            }
        }

        currentHandleCollisionResult = new HandleCollisionResult ();

        if (IsDying) {
            CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
            currentHandleCollisionResult.IsIgnoreCommand = true;
            return;
        }

        // Check for death
        if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Death)) {
            Die (MovingDirection);
            CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
            currentHandleCollisionResult.IsIgnoreCommand = true;
            return;
        }

        // Check for hurt
        if (!IsInvincible) {    // add IsInvincible checking to prevent some cases that somehow although has set invincible, the enemy collision has still not yet exited
            if (collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Enemy)) {
                // Only get the first collision details
                var details = collisionDetailsDict[LifeEnum.CollisionType.Enemy][0];
                if (details.AdditionalDetails is EnemyModelBase) {
                    var enemy = (EnemyModelBase)details.AdditionalDetails;
                    Log.Print ("CollideToEnemy : " + enemy.gameObject.name + " , collisionNormal : " + details.CollisionNormal, LogTypes.Char | LogTypes.Collision);

                    var hurtDirection = details.CollisionNormal.x < 0 ? LifeEnum.HorizontalDirection.Left : LifeEnum.HorizontalDirection.Right;
                    if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                        if (wallPosition == hurtDirection) {
                            // Change the hurt direction to away from the wall to prevent error due to being hurt + touch wall at the same frame
                            hurtDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                        }
                    }

                    Hurt (enemy.CollisionDP, hurtDirection);
                    CurrentLocation = isNowTouchingGround ? LifeEnum.Location.Ground : LifeEnum.Location.Air;
                    currentHandleCollisionResult.IsIgnoreCommand = true;
                    return;
                } else {
                    Log.PrintError ("The AdditionalDetails for enemy collision type has wrong object type : " + details.AdditionalDetails.GetType () + ". Please check.", LogTypes.Char | LogTypes.Collision);
                }
            }
        }

        // Remarks : Trigger animation at last to prevent triggering multiple animation
        var handleCollisionAnimation = HandleCollisionAnimation.None;

        Action CheckWithWallAndChangeMovingDirection = () => {
            if (wallPosition == MovingDirection) {
                // Change moving direction only when char originally move towards wall
                ChangeMovingDirection ();
                currentHandleCollisionResult.IsJustChangedDirection = true;
            }
        };

        //Log.PrintDebug (CurrentLocation + "  " + isNowTouchingGround + "  " + isNowTouchingWall + "  " + isNowTouchingSlippyWall);

        switch (CurrentLocation) {
            case LifeEnum.Location.Unknown:
                if (isNowTouchingGround) {
                    CurrentLocation = LifeEnum.Location.Ground;
                }
                break;
            case LifeEnum.Location.Ground:
                if (collisionAnalysis.GroundCollisionChangedType == LifeEnum.CollisionChangedType.Exit) {
                    // Ground -> Air
                    Log.PrintDebug ("Char : Leave ground", LogTypes.Char | LogTypes.Collision);
                    CurrentLocation = LifeEnum.Location.Air;

                    if (IsBeatingBack) {
                        // If it is beating back, behaviour is dominated by beat back handling
                        break;
                    }

                    var isOnlyStopHoldingDash = collisionAnalysis.WallCollisionChangedType != LifeEnum.CollisionChangedType.Enter;
                    currentHandleCollisionResult.IsJustStoppedDashing = BreakInProgressAction (isOnlyStopHoldingDash, false);

                    if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                        if (isNowTouchingWallSlippy) {
                            // Ground -> Air + Slippy Wall
                            Log.PrintWarning ("Char : Ground -> Air + Slippy Wall . This case should be very rare. Please check if everything behave alright.", LogTypes.Char | LogTypes.Collision);
                            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
                            RepelFromWall (FacingDirection, true, false);
                            handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                        } else {
                            // Ground -> Air + Normal Wall
                            Log.PrintWarning ("Char : Ground -> Air + Normal Wall . This case should be very rare. Please check if everything behave alright.", LogTypes.Char | LogTypes.Collision);
                            CheckWithWallAndChangeMovingDirection ();
                            handleCollisionAnimation = HandleCollisionAnimation.Sliding;
                        }
                    } else {
                        if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                            // If it is still dashing, it is one shot dash. Dominated by dash handling
                            break;
                        }

                        if (isJustJumpedUp) {
                            // Dominated by jump handling
                            isJustJumpedUp = false;
                            handleCollisionAnimation = HandleCollisionAnimation.None;
                        } else {
                            handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                        }
                    }
                } else {
                    if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                        // touch wall when on ground
                        Log.PrintDebug ("Char : Touch wall when on ground", LogTypes.Char | LogTypes.Collision);
                        CheckWithWallAndChangeMovingDirection ();
                        currentHandleCollisionResult.IsJustStoppedDashing = StopDashing (CharEnum.HorizontalSpeed.Walk, true, false);
                        handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                    }
                }
                break;
            case LifeEnum.Location.Air:
                if (collisionAnalysis.GroundCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                    // Air -> Ground
                    Log.PrintDebug ("Char : Touch ground", LogTypes.Char | LogTypes.Collision);
                    CurrentLocation = LifeEnum.Location.Ground;
                    isAllowAirJump = true;

                    var isOnlyStopHoldingDash = collisionAnalysis.WallCollisionChangedType != LifeEnum.CollisionChangedType.Enter;
                    currentHandleCollisionResult.IsJustStoppedDashing = BreakInProgressAction (isOnlyStopHoldingDash, false);

                    if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                        // Air -> Ground + Normal Wall / Slippy Wall
                        Log.PrintWarning ("Char : Air -> Ground + Normal Wall / Slippy Wall . This case should be very rare. Please check if everything behave alright.", LogTypes.Char | LogTypes.Collision);
                        CheckWithWallAndChangeMovingDirection ();
                    }

                    if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
                        // If it is still dashing, it is one shot dash (and is not touching wall). Dominated by dash handling
                        break;
                    }

                    AlignMovingWithFacingDirection ();

                    if (isAllowMove) {
                        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
                    } else {
                        // Mainly for some special cases that need to stop the char
                        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
                    }

                    if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                        SetStatuses (CharEnum.Statuses.Sliding, false);
                        handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                    } else {
                        handleCollisionAnimation = HandleCollisionAnimation.Landing;
                    }
                } else {
                    // keep in air
                    if (GetIsInStatuses (CharEnum.Statuses.DropHitting)) {
                        // Dominated by DropHitting handling
                        break;
                    }

                    if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Enter) {
                        currentHandleCollisionResult.IsJustStoppedDashing = StopDashing (CharEnum.HorizontalSpeed.Walk, true, false);

                        if (isNowTouchingWallSlippy) {
                            // Touch slippy wall when in air
                            Log.PrintDebug ("Char : Touch slippy wall when in air", LogTypes.Char | LogTypes.Collision);
                            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
                            RepelFromWall (FacingDirection, true, false);
                            handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                        } else {
                            // Touch normal wall when in air
                            Log.PrintDebug ("Char : Touch normal wall when in air", LogTypes.Char | LogTypes.Collision);
                            CheckWithWallAndChangeMovingDirection ();
                            handleCollisionAnimation = HandleCollisionAnimation.Sliding;
                        }
                    } else if (collisionAnalysis.WallCollisionChangedType == LifeEnum.CollisionChangedType.Exit) {
                        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
                            // Sliding to the end of the wall
                            Log.PrintDebug ("Char : Sliding to the end of the wall", LogTypes.Char | LogTypes.Collision);
                            SetStatuses (CharEnum.Statuses.Sliding, false);
                            ChangeMovingDirection ();
                            currentHandleCollisionResult.IsJustChangedDirection = true;
                            handleCollisionAnimation = HandleCollisionAnimation.IdleOrWalkOrFreeFall;
                        }
                    }
                }
                break;
        }

        switch (handleCollisionAnimation) {
            case HandleCollisionAnimation.Sliding:
                StartSliding ();
                break;
            case HandleCollisionAnimation.IdleOrWalkOrFreeFall:
                if (CurrentLocation == LifeEnum.Location.Ground) {
                    StartIdleOrWalk ();
                } else {
                    StartFreeFall ();
                }
                break;
            case HandleCollisionAnimation.Landing:
                SetAnimatorTrigger (CharAnimConstant.LandingTriggerName);
                break;
        }
    }

    protected override void AddCollisionEventHandlers () {
        //CollisionScript.TouchedGround += TouchedGroundHandler;
        //CollisionScript.LeftGround += LeftGroundHandler;
        //CollisionScript.TouchedWall += TouchedWallHandler;
        //CollisionScript.LeftWall += LeftWallHandler;
        //CollisionScript.TouchedDeathTag += TouchedDeathTagHandler;
        //CollisionScript.TouchedEnemy += TouchedEnemyHandler;
    }

    protected override void RemoveCollisionEventHandlers () {
        //CollisionScript.TouchedGround -= TouchedGroundHandler;
        //CollisionScript.LeftGround -= LeftGroundHandler;
        //CollisionScript.TouchedWall -= TouchedWallHandler;
        //CollisionScript.LeftWall -= LeftWallHandler;
        //CollisionScript.TouchedDeathTag -= TouchedDeathTagHandler;
        //CollisionScript.TouchedEnemy -= TouchedEnemyHandler;
    }

    //private void TouchedGroundHandler () {
    //    Log.PrintDebug ("Char : TouchGround", LogTypes.Char);

    //    isIgnoreUserInputInThisFrame = true;

    //    // Touch ground while init / set position
    //    if (CurrentLocation == LifeEnum.Location.Unknown) {
    //        CurrentLocation = LifeEnum.Location.Ground;
    //        return;
    //    }

    //    if (!GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //        if (isAllowMove) {
    //            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
    //        } else {
    //            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
    //        }
            
    //        AlignMovingWithFacingDirection ();

    //        if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {
    //            SetStatuses (CharEnum.Statuses.Sliding, false);
    //            StartWalking ();
    //        } else if (CurrentLocation == LifeEnum.Location.Air) {
    //            SetAnimatorTrigger (CharAnimConstant.LandingTriggerName);
    //        }
    //    }

    //    isAllowAirJump = true;
    //    CurrentLocation = LifeEnum.Location.Ground;

    //    BreakInProgressAction (true, true);
    //}

    //private void LeftGroundHandler () {
    //    Log.PrintDebug ("Char : LeaveGround", LogTypes.Char);

    //    isIgnoreUserInputInThisFrame = true;

    //    CurrentLocation = LifeEnum.Location.Air;

    //    if (IsBeatingBack) {
    //        // Dominate by beat back handling
    //        return;
    //    }

    //    BreakInProgressAction (true, false);

    //    if (isJustJumpedUp) {
    //        isJustJumpedUp = false;
    //    } else if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //        // Keep dashing
    //    } else {
    //        StartFreeFall ();
    //    }
    //}

    //private void TouchedWallHandler (LifeEnum.HorizontalDirection wallPosition, bool isSlippyWall) {
    //    Log.PrintDebug ("Char : TouchWall : isSlippyWall = " + isSlippyWall, LogTypes.Char);

    //    isIgnoreUserInputInThisFrame = true;
    //    isJustTouchWall = true;
    //    isTouchingWall = true;

    //    if (wallPosition == MovingDirection) {  // Change moving direction only when char originally move towards wall
    //        // if it is slippy wall, do not change moving direction when in air
    //        if (!isSlippyWall || CurrentLocation == LifeEnum.Location.Ground) {
    //            ChangeMovingDirection ();
    //        }
    //    }

    //    if (GetIsInStatuses (CharEnum.Statuses.Dashing)) {
    //        StopDashing (CharEnum.HorizontalSpeed.Walk, true, CurrentLocation == LifeEnum.Location.Ground);

    //        // Break hold input if "GroundHold - Dash" or "AirHold - Dash"
    //        if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
    //            if (currentCommand == CharEnum.Command.Dash) {
    //                isIgnoreHold = true;
    //            }
    //        }
    //    }

    //    if (CurrentLocation == LifeEnum.Location.Air) {
    //        if (isSlippyWall) {
    //            CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
    //            RepelFromWall (FacingDirection, true);
    //        } else {
    //            StartSliding ();
    //        }
    //    }
    //}

    //private void LeftWallHandler (bool isSlippyWall) {
    //    Log.PrintDebug ("Char : LeaveWall : isSlippyWall = " + isSlippyWall, LogTypes.Char);

    //    isTouchingWall = false;
    //    isIgnoreUserInputInThisFrame = true;

    //    if (GetIsInStatuses (CharEnum.Statuses.Sliding)) {  // The case that char is sliding to the end of the wall and then free fall
    //        SetStatuses (CharEnum.Statuses.Sliding, false);
    //        ChangeMovingDirection ();
    //        StartFreeFall ();
    //    }
    //}

    private void StartSliding () {
        SetStatuses (CharEnum.Statuses.Sliding, true);
        CurrentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
        isAllowAirJump = true;   // Allow jump in air again

        SetAnimatorTrigger (CharAnimConstant.SlideTriggerName);
    }

    private void RepelFromWall (LifeEnum.HorizontalDirection wallDirection, bool isFinallyFacingWall, bool isTriggerFreeFallAnim) {
        SetStatuses (CharEnum.Statuses.Sliding, false);

        var directionMultiplier = wallDirection == LifeEnum.HorizontalDirection.Left ? 1 : -1;
        SetPosByOffset (new Vector2 (Params.RepelFromWallDistByTurn, 0) * directionMultiplier);

        if (isFinallyFacingWall) {
            SetMovingDirection (wallDirection);
        } else {
            SetMovingDirection (wallDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left);
        }

        if (isTriggerFreeFallAnim) {
            StartFreeFall ();
        }
    }

    private void StartFreeFall () {
        SetAnimatorTrigger (CharAnimConstant.FreeFallTriggerName);
    }

    //private void TouchedDeathTagHandler () {
    //    Die (MovingDirection);
    //}

    //private void TouchedEnemyHandler (EnemyModelBase enemy, Vector2 collisionNormal) {
    //    Log.Print ("CollideToEnemy : " + enemy.gameObject.name + " , collisionNormal : " + collisionNormal, LogTypes.Char);
    //    var hurtDirection = collisionNormal.x < 0 ? LifeEnum.HorizontalDirection.Left : LifeEnum.HorizontalDirection.Right;
    //    Hurt (enemy.CollisionDP, hurtDirection);
    //}

    #endregion

    #region Collectable

    public Vector3 GetCurrentCollectedCollectablePos () {
        return collectCollectableRefPoint.position;
    }

    #endregion

    #region Controller

    public void SetAllowUserControl (bool isAllow, bool isOnlyActionInput = false, bool isForceAllow = false) {
        var isReallyAllow = isAllow;
        if (!isForceAllow) {
            if (isAllow) {
                isReallyAllow = UserManager.GetIsAllowUserInput ();

                if (!isReallyAllow) {
                    Log.Print ("Not yet allow user control.", LogTypes.Char | LogTypes.MissionEvent);
                }
            }
        }

        if (isOnlyActionInput) {
            controller.SetCharActionInputActive (isReallyAllow);
        } else {
            controller.SetAllInputActive (isReallyAllow);
        }

    }

    private void TappedHandler () {
        if (isHolding) {
            Log.PrintWarning ("Somehow triggered tap while holding. Do not do tap action.", LogTypes.Char);
            return;
        }

        Log.PrintDebug ("TriggerTapAction", LogTypes.Char);
        isJustTapped = true;



    }

    private void StartedHoldHandler () {
        if (isJustTapped) {
            Log.PrintWarning ("Somehow triggered hold while just tapped. Do not do hold action.", LogTypes.Char);
            return;
        }

        Log.PrintDebug ("StartHoldAction", LogTypes.Char);
        isHolding = true;
    }

    private void StoppedHoldHandler () {
        Log.PrintDebug ("StopHoldAction", LogTypes.Char);
        isHolding = false;
        isJustReleaseHold = true;
    }

    #endregion

    #region Mission Event

    /// <summary>
    /// It is mainly used to break the hold input and force going to release input action
    /// </summary>
    public void BreakUserControl () {
        StartCoroutine (BreakUserControlCoroutine ());
    }

    private IEnumerator BreakUserControlCoroutine () {
        SetAllowUserControl (false);

        yield return null;

        SetAllowUserControl (true);
    }

    public void StopChar (Action onFinished = null) {
        Log.Print ("StopChar", LogTypes.Char);

        BreakInProgressAction (false, false);
        SetAllowMove (false);

        switch (CurrentLocation) {
            case LifeEnum.Location.Air:
                isWaitingLandingToStopChar = true;
                break;
            case LifeEnum.Location.Ground:
            default:
                isWaitingLandingToStopChar = false;
                break;
        }

        StartCoroutine (CheckIsStoppedCharCoroutine (onFinished));
    }

    private IEnumerator CheckIsStoppedCharCoroutine (Action onFinished = null) {
        yield return new WaitForSeconds (Params.StopCharMinWaitTime);

        yield return new WaitWhile (() => isWaitingLandingToStopChar);

        onFinished?.Invoke ();
    }

    public void CancelStopChar () {
        Log.Print ("CancelStopChar", LogTypes.Char);

        SetAllowMove (true);
        isWaitingLandingToStopChar = false;
    }

    public void SetCommandInputMissionEvent (CommandInputSubEvent subEvent, Action onInputFinished) {
        Action onCommandInputFinished = () => {
            controller.SetCameraMovementInputActive (true);
            onInputFinished.Invoke ();
        };

        controller.SetCameraMovementInputActive (false);
        missionEventInputFinishedAction = onCommandInputFinished;
    }

    public void SetCameraInputMissionEvent (CameraInputSubEvent subEvent, Action onInputFinished) {
        Action onCameraInputFinished = () => {
            controller.SetCharActionInputActive (true);
            onInputFinished.Invoke ();
        };

        controller.SetCharActionInputActive (false);
        cameraModel.SetCameraInputMissionEvent (subEvent, onCameraInputFinished);
    }

    #endregion

    #region IMapTarget

    public Vector2 GetTargetPos () {
        return targetRefPoint.position;
    }

    #endregion

    #region MapDisposableBase

    private bool _isDisposeWhenMapReset = false;
    protected override bool IsDisposeWhenMapReset => _isDisposeWhenMapReset;

    protected override void Dispose () {
        // TODO
    }

    public void SetIsDisposeWhenMapReset (bool isDispose) {
        _isDisposeWhenMapReset = isDispose;
    }

    #endregion

}