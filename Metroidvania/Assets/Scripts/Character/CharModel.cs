using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;

public class CharModel : MonoBehaviour {
    // TODO : Set commandDict to empty
    private Dictionary<CharEnum.CommandSituation, CharEnum.Command> situationToCommandDict = new Dictionary<CharEnum.CommandSituation, CharEnum.Command> {
        { CharEnum.CommandSituation.GroundTap, CharEnum.Command.Jump },
        { CharEnum.CommandSituation.GroundHold, CharEnum.Command.Jump },
        { CharEnum.CommandSituation.GroundRelease, CharEnum.Command.Jump },
        { CharEnum.CommandSituation.AirTap, CharEnum.Command.Jump },
        { CharEnum.CommandSituation.AirHold, CharEnum.Command.Jump },
        { CharEnum.CommandSituation.AirRelease, CharEnum.Command.Jump }
    };

    [SerializeField] private CharController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private CharParams _characterParams;
    public CharParams characterParams { get => _characterParams; }

    public Rigidbody2D rb { get; private set; }
    public float originalGravityScale { get; private set; }

    // Body Parts
    public CharEnum.BodyPart obtainedBodyParts { get; private set; } = CharEnum.BodyPart.Head | CharEnum.BodyPart.Arms | CharEnum.BodyPart.Legs | CharEnum.BodyPart.Thrusters | CharEnum.BodyPart.Arrow;
    public event Action<CharEnum.BodyPart> obtainedBodyPartsChangedEvent;

    // Character Situation
    public CharEnum.Direction facingDirection { get; private set; }
    public CharEnum.Direction movingDirection { get; private set; }
    public CharEnum.HorizontalSpeed currentHorizontalSpeed { get; private set; }
    private CharEnum.Location currentLocation;
    private bool isAllowMove;
    private bool isAllowAirJump;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;

    // Command Control
    private CharEnum.CommandSituation? currentSituation;
    private CharEnum.Command? currentCommand;
    private CharEnum.Location startedPressLocation;
    private bool isIgnoreHold;
    private bool isIgnoreRelease;
    private bool isJustHitOnWall;

    // Jump Command Control
    public bool isJumpCharged { get; private set; }

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
    private Dictionary<Collider2D, string> currentCollisionDict = new Dictionary<Collider2D, string> ();

    void Start () {
        if (controller == null) {
            controller = GetComponent<CharController> ();
        }

        if (controller == null) {
            Log.PrintWarning ("Player controller is not assigned and cannot be found.");
        } else {
            // Remarks :
            // Currently do not add StartedLeft, StoppedLeft, StartedRight, StoppedRight handling to prevent complicated code.
            // Add them back if found them to be useful for development or debugging.
            controller.StartedPressEvent += TriggerStartPressAction;
            controller.TappedEvent += TriggerTapAction;
            controller.StartedHoldEvent += StartHoldAction;
            controller.StoppedHoldEvent += StopHoldAction;
        }

        InitPlayer ();
    }

    private void InitPlayer () {
        rb = GetComponent<Rigidbody2D> ();
        originalGravityScale = rb.gravityScale;

        facingDirection = CharEnum.Direction.Right;
        movingDirection = facingDirection;
        SetAllowMove (true);
        currentLocation = CharEnum.Location.Ground;
        isAllowAirJump = true;

        isJumpCharged = false;

        isDashing = false;
        isDashCoolingDown = false;
        dashCoroutine = null;

        isAttackCoolingDown = false;
        isDropHitCharging = false;
        isDropHitting = false;
        attackCoolDownCoroutine = null;

        ResetAllUpdateControlFlags ();
    }

    private void ResetAllUpdateControlFlags () {
        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;

        currentSituation = null;
        currentCommand = null;
        startedPressLocation = CharEnum.Location.Ground;
        isIgnoreHold = false;
        isIgnoreRelease = false;
        isJustHitOnWall = false;
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

        // Horizontal movement
        //HorizontalMovement ();

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;
        isJustHitOnWall = false;

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
            return null;
        }
    }

    #endregion

    private CharEnum.CommandSituation? GetCurrentCommandSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return currentLocation == CharEnum.Location.Ground ? CharEnum.CommandSituation.GroundTap : CharEnum.CommandSituation.AirTap;
        } else if (isHolding) {
            return currentLocation == CharEnum.Location.Ground ? CharEnum.CommandSituation.GroundHold : CharEnum.CommandSituation.AirHold;
        } else {
            return currentLocation == CharEnum.Location.Ground ? CharEnum.CommandSituation.GroundRelease : CharEnum.CommandSituation.AirRelease;
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
                // (The situation that it is just triggered hold)
                // Hold and release would fail if started to press in air but it is on the ground while triggered hold, or vice versa
                var isHoldFailed = false;
                if (situation == CharEnum.CommandSituation.GroundHold && startedPressLocation != CharEnum.Location.Ground) {
                    isHoldFailed = true;
                } else if (situation == CharEnum.CommandSituation.AirHold && startedPressLocation == CharEnum.Location.Ground) {
                    isHoldFailed = true;
                }

                if (isHoldFailed) {
                    isIgnoreHold = true;
                    isIgnoreRelease = true;
                    SetCurrentCommandStatus (null, null);
                    return;
                }
            }
        }

        if (situation == CharEnum.CommandSituation.GroundRelease || situation == CharEnum.CommandSituation.AirRelease) {
            if (isIgnoreRelease) {
                SetCurrentCommandStatus (null, null);
                return;
            }
        }

        if (isDropHitting) {
            Log.Print ("Ignore command situation due to drop hitting. situation = " + situation);
            SetCurrentCommandStatus (null, null);

            if (situation == CharEnum.CommandSituation.GroundHold || situation == CharEnum.CommandSituation.AirHold) {
                isIgnoreHold = true;
            }
            return;
        }

        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Situation : " + situation + "   Command : " + command);

        // Fisish the hold command
        if (situation == CharEnum.CommandSituation.GroundRelease || situation == CharEnum.CommandSituation.AirRelease) {
            if (currentSituation == CharEnum.CommandSituation.GroundHold || currentSituation == CharEnum.CommandSituation.AirHold) {
                switch (currentCommand) {
                    case CharEnum.Command.Dash:
                        if (command == CharEnum.Command.Jump || command == CharEnum.Command.Dash) {
                            StopDashing (currentHorizontalSpeed, false);
                        } else {
                            StopDashing (CharEnum.HorizontalSpeed.Walk, true);
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
                            if (isJumpCharged) {
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
                            if (!isJustHitOnWall) {
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
                switch (situation) {
                    case CharEnum.CommandSituation.GroundTap:
                    case CharEnum.CommandSituation.GroundRelease:
                        ChangeFacingDirection (true);
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.CommandSituation.AirTap:
                    case CharEnum.CommandSituation.AirRelease:
                        if (currentLocation == CharEnum.Location.Wall) {
                            ChangeFacingDirection (true);
                            var directionMultiplier = facingDirection == CharEnum.Direction.Right ? -1 : 1;
                            transform.position = transform.position + new Vector3 (characterParams.repelFromWallDistByTurn, 0, 0) * directionMultiplier;

                            ReleaseFromWallSliding ();
                        } else {
                            ChangeFacingDirection (false);
                        }
                        isTriggeredCommand = true;
                        break;
                    case CharEnum.CommandSituation.GroundHold:
                    case CharEnum.CommandSituation.AirHold:
                        Log.PrintWarning ("No action of Turn command is defined for holding. Please check.");
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
    }

    #endregion

    #region Horizontal movement

    private void HorizontalMovement () {
        if (!isAllowMove) {
            return;
        }

        var directionMultiplier = movingDirection == CharEnum.Direction.Right ? 1 : -1;
        var horizontalSpeed = 0f;

        switch (currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                horizontalSpeed = 0;
                break;
            case CharEnum.HorizontalSpeed.Walk:
                horizontalSpeed = characterParams.walkingSpeed;
                break;
            case CharEnum.HorizontalSpeed.Dash:
                horizontalSpeed = characterParams.dashingSpeed;
                break;
        }

        rb.velocity = new Vector3 (horizontalSpeed * directionMultiplier, rb.velocity.y);

        // TODO : Think of idle/walk animation
    }

    public void StartIdleOrWalk () {
        switch (currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Idle:
                StartIdling ();
                break;
            case CharEnum.HorizontalSpeed.Walk:
                StartWalking ();
                break;
            default:
                Log.PrintWarning ("Current horizontal speed is : " + currentHorizontalSpeed + " . It does not match idle or walk. Force to change to walk. Please check.");
                StartWalking ();
                break;
        }
    }

    private void StartIdling () {
        // TODO : Check and remove below
        //rb.velocity = Vector2.zero; // Need to set velocity here because if set isAllowMove = false, HorizontalMovement() logic will bypass
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        animator.SetTrigger (CharAnimConstant.IdleTriggerName);
    }

    private void StartWalking () {
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;

        animator.SetTrigger (CharAnimConstant.WalkTriggerName);
    }

    #endregion

    #region Dash

    private void StartDashing (bool isOneShot) {
        Log.Print ("Dash : isOneShot = " + isOneShot);

        StopDashing (currentHorizontalSpeed, false);  // To ensure do not trigger 2 dash coroutines at the same time

        if (currentLocation == CharEnum.Location.Wall) {
            currentLocation = CharEnum.Location.Air;
        }

        if (isOneShot) {
            dashCoroutine = StartCoroutine (OneShotDashCoroutine ());
        } else {
            SetDashing ();
        }
    }

    private void StopDashing (CharEnum.HorizontalSpeed speedAfterStopDashing, bool isNeedDashCoolDown) {
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
        rb.gravityScale = originalGravityScale;
        currentHorizontalSpeed = speedAfterStopDashing;

        // TODO : Walk / fall down animation

        if (isNeedDashCoolDown) {
            if (dashCoolDownCoroutine != null) {
                StopCoroutine (dashCoolDownCoroutine);
            }
            dashCoolDownCoroutine = StartCoroutine (DashCoolDownCoroutine ());
        }
    }

    private void SetDashing () {
        isDashing = true;
        rb.gravityScale = 0;  // prevent fall down behaviour from gravity
        rb.velocity = new Vector3 (rb.velocity.x, 0);
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Dash;

        // TODO : Dash animation
    }

    private IEnumerator OneShotDashCoroutine () {
        SetDashing ();

        var startTime = Time.time;

        while (Time.time - startTime < characterParams.oneShotDashPeriod) {
            if (isJustHitOnWall) {
                break;
            }

            yield return null;
        }

        StopDashing (CharEnum.HorizontalSpeed.Walk, true);
    }

    private IEnumerator DashCoolDownCoroutine () {
        isDashCoolingDown = true;

        yield return new WaitForSeconds (characterParams.dashCoolDownPeriod);

        isDashCoolingDown = false;
        dashCoolDownCoroutine = null;
    }

    #endregion

    #region Jump

    private bool CheckIsAllowJump () {
        if (currentLocation == CharEnum.Location.Ground) {
            return true;
        }

        return isAllowAirJump;
    }

    private IEnumerator Jump () {
        Log.Print ("Jump : isCharged = " + isJumpCharged);

        StopDashing (currentHorizontalSpeed, false);

        if (currentLocation == CharEnum.Location.Wall) {
            currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
        }

        if (currentLocation != CharEnum.Location.Ground) {
            isAllowAirJump = false;
        }
        currentLocation = CharEnum.Location.Air;

        animator.SetTrigger (CharAnimConstant.JumpTriggerName);

        // Remarks : WaitForEndOfFrame in order to let CharJumpSMB know the jump is charged
        yield return new WaitForEndOfFrame ();
        StopJumpCharge ();
    }

    private void JumpCharge () {
        if (!isJumpCharged) {
            isJumpCharged = true;
            isIgnoreHold = true;

            // TODO : Jump charge animation
        }
    }

    private void StopJumpCharge () {
        if (isJumpCharged) {
            isJumpCharged = false;

            // TODO : Cancel Jump charge animation
        }
    }

    #endregion

    #region Hit

    private void Hit (CharEnum.HitType hitType) {
        if (isAttackCoolingDown) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger Hit action. Please check.");
            return;
        }

        if (isDropHitting) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger Hit action. Please check.");
            return;
        }

        Log.Print ("Hit : HitType = " + hitType);

        switch (hitType) {
            case CharEnum.HitType.Normal:
            case CharEnum.HitType.Charged:
            case CharEnum.HitType.Finishing:
                Log.PrintWarning ("Hit!!!    " + hitType);
                // TODO : Implementation of actual hit
                attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (hitType));
                break;
            case CharEnum.HitType.Drop:
                DropHit ();
                break;
        }
    }

    private void DropHit () {
        Log.Print ("Start DropHit");

        StopDropHitCharge ();
        isDropHitting = true;

        // TODO : Implementation of actual hit

        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;

        animator.SetTrigger (CharAnimConstant.DropHitTriggerName);
    }

    private void FinishDropHit () {
        Log.Print ("Finish DropHit");

        isDropHitting = false;
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;

        attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (CharEnum.HitType.Drop));
    }

    private void DropHitCharge () {
        if (!isDropHitCharging) {
            isDropHitCharging = true;
            // TODO : drop hit charge animation
        }
    }

    private void StopDropHitCharge () {
        if (isDropHitCharging) {
            isDropHitCharging = false;

            // TODO : cancel drop hit charge animation
        }
    }

    private IEnumerator HitCoolDownCoroutine (CharEnum.HitType hitType) {
        isAttackCoolingDown = true;

        var hitCoolDownPeriod = 0f;

        switch (hitType) {
            case CharEnum.HitType.Normal:
                hitCoolDownPeriod = characterParams.hitCoolDownPeriod_Normal;
                break;
            case CharEnum.HitType.Charged:
                hitCoolDownPeriod = characterParams.hitCoolDownPeriod_Charged;
                break;
            case CharEnum.HitType.Finishing:
                hitCoolDownPeriod = characterParams.hitCoolDownPeriod_Finishing;
                break;
            case CharEnum.HitType.Drop:
                hitCoolDownPeriod = characterParams.hitCoolDownPeriod_Drop;
                break;
            default:
                Log.PrintWarning ("Not yet set hit cool down period for HitType : " + hitType + " . Assume cool down period to be 0s");
                break;
        }

        yield return new WaitForSeconds (hitCoolDownPeriod);

        isAttackCoolingDown = false;
        attackCoolDownCoroutine = null;
    }

    #endregion

    #region Arrow

    private void ShootArrow (CharEnum.ArrowType arrowType) {
        if (isAttackCoolingDown) {
            Log.PrintWarning ("isAttackCoolingDown = true. It should not trigger shoot arrow action. Please check.");
            return;
        }

        if (isDropHitting) {
            Log.PrintWarning ("isDropHitting = true. It should not trigger shoot arrow action. Please check.");
            return;
        }

        Log.Print ("Shoot arrow : ArrowType = " + arrowType);

        switch (arrowType) {
            case CharEnum.ArrowType.Target:
            case CharEnum.ArrowType.Straight:
            case CharEnum.ArrowType.Triple:
                Log.PrintWarning ("Arrow!!!    " + arrowType);
                // TODO : Implementation of actual arrow shooting
                attackCoolDownCoroutine = StartCoroutine (ArrowCoolDownCoroutine (arrowType));
                break;
        }
    }

    private IEnumerator ArrowCoolDownCoroutine (CharEnum.ArrowType arrowType) {
        isAttackCoolingDown = true;

        var arrowCoolDownPeriod = 0f;

        switch (arrowType) {
            case CharEnum.ArrowType.Target:
                arrowCoolDownPeriod = characterParams.arrowCoolDownPeriod_Target;
                break;
            case CharEnum.ArrowType.Straight:
                arrowCoolDownPeriod = characterParams.arrowCoolDownPeriod_Straight;
                break;
            case CharEnum.ArrowType.Triple:
                arrowCoolDownPeriod = characterParams.arrowCoolDownPeriod_Triple;
                break;
            default:
                Log.PrintWarning ("Not yet set arrow cool down period for ArrowType : " + arrowType + " . Assume cool down period to be 0s");
                break;
        }

        yield return new WaitForSeconds (arrowCoolDownPeriod);

        isAttackCoolingDown = false;

        attackCoolDownCoroutine = null;
    }
    #endregion

    #region Change Direction

    private void ChangeFacingDirection (bool isAlignMovingDirection) {
        Log.PrintDebug ("ChangeFacingDirection : isAlignMovingDirection = " + isAlignMovingDirection);
        if (facingDirection == CharEnum.Direction.Left) {
            facingDirection = CharEnum.Direction.Right;
        } else {
            facingDirection = CharEnum.Direction.Left;
        }

        if (isAlignMovingDirection) {
            movingDirection = facingDirection;
        }
    }

    private void ChangeMovingDirection () {
        // Remarks : Changing moving direction must also align facing direction

        Log.PrintDebug ("ChangeMovingDirection");
        if (movingDirection == CharEnum.Direction.Left) {
            movingDirection = CharEnum.Direction.Right;
        } else {
            movingDirection = CharEnum.Direction.Left;
        }

        facingDirection = movingDirection;
    }

    private void AlignMovingWithFacingDirection () {
        movingDirection = facingDirection;
    }

    #endregion

    #region Collision

    public void OnCollisionEnter2D (Collision2D collision) {
        var collideType = collision.gameObject.tag;

        if (collision.gameObject.tag == GameVariable.WallTag) {
            var collisionNormal = collision.GetContact(0).normal;
            var absX = Mathf.Abs (collisionNormal.x);
            if (collisionNormal.y > 0 && collisionNormal.y > absX) {
                collideType = GameVariable.GroundTag;
            } else if (collisionNormal.y < 0 && -collisionNormal.y > absX) {
                collideType = GameVariable.GroundTag;
            }
        }

        Log.Print ("Char Collision Enter : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType);

        if (currentCollisionDict.ContainsKey (collision.collider)) {
            currentCollisionDict[collision.collider] = collideType;
        } else {
            currentCollisionDict.Add (collision.collider, collideType);
        }

        if (Time.time == 0) {
            // Do not do collide action for time = 0
            return;
        }

        switch (collideType) {
            case GameVariable.GroundTag:
                TouchGround ();
                break;
            case GameVariable.WallTag:
                TouchWall ();
                break;
        }
    }

    public void OnCollisionExit2D (Collision2D collision) {
        if (!currentCollisionDict.ContainsKey (collision.collider)) {
            Log.PrintError ("Missing key in currentCollisionDict. collision name : " + collision.gameObject.name);
            return;
        }
        var collideType = currentCollisionDict[collision.collider];
        currentCollisionDict.Remove (collision.collider);

        Log.Print ("Char Collision Exit : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType);

        switch (collideType) {
            case GameVariable.GroundTag:
                LeaveGround ();
                break;
        }
    }

    private void TouchGround () {
        Log.PrintDebug ("TouchGround");

        if (isDashing) {
            if (currentSituation == CharEnum.CommandSituation.AirHold) {         // "AirHold - Dash" command
                StopDashing (CharEnum.HorizontalSpeed.Walk, true);
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
                } else if (isJumpCharged) {         // "AirHold - Jump" command
                    StopJumpCharge ();
                }
                currentHorizontalSpeed = CharEnum.HorizontalSpeed.Walk;
                AlignMovingWithFacingDirection ();
            }

            if (currentLocation == CharEnum.Location.Wall) {
                StartWalking ();
            } else {
                animator.SetTrigger (CharAnimConstant.LandingTriggerName);
            }
        }

        isAllowAirJump = true;
        currentLocation = CharEnum.Location.Ground;

        if (currentSituation == CharEnum.CommandSituation.AirHold) {
            isIgnoreHold = true;
            isIgnoreRelease = true;
        }
    }

    private void LeaveGround () {
        Log.PrintDebug ("LeaveGround");
        currentLocation = CharEnum.Location.Air;

        // Special Handling
        if (isJumpCharged) {         // "GroundHold - Jump" command
            StopJumpCharge ();
        } else if (currentSituation == CharEnum.CommandSituation.GroundHold && isDashing) {         // "GroundHold - Dash" command
            StopDashing (CharEnum.HorizontalSpeed.Walk, true);
        }

        if (currentSituation == CharEnum.CommandSituation.GroundHold) {
            isIgnoreHold = true;
            isIgnoreRelease = true;
        }

        // TODO : Leave ground animation
    }

    private void TouchWall () {
        isJustHitOnWall = true;
        ChangeMovingDirection ();

        if (isDashing) {
            StopDashing (CharEnum.HorizontalSpeed.Walk, true);
        }

        if (currentLocation == CharEnum.Location.Air) {
            SlideOnWall ();
        }
    }

    private void SlideOnWall () {
        currentLocation = CharEnum.Location.Wall;
        currentHorizontalSpeed = CharEnum.HorizontalSpeed.Idle;
        isAllowAirJump = true;   // Allow jump in air again

        // Slide down with constant speed
        rb.gravityScale = 0;
        rb.velocity = new Vector3 (0, characterParams.slideDownVelocity);

        // TODO : Sliding animation
    }

    private void ReleaseFromWallSliding () {
        currentLocation = CharEnum.Location.Air;

        rb.gravityScale = originalGravityScale;
        rb.velocity = new Vector3 (0, 0);

        // TODO : Release from wall sliding animation
    }

    #endregion

    #region Event Handler

    private void TriggerStartPressAction () {
        startedPressLocation = currentLocation;

        Log.PrintDebug ("TriggerStartPressAction");
    }

    private void TriggerTapAction () {
        if (isHolding) {
            Log.PrintWarning ("Somehow triggered tap while holding. Do not do tap action.");
            return;
        }

        Log.PrintDebug ("TriggerTapAction");
        isJustTapped = true;
    }

    private void StartHoldAction () {
        if (isJustTapped) {
            Log.PrintWarning ("Somehow triggered hold while just tapped. Do not do hold action.");
            return;
        }

        Log.PrintDebug ("StartHoldAction");
        isHolding = true;
    }

    private void StopHoldAction () {
        Log.PrintDebug ("StopHoldAction");
        isHolding = false;
        isJustReleaseHold = true;
    }

    #endregion
}