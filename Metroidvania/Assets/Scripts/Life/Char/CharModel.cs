using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;
using UnityEngine.SceneManagement;

public class CharModel : LifeBase<CharParams> {
    private Dictionary<CharEnum.CommandSituation, CharEnum.Command> situationToCommandDict = new Dictionary<CharEnum.CommandSituation, CharEnum.Command> ();

    [SerializeField] private CharController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private CharPSHandler _psHandler;
    public CharPSHandler psHandler => _psHandler;

    // Body Parts
    public CharEnum.BodyPart obtainedBodyParts { get; private set; } = CharEnum.BodyPart.Head | CharEnum.BodyPart.Arms | CharEnum.BodyPart.Legs | CharEnum.BodyPart.Thrusters | CharEnum.BodyPart.Arrow;
    public event Action<CharEnum.BodyPart> obtainedBodyPartsChangedEvent;

    // Status
    public event Action<LifeEnum.Status> statusChangedEvent;
    private LifeEnum.Status _currentStatus;
    protected override LifeEnum.Status currentStatus {
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
    private CharEnum.CommandSituation? currentSituation;
    private CharEnum.Command? currentCommand;
    private LifeEnum.Location startedPressLocation;
    private bool isIgnoreHold;
    private bool isIgnoreRelease;

    // Jump Command Control
    public bool isJumpCharging { get; private set; }
    private bool isJustJumpedUp;

    // Dash Command Control
    private bool isDashing;
    private bool isDashCoolingDown;
    private Coroutine dashCoroutine;
    private Coroutine dashCoolDownCoroutine;

    // Hit / Arrow Command Control
    private bool isAttackCoolingDown;
    private bool isDropHitCharging;
    private bool isDropHitting;
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

    // TODO : Handle the case that init / set position in air
    public override bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitBefore = base.Init (pos, direction);

        if (hasInitBefore) {
            return hasInitBefore;
        }

        SetAllowMove (false);
        currentLocation = LifeEnum.Location.Ground;
        currentHitType = null;
        currentArrowType = null;
        isAllowAirJump = true;

        isJumpCharging = false;
        isJustJumpedUp = false;

        isDashing = false;
        isDashCoolingDown = false;
        dashCoroutine = null;

        isAttackCoolingDown = false;
        isDropHitCharging = false;
        isDropHitting = false;
        attackCoolDownCoroutine = null;

        isTouchingWall = false;

        ResetAllUpdateControlFlags ();

        return hasInitBefore;
    }

    private void ResetAllUpdateControlFlags () {
        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;

        currentSituation = null;
        currentCommand = null;
        startedPressLocation = LifeEnum.Location.Ground;
        isIgnoreHold = false;
        isIgnoreRelease = false;

        isJustTouchWall = false;
        isIgnoreUserInputInThisFrame = false;
    }

    // Remarks :
    // Currently all physics is with sharp changes, so they are stick on Update().
    // Change to stick on FixedUpdate() if continuous changes is needed.
    private void Update () {
        if (!isAllowMove) {
            return;
        }

        // Action by situation and command
        var situation = GetCurrentCommandSituation ();
        HandleCommand (situation);

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;
        isJustTouchWall = false;
        isIgnoreUserInputInThisFrame = false;

        if (situation == CharEnum.CommandSituation.GroundRelease || situation == CharEnum.CommandSituation.AirRelease) {
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

    public void SetSituationToCommandDict (CharEnum.CommandSituation situation, CharEnum.Command command) {
        if (situationToCommandDict.ContainsKey (situation)) {
            situationToCommandDict[situation] = command;
        } else {
            situationToCommandDict.Add (situation, command);
        }
    }

    public void RemoveSituationToCommandDictKey (CharEnum.CommandSituation situation) {
        situationToCommandDict.Remove (situation);
    }

    public CharEnum.Command? GetCommandBySituation (CharEnum.CommandSituation situation) {
        if (situationToCommandDict.ContainsKey (situation)) {
            return situationToCommandDict[situation];
        } else {
            // TODO : Dev only
            switch (situation) {
                case CharEnum.CommandSituation.GroundTap:
                    return GetParams ().groundTapCommand;
                case CharEnum.CommandSituation.GroundHold:
                    return GetParams ().groundHoldCommand;
                case CharEnum.CommandSituation.GroundRelease:
                    return GetParams ().groundReleaseCommand;
                case CharEnum.CommandSituation.AirTap:
                    return GetParams ().airTapCommand;
                case CharEnum.CommandSituation.AirHold:
                    return GetParams ().airHoldCommand;
                case CharEnum.CommandSituation.AirRelease:
                    return GetParams ().airReleaseCommand;
            }

            return null;
        }
    }

    #endregion

    private CharEnum.CommandSituation? GetCurrentCommandSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.CommandSituation.GroundTap : CharEnum.CommandSituation.AirTap;
        } else if (isHolding) {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.CommandSituation.GroundHold : CharEnum.CommandSituation.AirHold;
        } else {
            return currentLocation == LifeEnum.Location.Ground ? CharEnum.CommandSituation.GroundRelease : CharEnum.CommandSituation.AirRelease;
        }
    }

    private void HandleCommand (CharEnum.CommandSituation? optionalSituation) {
        if (optionalSituation == null) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        var situation = (CharEnum.CommandSituation)optionalSituation;

        if (situation == CharEnum.CommandSituation.GroundHold || situation == CharEnum.CommandSituation.AirHold) {
            if (isIgnoreHold) {
                SetCurrentCommandStatus (null, null);
                return;
            } else if (currentSituation != CharEnum.CommandSituation.GroundHold && currentSituation != CharEnum.CommandSituation.AirHold) {
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
                if (situation == CharEnum.CommandSituation.GroundHold && startedPressLocation != LifeEnum.Location.Ground) {
                    isHoldFailed = true;
                } else if (situation == CharEnum.CommandSituation.AirHold && startedPressLocation == LifeEnum.Location.Ground) {
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

        if (situation == CharEnum.CommandSituation.GroundRelease || situation == CharEnum.CommandSituation.AirRelease) {
            if (isIgnoreRelease) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        }

        if (isDropHitting) {
            Log.Print ("Ignore command situation due to drop hitting. situation = " + situation, LogType.Char);
            SetCurrentCommandStatus (null, null);

            if (situation == CharEnum.CommandSituation.GroundHold || situation == CharEnum.CommandSituation.AirHold) {
                isIgnoreHold = true;
            }
            return;
        }

        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Handle/Current : Situation : " + situation + " / " + currentSituation + " ; Command : " + command + " / " + currentCommand, LogType.Char);

        // Fisish the hold command
        if (situation == CharEnum.CommandSituation.GroundRelease || situation == CharEnum.CommandSituation.AirRelease) {
            if (currentSituation == CharEnum.CommandSituation.GroundHold || currentSituation == CharEnum.CommandSituation.AirHold) {
                switch (currentCommand) {
                    case CharEnum.Command.Dash:
                        if (command == CharEnum.Command.Jump || command == CharEnum.Command.Dash) {
                            StopDashing (currentHorizontalSpeed, false, false);
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
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.AirTap:
                        StartCoroutine (Jump ());
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                    case CharEnum.CommandSituation.AirHold:
                        JumpCharge ();
                        break;
                    case CharEnum.CommandSituation.GroundRelease:
                    case CharEnum.CommandSituation.AirRelease:
                        var checkSituation = (situation == CharEnum.CommandSituation.GroundRelease) ? CharEnum.CommandSituation.GroundHold : CharEnum.CommandSituation.AirHold;
                        if (GetCommandBySituation(checkSituation) == CharEnum.Command.Jump) {
                            // That mean this release command should be a charged jump
                            if (isJumpCharging) {
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
                if (isDashCoolingDown) {
                    break;
                }

                switch (situation) {
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.AirTap:
                        if (!isDashing) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                    case CharEnum.CommandSituation.AirHold:
                        if (currentSituation == situation) {    // Already dashing
                            if (!isJustTouchWall) {
                                isTriggeredCommand = true;
                            }
                        } else {
                            if (!isDashing) {                    // Ensure not trigger hold dash while doing one tap dash
                                StartDashing (false);
                                isTriggeredCommand = true;
                            }
                        }
                        break;
                    case CharEnum.CommandSituation.GroundRelease:
                    case CharEnum.CommandSituation.AirRelease:
                        if (!isDashing) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                }
                break;
            case CharEnum.Command.Hit:
                if (isAttackCoolingDown) {
                    break;
                }

                CharEnum.HitType? hitType = null;
                switch (situation) {
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.AirTap:
                        hitType = CharEnum.HitType.Normal;
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                        hitType = CharEnum.HitType.Charged;
                        isIgnoreHold = true;
                        break;
                    case CharEnum.CommandSituation.AirHold:
                        DropHitCharge ();
                        break;
                    case CharEnum.CommandSituation.GroundRelease:
                        hitType = CharEnum.HitType.Finishing;
                        break;
                    case CharEnum.CommandSituation.AirRelease:
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
                if (isAttackCoolingDown) {
                    if (situation == CharEnum.CommandSituation.GroundHold || situation == CharEnum.CommandSituation.AirHold) {
                        isIgnoreHold = true;
                    }
                    break;
                }

                CharEnum.ArrowType? arrowType = null;
                switch (situation) {
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.AirTap:
                        arrowType = CharEnum.ArrowType.Target;
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                    case CharEnum.CommandSituation.AirHold:
                        arrowType = CharEnum.ArrowType.Straight;
                        isIgnoreHold = true;
                        break;
                    case CharEnum.CommandSituation.GroundRelease:
                    case CharEnum.CommandSituation.AirRelease:
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

                if (isDashing) {
                    StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
                }

                switch (situation) {
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.GroundRelease:
                        ChangeFacingDirection (true);
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.CommandSituation.AirTap:
                    case CharEnum.CommandSituation.AirRelease:
                        if (currentLocation == LifeEnum.Location.Wall) {
                            RepelFromWallAndStartFreeFall (true);
                        } else {
                            ChangeFacingDirection (false);
                        }
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                    case CharEnum.CommandSituation.AirHold:
                        Log.PrintWarning ("No action of Turn command is defined for holding. Please check.", LogType.Char);
                        break;
                }
                
                break;
        }

        if (!isTriggeredCommand) {
            if (situation == CharEnum.CommandSituation.GroundHold || situation == CharEnum.CommandSituation.AirHold) {
                isIgnoreHold = true;
            }
        }
        SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);
    }

    private void SetCurrentCommandStatus (CharEnum.CommandSituation? situation, CharEnum.Command? command) {
        currentSituation = situation;
        currentCommand = command;

        //Log.PrintDebug ("SetCurrentCommandStatus  : Situation : " + currentSituation + "   Command : " + currentCommand, LogType.Char);
    }

    #endregion

    #region Animtor

    private void SetAnimatorTrigger (string trigger) {
        Log.PrintDebug ("Char SetAnimatorTrigger : " + trigger, LogType.Char | LogType.Animation);
        animator.SetTrigger (trigger);
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

        if (currentLocation == LifeEnum.Location.Wall) {
            currentLocation = LifeEnum.Location.Air;
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
                dashCoolDownCoroutine = null;
                isDashCoolingDown = false;
            }
        }

        if (!isDashing) {
            return;
        }

        if (dashCoroutine != null) {
            StopCoroutine (dashCoroutine);
            dashCoroutine = null;
        }

        isDashing = false;
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
        isDashing = true;
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Dash;

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
        isDashCoolingDown = true;

        yield return new WaitForSeconds (GetParams ().dashCoolDownPeriod);

        isDashCoolingDown = false;
        dashCoolDownCoroutine = null;
    }

    #endregion

    #region Jump

    public float GetCurrentJumpInitSpeed () {
        return isJumpCharging ? GetParams ().chargeJumpInitSpeed : GetParams ().normalJumpInitSpeed;
    }

    private bool CheckIsAllowJump () {
        if (currentLocation == LifeEnum.Location.Ground) {
            return true;
        }

        return isAllowAirJump;
    }

    private IEnumerator Jump () {
        Log.Print ("Jump : isCharged = " + isJumpCharging, LogType.Char);

        StopDashing (currentHorizontalSpeed, false, false);

        if (currentLocation == LifeEnum.Location.Wall) {
            currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
        }

        if (currentLocation == LifeEnum.Location.Ground) {
            isJustJumpedUp = true;
        } else {
            isAllowAirJump = false;
        }

        currentLocation = LifeEnum.Location.Air;


        SetAnimatorTrigger (CharAnimConstant.JumpTriggerName);

        // Remarks : WaitForEndOfFrame in order to let CharJumpSMB know the jump is charged
        yield return new WaitForEndOfFrame ();
        StopJumpCharge ();
    }

    private void JumpCharge () {
        if (!isJumpCharging) {
            Log.PrintDebug ("JumpCharge", LogType.Char);
            isJumpCharging = true;
            isIgnoreHold = true;

            psHandler.SetJumpChargePS (true);
        }
    }

    private void StopJumpCharge () {
        if (isJumpCharging) {
            isJumpCharging = false;

            Log.PrintDebug ("StopJumpCharge", LogType.Char);

            psHandler.SetJumpChargePS (false);
        }
    }

    #endregion

    #region Hit

    private void Hit (CharEnum.HitType hitType) {
        if (isAttackCoolingDown) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger Hit action. Please check.", LogType.Char);
            return;
        }

        if (isDropHitting) {
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

        StopDropHitCharge ();
        isDropHitting = true;

        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        SetAnimatorTrigger (CharAnimConstant.DropHitTriggerName);
    }

    private void FinishDropHit () {
        Log.Print ("Finish DropHit", LogType.Char);

        isDropHitting = false;
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;

        attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (CharEnum.HitType.Drop));
    }

    private void DropHitCharge () {
        if (!isDropHitCharging) {
            isDropHitCharging = true;

            psHandler.SetDropHitChargePS (true);
        }
    }

    private void StopDropHitCharge () {
        if (isDropHitCharging) {
            isDropHitCharging = false;

            psHandler.SetDropHitChargePS (false);
        }
    }

    private IEnumerator HitCoolDownCoroutine (CharEnum.HitType hitType) {
        isAttackCoolingDown = true;

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
        isAttackCoolingDown = false;
        attackCoolDownCoroutine = null;
    }

    #endregion

    #region Arrow

    private void ShootArrow (CharEnum.ArrowType arrowType) {
        if (isAttackCoolingDown) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger shoot arrow action. Please check.", LogType.Char);
            return;
        }

        if (isDropHitting) {
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
        isAttackCoolingDown = true;

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
        isAttackCoolingDown = false;
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

        // TODO
        // Clear all input action (e.g. charging, hold / release action)
        // Do not allow input while beating back

        // If dying, dominated by die animation
        if (!GetIsDying ()) {
            if (currentLocation == LifeEnum.Location.Wall) {
                RepelFromWallAndStartFreeFall (true);
            } else {
                beatBackDirection = hurtDirection == LifeEnum.HorizontalDirection.Left ? BeatBackDirection_Left : BeatBackDirection_Right;
                movingDirection = hurtDirection;

                SetAnimatorTrigger (CharAnimConstant.BeatBackTriggerName);
            }
        }

        StartCoroutine (WaitAndFinishBeatingBack ());
    }

    protected override void StartInvincible () {
        base.StartInvincible ();

        // If dying, dominated by die animation
        if (!GetIsDying ()) {
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

        if (isDashing) {
            if (currentSituation == CharEnum.CommandSituation.AirHold) {         // "AirHold - Dash" command
                StopDashing (CharEnum.HorizontalSpeed.Walk, true, true);
            } else {
                // Remarks :
                // That is, one shot dash and somehow touch ground during dash (e.g.dashing through a 凹 shape)
                // Do not do landing stuff. Handle by StopDashing
            }
        } else {
            // TODO : Think of a better way to handle all these bool flag, VFX, speial handling...
            if (isDropHitting) {         // "AirRelease - Hit" command
                FinishDropHit ();
            } else {
                if (isDropHitCharging) {     // "AirHold - Hit" command
                    StopDropHitCharge ();
                } else if (isJumpCharging) {         // "AirHold - Jump" command
                    StopJumpCharge ();
                }
                currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
                AlignMovingWithFacingDirection ();
            }

            switch (currentLocation) {
                case LifeEnum.Location.Wall:
                    StartWalking ();
                    break;
                case LifeEnum.Location.Air:
                    SetAnimatorTrigger (CharAnimConstant.LandingTriggerName);
                    break;
                default:
                    // Do nothing
                    break;
            }
        }

        isAllowAirJump = true;
        currentLocation = LifeEnum.Location.Ground;

        if (currentSituation == CharEnum.CommandSituation.AirHold) {
            isIgnoreHold = true;
            isIgnoreRelease = true;
        }
    }

    private void LeaveGround () {
        Log.PrintDebug ("Char : LeaveGround", LogType.Char);

        isIgnoreUserInputInThisFrame = true;

        currentLocation = LifeEnum.Location.Air;

        if (GetIsBeatingBack ()) {
            // Dominate by beat back handling
            return;
        }

        // Special Handling
        if (isJumpCharging) {         // "GroundHold - Jump" command (e.g. holding jump but then free fall)
            StopJumpCharge ();
        } else if (currentSituation == CharEnum.CommandSituation.GroundHold && isDashing) {         // "GroundHold - Dash" command
            StopDashing (CharEnum.HorizontalSpeed.Walk, true, false);
        }

        if (currentSituation == CharEnum.CommandSituation.GroundHold) {
            isIgnoreHold = true;
            isIgnoreRelease = true;
        }

        if (isJustJumpedUp) {
            isJustJumpedUp = false;
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

        if (isDropHitCharging) {     // "AirHold - Hit" command
            StopDropHitCharge ();
        } else if (isJumpCharging) {         // "AirHold - Jump" command
            StopJumpCharge ();
        }

        if (isDashing) {
            StopDashing (CharEnum.HorizontalSpeed.Walk, true, currentLocation == LifeEnum.Location.Ground);
        }

        if (currentLocation != LifeEnum.Location.Ground) {
            if (isSlippyWall) {
                currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

                RepelFromWallAndStartFreeFall (false);
            } else {
                SlideOnWall ();
            }
        }

        if (currentSituation == CharEnum.CommandSituation.GroundHold || currentSituation == CharEnum.CommandSituation.AirHold) {
            isIgnoreHold = true;
            isIgnoreRelease = true;
        }
    }

    private void LeaveWall (bool isSlippyWall) {
        Log.PrintDebug ("Char : LeaveWall : isSlippyWall = " + isSlippyWall, LogType.Char);

        isTouchingWall = false;
        isIgnoreUserInputInThisFrame = true;

        if (!isSlippyWall) {
            if (currentLocation == LifeEnum.Location.Wall) {
                ChangeMovingDirection ();
                StartFreeFall ();
            }
        }
    }

    private void SlideOnWall () {
        currentLocation = LifeEnum.Location.Wall;
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
        isAllowAirJump = true;   // Allow jump in air again

        SetAnimatorTrigger (CharAnimConstant.SlideTriggerName);
    }

    private void RepelFromWallAndStartFreeFall (bool isChangeMovingDirection) {
        if (isChangeMovingDirection) {
            ChangeMovingDirection ();
        }
        var directionMultiplier = facingDirection == LifeEnum.HorizontalDirection.Right ? -1 : 1;
        SetPosByOffset (new Vector2 (GetParams ().repelFromWallDistByTurn, 0) * directionMultiplier);

        StartFreeFall ();
    }

    private void StartFreeFall () {
        currentLocation = LifeEnum.Location.Air;

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