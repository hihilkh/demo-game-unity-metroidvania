using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;
using UnityEngine.SceneManagement;

public class CharModel : LifeBase<CharParams> {
    private Dictionary<CharEnum.InputSituation, CharEnum.Command> situationToCommandDict = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();

    [SerializeField] private CharController controller;
    [SerializeField] private Animator animator;

    // Body Parts
    public CharEnum.BodyPart obtainedBodyParts { get; private set; } = CharEnum.BodyPart.Head | CharEnum.BodyPart.Arms | CharEnum.BodyPart.Legs | CharEnum.BodyPart.Thrusters | CharEnum.BodyPart.Arrow;
    public event Action<CharEnum.BodyPart> obtainedBodyPartsChangedEvent;

    // Status
    public event Action<CharEnum.Status> statusChangedEvent;

    private CharEnum.Status _currentStatus;
    protected CharEnum.Status currentStatus {
        get {
            return _currentStatus;
        }
        set {
            if (_currentStatus != value) {
                _currentStatus = value;
                statusChangedEvent?.Invoke (_currentStatus);
            }
        }
    }

    public override bool isBeatingBack {
        get { return GetIsInStatus (CharEnum.Status.BeatingBack); }
        protected set { SetStatus (CharEnum.Status.BeatingBack, value); }
    }

    public override bool isInvincible {
        get { return GetIsInStatus (CharEnum.Status.Invincible); }
        protected set { SetStatus (CharEnum.Status.Invincible, value); }
    }

    public override bool isDying {
        get { return GetIsInStatus (CharEnum.Status.Dying); }
        protected set { SetStatus (CharEnum.Status.Dying, value); }
    }

    // Character Situation
    public LifeEnum.HorizontalDirection movingDirection { get; private set; }
    public CharEnum.HorizontalSpeed currentHorizontalSpeed { get; private set; }
    public CharEnum.HitType? currentHitType { get; private set; }
    public CharEnum.ArrowType? currentArrowType { get; private set; }
    private bool isAllowMove;
    private bool isAllowAirJump;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;
    private bool isIgnoreUserInputInThisFrame;

    // Command Control
    private CharEnum.InputSituation? currentInputSituation;
    private CharEnum.Command? currentCommand;
    private LifeEnum.Location startedPressLocation;
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
    private bool isJustTouchWall;
    private bool isTouchingWall;

    // Beat Back
    /// <summary>
    /// Normalized.
    /// </summary>
    public Vector2 beatBackDirection { get; private set; } = Vector2.one;
    private static Vector2 BeatBackDirection_Right = new Vector2 (1, 1).normalized;    // About 45 degree elevation
    private static Vector2 BeatBackDirection_Left = Vector2.Scale (BeatBackDirection_Right, new Vector2 (-1, 1));

    private void Awake () {
        // Remarks :
        // Currently do not add StartedLeft, StoppedLeft, StartedRight, StoppedRight handling to prevent complicated code.
        // Add them back if found them to be useful for development or debugging.
        controller.StartedPressEvent += TriggerStartPressAction;
        controller.TappedEvent += TriggerTapAction;
        controller.StartedHoldEvent += StartHoldAction;
        controller.StoppedHoldEvent += StopHoldAction;

        if (SceneManager.GetActiveScene ().name == GameVariable.MapEditorSceneName) {
            Init (baseTransform.position, GetParams ().initDirection);
            SetAllowMove (true);
        }
    }

    protected override void OnDestroy () {
        base.OnDestroy ();

        controller.StartedPressEvent -= TriggerStartPressAction;
        controller.TappedEvent -= TriggerTapAction;
        controller.StartedHoldEvent -= StartHoldAction;
        controller.StoppedHoldEvent -= StopHoldAction;
    }

    public override bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Init (pos, direction);

        if (hasInitBefore) {
            return hasInitBefore;
        }

        currentStatus = CharEnum.Status.Normal;

        SetAllowMove (false);

        currentHitType = null;
        currentArrowType = null;
        isAllowAirJump = true;

        isJustJumpedUp = false;

        dashCoroutine = null;

        attackCoolDownCoroutine = null;

        isTouchingWall = false;

        ResetAllUpdateControlFlags ();

        return hasInitBefore;
    }

    private void ResetAllUpdateControlFlags () {
        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;

        currentInputSituation = null;
        currentCommand = null;
        startedPressLocation = LifeEnum.Location.Ground;
        isIgnoreHold = false;
        isIgnoreRelease = false;

        isJustTouchWall = false;
        isIgnoreUserInputInThisFrame = false;
    }

    protected override IEnumerator ResetCurrentLocation () {
        yield return StartCoroutine (base.ResetCurrentLocation ());

        // To prevent error if the character is initialized in air. Mainly for development convenience.
        if (currentLocation == LifeEnum.Location.Air) {
            ResetAnimatorTrigger (CharAnimConstant.IdleTriggerName);
            ResetAnimatorTrigger (CharAnimConstant.WalkTriggerName);
            currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
            StartFreeFall ();
        }
    }

    // Remarks :
    // Currently all physics is with sharp changes, so they are stick on Update().
    // Change to stick on FixedUpdate() if continuous changes is needed.
    private void Update () {
        if (!isAllowMove) {
            return;
        }

        // Action by situation and command
        var situation = GetCurrentInputSituation ();
        HandleCommand (situation);

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;
        isJustTouchWall = false;
        isIgnoreUserInputInThisFrame = false;

        if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            isIgnoreHold = false;
            isIgnoreRelease = false;
        }
    }

    public void SetAllowMove (bool isAllowMove) {
        this.isAllowMove = isAllowMove;

        controller.enabled = isAllowMove;

        if (isAllowMove) {
            StartWalking ();
        } else {
            ResetAllUpdateControlFlags ();
            StartIdling ();
        }
    }

    public override void SetPosAndDirection (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        base.SetPosAndDirection (pos, direction);
        movingDirection = facingDirection;
    }

    private void SetStatus (CharEnum.Status status, bool isOn) {
        if (isOn) {
            currentStatus = currentStatus | status;
        } else {
            currentStatus = currentStatus & ~status;
        }
    }

    /// <summary>
    /// If <paramref name="status"/> is composite, it will return true only when currentStatus contains all those status
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool GetIsInStatus (CharEnum.Status status) {
        return (currentStatus & status) == status;
    }

    #region Body Parts

    public CharEnum.BodyPart GetObtainedBodyParts () {
        return obtainedBodyParts;
    }

    public void ObtainBodyPart (CharEnum.BodyPart part) {
        if (!obtainedBodyParts.HasFlag(part)) {
            obtainedBodyParts = obtainedBodyParts | part;

            obtainedBodyPartsChangedEvent?.Invoke (obtainedBodyParts);
        }
    }

    #endregion

    #region Situation and Command

    #region Situation To Command Dictionary

    public void ClearSituationToCommandDict () {
        situationToCommandDict.Clear ();
    }

    public void SetSituationToCommandDict (CharEnum.InputSituation situation, CharEnum.Command command) {
        if (situationToCommandDict.ContainsKey (situation)) {
            situationToCommandDict[situation] = command;
        } else {
            situationToCommandDict.Add (situation, command);
        }
    }

    public void RemoveSituationToCommandDictKey (CharEnum.InputSituation situation) {
        situationToCommandDict.Remove (situation);
    }

    public CharEnum.Command? GetCommandBySituation (CharEnum.InputSituation situation) {
        if (situationToCommandDict.ContainsKey (situation)) {
            return situationToCommandDict[situation];
        } else {
            // TODO : Dev only
            switch (situation) {
                case CharEnum.InputSituation.GroundTap:
                    return GetParams ().groundTapCommand;
                case CharEnum.InputSituation.GroundHold:
                    return GetParams ().groundHoldCommand;
                case CharEnum.InputSituation.GroundRelease:
                    return GetParams ().groundReleaseCommand;
                case CharEnum.InputSituation.AirTap:
                    return GetParams ().airTapCommand;
                case CharEnum.InputSituation.AirHold:
                    return GetParams ().airHoldCommand;
                case CharEnum.InputSituation.AirRelease:
                    return GetParams ().airReleaseCommand;
            }

            return null;
        }
    }

    #endregion

    private CharEnum.InputSituation? GetCurrentInputSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundTap : CharEnum.InputSituation.AirTap;
        } else if (isHolding) {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundHold : CharEnum.InputSituation.AirHold;
        } else {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.InputSituation.GroundRelease : CharEnum.InputSituation.AirRelease;
        }
    }

    private void HandleCommand (CharEnum.InputSituation? optionalSituation) {
        if (optionalSituation == null) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        var situation = (CharEnum.InputSituation)optionalSituation;

        if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
            if (isIgnoreHold) {
                SetCurrentCommandStatus (null, null);
                return;
            } else if (currentInputSituation != CharEnum.InputSituation.GroundHold && currentInputSituation != CharEnum.InputSituation.AirHold) {
                // (The situation of just triggered hold)

                if (isIgnoreUserInputInThisFrame) {
                    // Ignore user input if just trigger hold
                    // TODO : It may lead to some wired cases, e.g.:
                    // trigger hold while touch wall -> hold action and release action failed, but
                    // triggered hold and touch wall later -> hold action and release action success
                    SetCurrentCommandStatus (null, null);
                    return;
                }

                // Hold and release would fail if started to press in air but it is on the ground while triggered hold, or vice versa
                var isHoldFailed = false;
                if (situation == CharEnum.InputSituation.GroundHold && startedPressLocation != LifeEnum.Location.Ground) {
                    isHoldFailed = true;
                } else if (situation == CharEnum.InputSituation.AirHold && startedPressLocation == LifeEnum.Location.Ground) {
                    isHoldFailed = true;
                }

                if (isHoldFailed) {
                    isIgnoreHold = true;
                    isIgnoreRelease = true;
                    SetCurrentCommandStatus (null, null);
                    return;
                }
            }
        } else {
            if (isIgnoreUserInputInThisFrame) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        }

        if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            if (isIgnoreRelease) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        }

        if (GetIsInStatus (CharEnum.Status.DropHitting)) {
            Log.Print ("Ignore InputSituation due to drop hitting. Situation = " + situation, LogType.Char);
            SetCurrentCommandStatus (null, null);

            if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
                isIgnoreHold = true;
                isIgnoreRelease = true;
            }
            return;
        }

        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Handle/Current : InputSituation : " + situation + " / " + currentInputSituation + " ; Command : " + command + " / " + currentCommand, LogType.Char);

        // Fisish the hold command
        if (situation == CharEnum.InputSituation.GroundRelease || situation == CharEnum.InputSituation.AirRelease) {
            if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
                switch (currentCommand) {
                    case CharEnum.Command.Dash:
                        if (command == CharEnum.Command.Dash) {
                            StopDashing (currentHorizontalSpeed, false, false);
                        } else if (command == CharEnum.Command.Jump) {
                            if (CheckIsAllowJump ()) {
                                StopDashing (currentHorizontalSpeed, false, false);
                            } else {
                                StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
                            }
                        } else {
                            StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
                        }
                        break;
                }
            }
        }

        if (command == null) {
            SetCurrentCommandStatus (situation, null);
            return;
        }

        command = (CharEnum.Command)command;
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
                        if (!GetIsInStatus (CharEnum.Status.JumpCharging)) {
                            JumpCharge ();
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        var checkSituation = (situation == CharEnum.InputSituation.GroundRelease) ? CharEnum.InputSituation.GroundHold : CharEnum.InputSituation.AirHold;
                        if (GetCommandBySituation(checkSituation) == CharEnum.Command.Jump) {
                            // That mean this release command should be a charged jump
                            if (GetIsInStatus (CharEnum.Status.JumpCharging)) {
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
                if (GetIsInStatus (CharEnum.Status.DashCollingDown)) {
                    break;
                }

                switch (situation) {
                    case CharEnum.InputSituation.GroundTap:
                    case CharEnum.InputSituation.AirTap:
                        if (!GetIsInStatus (CharEnum.Status.Dashing)) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        if (currentInputSituation == situation) {    // Already dashing
                            if (!isJustTouchWall) {
                                isTriggeredCommand = true;
                            }
                        } else {
                            if (!GetIsInStatus (CharEnum.Status.Dashing)) { // Ensure not trigger hold dash while doing one tap dash
                                StartDashing (false);
                                isTriggeredCommand = true;
                            }
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                    case CharEnum.InputSituation.AirRelease:
                        if (!GetIsInStatus (CharEnum.Status.Dashing)) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                }
                break;
            case CharEnum.Command.Hit:
                if (GetIsInStatus (CharEnum.Status.AttackCoolingDown)) {
                    break;
                }

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
                        if (!GetIsInStatus (CharEnum.Status.DropHitCharging)) {
                            DropHitCharge ();
                        }
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                        hitType = CharEnum.HitType.Finishing;
                        break;
                    case CharEnum.InputSituation.AirRelease:
                        if (currentCommand == CharEnum.Command.Hit) {  // That means, AirHold command is also Hit
                            hitType = CharEnum.HitType.Drop;
                        } else {
                            hitType = CharEnum.HitType.Finishing;
                        }
                        break;
                }

                if (hitType != null) {
                    Hit ((CharEnum.HitType)hitType);
                }
                isTriggeredCommand = true;
                
                break;
            case CharEnum.Command.Arrow:
                if (GetIsInStatus (CharEnum.Status.AttackCoolingDown)) {
                    break;
                }

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
                isTriggeredCommand = true;

                break;
            case CharEnum.Command.Turn:
                // Do not do turn action if touching wall and on ground because changing moving direction is trigged while touch wall
                if (isTouchingWall && currentLocation == LifeEnum.Location.Ground) {
                    break;
                }

                if (GetIsInStatus (CharEnum.Status.Dashing)) {
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
                        if (GetIsInStatus (CharEnum.Status.Sliding)) {
                            var wallDirection = facingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                            RepelFromWall (wallDirection, true);
                        } else {
                            ChangeFacingDirection (false);
                        }
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.InputSituation.GroundHold:
                    case CharEnum.InputSituation.AirHold:
                        Log.PrintWarning ("No action of Turn command is defined for holding. Please check.", LogType.Char);
                        break;
                }
                
                break;
        }

        if (!isTriggeredCommand) {
            if (situation == CharEnum.InputSituation.GroundHold || situation == CharEnum.InputSituation.AirHold) {
                isIgnoreHold = true;
            }
        }
        SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);
    }

    private void SetCurrentCommandStatus (CharEnum.InputSituation? situation, CharEnum.Command? command) {
        currentInputSituation = situation;
        currentCommand = command;

        //Log.PrintDebug ("SetCurrentCommandStatus  : InputSituation : " + currentInputSituation + "   Command : " + currentCommand, LogType.Char);
    }

    /// <param name="isOnlyStopHoldingDash"><b>true</b> means allow one shot dash to keep on</param>
    private void BreakInProgressAction (bool isOnlyStopHoldingDash, bool isChangeMovementAnimIfStoppedDashing) {

        StopJumpCharge ();
        StopDropHitCharge ();
        StopDropHit ();

        if (!isOnlyStopHoldingDash) {
            StopDashing (CharEnum.HorizontalSpeed.Walk, true, isChangeMovementAnimIfStoppedDashing);
        }

        if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
            if (isOnlyStopHoldingDash) {
                if (currentCommand == CharEnum.Command.Dash) {
                    StopDashing (CharEnum.HorizontalSpeed.Walk, true, isChangeMovementAnimIfStoppedDashing);
                }
            }

            isIgnoreHold = true;
            isIgnoreRelease = true;
        }
    }

    #endregion

    #region Animtor

    private void SetAnimatorTrigger (string trigger) {
        Log.PrintDebug ("Char SetAnimatorTrigger : " + trigger, LogType.Char | LogType.Animation);
        animator.SetTrigger (trigger);
    }

    private void ResetAnimatorTrigger (string trigger) {
        Log.PrintDebug ("Char ResetAnimatorTrigger : " + trigger, LogType.Char | LogType.Animation);
        animator.ResetTrigger (trigger);
    }
    
    protected void SetAnimatorBool (string boolName, bool value) {
        Log.PrintDebug ("Char SetAnimatorBool : " + boolName + " ; Value : " + value, LogType.Char | LogType.Animation);
        animator.SetBool (boolName, value);
    }

    #endregion

    #region Idle / Walk

    public void StartIdleOrWalk () {
        switch (currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                StartIdling ();
                break;
            case CharEnum.HorizontalSpeed.Walk:
                StartWalking ();
                break;
            default:
                Log.PrintWarning ("Current horizontal speed is : " + currentHorizontalSpeed + " . It does not match idle or walk. Force to change to walk. Please check.", LogType.Char);
                StartWalking ();
                break;
        }
    }

    private void StartIdling () {
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        SetAnimatorTrigger (CharAnimConstant.IdleTriggerName);
    }

    private void StartWalking () {
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;

        SetAnimatorTrigger (CharAnimConstant.WalkTriggerName);
    }

    #endregion

    #region Dash

    private void StartDashing (bool isOneShot) {
        Log.Print ("Dash : isOneShot = " + isOneShot, LogType.Char);

        StopDashing (currentHorizontalSpeed, false, false);  // To ensure do not trigger 2 dash coroutines at the same time

        if (GetIsInStatus (CharEnum.Status.Sliding)) {
            SetStatus (CharEnum.Status.Sliding, false);
        }

        if (isOneShot) {
            dashCoroutine = StartCoroutine (OneShotDashCoroutine ());
        } else {
            SetDashing ();
        }
    }

    private void StopDashing (CharEnum.HorizontalSpeed speedAfterStopDashing, bool isNeedDashCoolDown, bool isNeedToChangeMovementAnim) {
        // Remarks:
        // There is a case that calling StopDashing() while it is already in dash cool down stage.
        // Below is to ensure no dash cool down if isNeedDashCoolDown = false
        if (!isNeedDashCoolDown) {
            if (dashCoolDownCoroutine != null) {
                StopCoroutine (dashCoolDownCoroutine);
                SetStatus (CharEnum.Status.DashCollingDown, false);
                dashCoolDownCoroutine = null;
            }
        }

        if (!GetIsInStatus (CharEnum.Status.Dashing)) {
            return;
        }

        if (dashCoroutine != null) {
            StopCoroutine (dashCoroutine);
            dashCoroutine = null;
        }

        SetStatus (CharEnum.Status.Dashing, false);
        currentHorizontalSpeed = speedAfterStopDashing;
        if (isNeedToChangeMovementAnim) {
            if (currentLocation == LifeEnum.Location.Ground) {
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
    }

    private void SetDashing () {
        SetStatus (CharEnum.Status.Dashing, true);
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Dash;
        AlignMovingWithFacingDirection ();

        SetAnimatorTrigger (CharAnimConstant.DashTriggerName);
    }

    private IEnumerator OneShotDashCoroutine () {
        SetDashing ();

        var startTime = Time.time;

        while (Time.time - startTime < GetParams ().oneShotDashPeriod) {
            if (isJustTouchWall) {
                break;
            }

            yield return null;
        }

        // If touching wall, sliding animation dominate the animation
        StopDashing (CharEnum.HorizontalSpeed.Walk, true, isJustTouchWall ? false : true);
    }

    private IEnumerator DashCoolDownCoroutine () {
        SetStatus (CharEnum.Status.DashCollingDown, true);

        yield return new WaitForSeconds (GetParams ().dashCoolDownPeriod);

        SetStatus (CharEnum.Status.DashCollingDown, false);
        dashCoolDownCoroutine = null;
    }

    #endregion

    #region Jump

    public float GetCurrentJumpInitSpeed () {
        return GetIsInStatus (CharEnum.Status.JumpCharging) ? GetParams ().chargeJumpInitSpeed : GetParams ().normalJumpInitSpeed;
    }

    private bool CheckIsAllowJump () {
        if (currentLocation == LifeEnum.Location.Ground) {
            return true;
        }

        return isAllowAirJump;
    }

    private IEnumerator Jump () {
        Log.Print ("Jump : isCharged = " + GetIsInStatus (CharEnum.Status.JumpCharging), LogType.Char);

        StopDashing (currentHorizontalSpeed, false, false);

        if (GetIsInStatus (CharEnum.Status.Sliding)) {
            SetStatus (CharEnum.Status.Sliding, false);
        }

        if (currentHorizontalSpeed == CharEnum.HorizontalSpeed.Idle) {
            currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
        }
        AlignMovingWithFacingDirection ();

        if (currentLocation == LifeEnum.Location.Ground) {
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
        if (GetIsInStatus (CharEnum.Status.JumpCharging)) {
            return;
        }

        Log.PrintDebug ("JumpCharge", LogType.Char);
        SetStatus (CharEnum.Status.JumpCharging, true);
    }

    private void StopJumpCharge () {
        if (!GetIsInStatus (CharEnum.Status.JumpCharging)) {
            return;
        }

        Log.PrintDebug ("StopJumpCharge", LogType.Char);
        SetStatus (CharEnum.Status.JumpCharging, false);
    }

    #endregion

    #region Hit

    private void Hit (CharEnum.HitType hitType) {
        if (GetIsInStatus (CharEnum.Status.AttackCoolingDown)) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger Hit action. Please check.", LogType.Char);
            return;
        }

        if (GetIsInStatus (CharEnum.Status.DropHitting)) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger Hit action. Please check.", LogType.Char);
            return;
        }

        Log.Print ("Hit : HitType = " + hitType, LogType.Char);
        currentHitType = hitType;

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
        Log.Print ("Start DropHit", LogType.Char);

        if (GetIsInStatus (CharEnum.Status.Sliding)) {
            var wallDirection = facingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
            RepelFromWall (wallDirection, true);
        }

        StopDropHitCharge ();
        SetStatus (CharEnum.Status.DropHitting, true);

        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        SetAnimatorTrigger (CharAnimConstant.DropHitTriggerName);
    }

    private void StopDropHit () {
        if (!GetIsInStatus (CharEnum.Status.DropHitting)) {
            return;
        }

        Log.Print ("Stop DropHit", LogType.Char);

        SetStatus (CharEnum.Status.DropHitting, false);

        attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (CharEnum.HitType.Drop));
    }

    private void DropHitCharge () {
        if (GetIsInStatus (CharEnum.Status.DropHitCharging)) {
            return;
        }

        SetStatus (CharEnum.Status.DropHitCharging, true);
    }

    private void StopDropHitCharge () {
        if (!GetIsInStatus (CharEnum.Status.DropHitCharging)) {
            return;
        }

        SetStatus (CharEnum.Status.DropHitCharging, false);
    }

    private IEnumerator HitCoolDownCoroutine (CharEnum.HitType hitType) {
        SetStatus (CharEnum.Status.AttackCoolingDown, true);

        var hitCoolDownPeriod = 0f;

        switch (hitType) {
            case CharEnum.HitType.Normal:
                hitCoolDownPeriod = GetParams ().hitCoolDownPeriod_Normal;
                break;
            case CharEnum.HitType.Charged:
                hitCoolDownPeriod = GetParams ().hitCoolDownPeriod_Charged;
                break;
            case CharEnum.HitType.Finishing:
                hitCoolDownPeriod = GetParams ().hitCoolDownPeriod_Finishing;
                break;
            case CharEnum.HitType.Drop:
                hitCoolDownPeriod = GetParams ().hitCoolDownPeriod_Drop;
                break;
            default:
                Log.PrintWarning ("Not yet set hit cool down period for HitType : " + hitType + " . Assume cool down period to be 0s", LogType.Char);
                break;
        }

        yield return new WaitForSeconds (hitCoolDownPeriod);

        currentHitType = null;
        SetStatus (CharEnum.Status.AttackCoolingDown, false);
        attackCoolDownCoroutine = null;
    }

    #endregion

    #region Arrow

    private void ShootArrow (CharEnum.ArrowType arrowType) {
        if (GetIsInStatus (CharEnum.Status.AttackCoolingDown)) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger shoot arrow action. Please check.", LogType.Char);
            return;
        }

        if (GetIsInStatus (CharEnum.Status.DropHitting)) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger shoot arrow action. Please check.", LogType.Char);
            return;
        }

        Log.Print ("Shoot arrow : ArrowType = " + arrowType, LogType.Char);
        currentArrowType = arrowType;

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
        SetStatus (CharEnum.Status.AttackCoolingDown, true);

        var arrowCoolDownPeriod = 0f;

        switch (arrowType) {
            case CharEnum.ArrowType.Target:
                arrowCoolDownPeriod = GetParams ().arrowCoolDownPeriod_Target;
                break;
            case CharEnum.ArrowType.Straight:
                arrowCoolDownPeriod = GetParams ().arrowCoolDownPeriod_Straight;
                break;
            case CharEnum.ArrowType.Triple:
                arrowCoolDownPeriod = GetParams ().arrowCoolDownPeriod_Triple;
                break;
            default:
                Log.PrintWarning ("Not yet set arrow cool down period for ArrowType : " + arrowType + " . Assume cool down period to be 0s", LogType.Char);
                break;
        }

        yield return new WaitForSeconds (arrowCoolDownPeriod);

        currentArrowType = null;
        SetStatus (CharEnum.Status.AttackCoolingDown, false);
        attackCoolDownCoroutine = null;
    }

    public Transform SearchShootTarget () {
        // TODO
        return null;
    }

    #endregion

    #region Change Direction

    private void ChangeFacingDirection (bool isAlignMovingDirection) {
        Log.PrintDebug ("ChangeFacingDirection : isAlignMovingDirection = " + isAlignMovingDirection, LogType.Char);
        if (facingDirection == LifeEnum.HorizontalDirection.Left) {
            facingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            facingDirection = LifeEnum.HorizontalDirection.Left;
        }

        if (isAlignMovingDirection) {
            movingDirection = facingDirection;
        }
    }

    private void ChangeMovingDirection () {
        // Remarks : Changing moving direction must also align facing direction

        Log.PrintDebug ("ChangeMovingDirection", LogType.Char);
        if (movingDirection == LifeEnum.HorizontalDirection.Left) {
            movingDirection = LifeEnum.HorizontalDirection.Right;
        } else {
            movingDirection = LifeEnum.HorizontalDirection.Left;
        }

        facingDirection = movingDirection;
    }

    private void SetMovingDirection (LifeEnum.HorizontalDirection direction) {
        // Remarks : Changing moving direction must also align facing direction
        movingDirection = direction;
        facingDirection = movingDirection;
    }

    private void AlignMovingWithFacingDirection () {
        movingDirection = facingDirection;
    }

    #endregion

    #region HP

    public override bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        var isAlive = base.Hurt (dp, hurtDirection);

        Log.Print ("Char : Hurt! dp : " + dp + " , hurtDirection : " + hurtDirection + " , remain HP : " + currentHP, LogType.Char);

        return isAlive;
    }

    private void DieByTouchedDeathTag () {
        Die (movingDirection);
    }

    protected override void Die (LifeEnum.HorizontalDirection dieDirection) {
        base.Die (dieDirection);

        Log.Print ("Char : Die!", LogType.Char);
        SetAnimatorTrigger (CharAnimConstant.DieTriggerName);
        StartCoroutine (WaitAndFinishDying ());
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        base.StartBeatingBack (hurtDirection);

        BreakInProgressAction (false, false);
        controller.enabled = false;

        // If dying, dominated by die animation
        if (!isDying) {
            if (GetIsInStatus (CharEnum.Status.Sliding)) {
                var wallDirection = facingDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                RepelFromWall (wallDirection, true);
            } else {
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? BeatBackDirection_Left : BeatBackDirection_Right;
                movingDirection = hurtDirection;

                SetAnimatorTrigger (CharAnimConstant.BeatBackTriggerName);
            }
        }


        StartCoroutine (WaitAndFinishBeatingBack ());
    }

    protected override void StopBeatingBack () {
        base.StopBeatingBack ();

        controller.enabled = true;
    }

    protected override void StartInvincible () {
        base.StartInvincible ();

        // If dying, dominated by die animation
        if (!isDying) {
            SetAnimatorBool (CharAnimConstant.InvincibleBoolName, true);
        }
    }

    protected override void StopInvincible () {
        base.StopInvincible ();

        SetAnimatorBool (CharAnimConstant.InvincibleBoolName, false);
    }

    private void CollideToEnemy (EnemyModelBase enemy, Vector2 collisionNormal) {
        Log.Print ("CollideToEnemy : " + enemy.gameObject.name + " , collisionNormal : " + collisionNormal, LogType.Char);
        var hurtDirection = collisionNormal.x < 0 ? LifeEnum.HorizontalDirection.Left : LifeEnum.HorizontalDirection.Right;
        Hurt (enemy.collisionDP, hurtDirection);
    }

    private IEnumerator WaitAndFinishBeatingBack () {
        yield return new WaitForSeconds (GetParams ().beatBackPeriod);

        StopBeatingBack ();
    }

    private IEnumerator WaitAndFinishDying () {
        yield return new WaitForSeconds (GetParams ().dyingPeriod);

        // TODO
    }
    #endregion

    #region Collision Event

    protected override void RegisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent += TouchGround;
        lifeCollision.LeftGroundEvent += LeaveGround;
        lifeCollision.TouchedWallEvent += TouchWall;
        lifeCollision.LeftWallEvent += LeaveWall;
        lifeCollision.TouchedDeathTagEvent += DieByTouchedDeathTag;
        lifeCollision.TouchedEnemyEvent += CollideToEnemy;
    }

    protected override void UnregisterCollisionEventHandler () {
        lifeCollision.TouchedGroundEvent -= TouchGround;
        lifeCollision.LeftGroundEvent -= LeaveGround;
        lifeCollision.TouchedWallEvent -= TouchWall;
        lifeCollision.LeftWallEvent -= LeaveWall;
        lifeCollision.TouchedDeathTagEvent -= DieByTouchedDeathTag;
        lifeCollision.TouchedEnemyEvent -= CollideToEnemy;
    }

    private void TouchGround () {
        Log.PrintDebug ("Char : TouchGround", LogType.Char);

        isIgnoreUserInputInThisFrame = true;

        // Touch ground while init / set position
        if (currentLocation == LifeEnum.Location.Unknown) {
            currentLocation = LifeEnum.Location.Ground;
            return;
        }

        if (!GetIsInStatus (CharEnum.Status.Dashing)) {
            currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
            AlignMovingWithFacingDirection ();

            if (GetIsInStatus (CharEnum.Status.Sliding)) {
                SetStatus (CharEnum.Status.Sliding, false);
                StartWalking ();
            } else if (currentLocation == LifeEnum.Location.Air) {
                SetAnimatorTrigger (CharAnimConstant.LandingTriggerName);
            }
        }

        isAllowAirJump = true;
        currentLocation = LifeEnum.Location.Ground;

        BreakInProgressAction (true, true);
    }

    private void LeaveGround () {
        Log.PrintDebug ("Char : LeaveGround", LogType.Char);

        isIgnoreUserInputInThisFrame = true;

        currentLocation = LifeEnum.Location.Air;

        if (isBeatingBack) {
            // Dominate by beat back handling
            return;
        }

        BreakInProgressAction (true, false);

        if (isJustJumpedUp) {
            isJustJumpedUp = false;
        } else if (GetIsInStatus (CharEnum.Status.Dashing)) {
            // Keep dashing
        } else {
            StartFreeFall ();
        }
    }

    private void TouchWall (LifeEnum.HorizontalDirection wallPosition, bool isSlippyWall) {
        Log.PrintDebug ("Char : TouchWall : isSlippyWall = " + isSlippyWall, LogType.Char);

        isIgnoreUserInputInThisFrame = true;
        isJustTouchWall = true;
        isTouchingWall = true;

        if (wallPosition == movingDirection) {  // Change moving direction only when char originally move towards wall
            // if it is slippy wall, do not change moving direction when in air
            if (!isSlippyWall || currentLocation == LifeEnum.Location.Ground) {
                ChangeMovingDirection ();
            }
        }

        if (GetIsInStatus (CharEnum.Status.Dashing)) {
            StopDashing (CharEnum.HorizontalSpeed.Walk, true, currentLocation == LifeEnum.Location.Ground);

            // Break hold and release input if "GroundHold - Dash" or "AirHold - Dash"
            if (currentInputSituation == CharEnum.InputSituation.GroundHold || currentInputSituation == CharEnum.InputSituation.AirHold) {
                if (currentCommand == CharEnum.Command.Dash) {
                    isIgnoreHold = true;
                    isIgnoreRelease = true;
                }
            }
        }

        if (currentLocation == LifeEnum.Location.Air) {
            if (isSlippyWall) {
                currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
                RepelFromWall (facingDirection, true);
            } else {
                StartSliding ();
            }
        }
    }

    private void LeaveWall (bool isSlippyWall) {
        Log.PrintDebug ("Char : LeaveWall : isSlippyWall = " + isSlippyWall, LogType.Char);

        isTouchingWall = false;
        isIgnoreUserInputInThisFrame = true;

        if (GetIsInStatus (CharEnum.Status.Sliding)) {  // The case that char is sliding to the end of the wall and then free fall
            SetStatus (CharEnum.Status.Sliding, false);
            ChangeMovingDirection ();
            StartFreeFall ();
        }
    }

    private void StartSliding () {
        SetStatus (CharEnum.Status.Sliding, true);
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
        isAllowAirJump = true;   // Allow jump in air again

        SetAnimatorTrigger (CharAnimConstant.SlideTriggerName);
    }

    private void RepelFromWall (LifeEnum.HorizontalDirection wallDirection, bool isFinallyFacingWall) {
        SetStatus (CharEnum.Status.Sliding, false);

        var directionMultiplier = wallDirection == LifeEnum.HorizontalDirection.Left ? 1 : -1;
        SetPosByOffset (new Vector2 (GetParams ().repelFromWallDistByTurn, 0) * directionMultiplier);

        if (isFinallyFacingWall) {
            SetMovingDirection (wallDirection);
        } else {
            SetMovingDirection (wallDirection == LifeEnum.HorizontalDirection.Left ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left);
        }

        StartFreeFall ();
    }

    private void StartFreeFall () {
        SetAnimatorTrigger (CharAnimConstant.FreeFallTriggerName);
    }

    #endregion

    #region Controller Event

    private void TriggerStartPressAction () {
        startedPressLocation = currentLocation;

        Log.PrintDebug ("TriggerStartPressAction", LogType.Char);
    }

    private void TriggerTapAction () {
        if (isHolding) {
            Log.PrintWarning ("Somehow triggered tap while holding. Do not do tap action.", LogType.Char);
            return;
        }

        Log.PrintDebug ("TriggerTapAction", LogType.Char);
        isJustTapped = true;
    }

    private void StartHoldAction () {
        if (isJustTapped) {
            Log.PrintWarning ("Somehow triggered hold while just tapped. Do not do hold action.", LogType.Char);
            return;
        }

        Log.PrintDebug ("StartHoldAction", LogType.Char);
        isHolding = true;
    }

    private void StopHoldAction () {
        Log.PrintDebug ("StopHoldAction", LogType.Char);
        isHolding = false;
        isJustReleaseHold = true;
    }

    #endregion
}