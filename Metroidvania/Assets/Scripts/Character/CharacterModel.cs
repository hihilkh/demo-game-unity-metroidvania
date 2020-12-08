using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;

public class CharacterModel : MonoBehaviour {
    // TODO : remove SerializeField and set const (Or think of a more beautiful way to store/change below constraints
    [SerializeField] private float WalkingSpeed = 5f;
    [SerializeField] private float SlideDownVelocity = -4f;

    // Jump Command Constraint
    private const int MaxConsecutiveJump = 2;
    private static Dictionary<CharacterEnum.JumpChargeLevel, float> JumpInitSpeedDict = new Dictionary<CharacterEnum.JumpChargeLevel, float> {
        { CharacterEnum.JumpChargeLevel.Zero, 12f },
        { CharacterEnum.JumpChargeLevel.One, 16f },
        { CharacterEnum.JumpChargeLevel.Two, 20f }
    };
    private static Dictionary<CharacterEnum.JumpChargeLevel, float> JumpChargeTimeDict = new Dictionary<CharacterEnum.JumpChargeLevel, float> {
        { CharacterEnum.JumpChargeLevel.Zero, 0f },     // Done by tap
        { CharacterEnum.JumpChargeLevel.One, 0f },      // Done by initial hold input
        { CharacterEnum.JumpChargeLevel.Two, 0.5f }     // Start counting from StartHoldAction
    };

    // Dash Command Constraint
    [SerializeField] private float DashingSpeed = 15f;
    [SerializeField] private float OneShotDashPeriod = 0.5f;
    [SerializeField] private float DashCoolDownPeriod = 0.2f;

    // Hit Command Constraint
    private static Dictionary<CharacterEnum.HitType, float> HitCoolDownPeriodDict = new Dictionary<CharacterEnum.HitType, float> {
        { CharacterEnum.HitType.Normal, 0.3f },
        { CharacterEnum.HitType.Charged, 0.5f },
        { CharacterEnum.HitType.Finishing, 0.8f },
        { CharacterEnum.HitType.Drop, 0.8f }
    };

    [SerializeField] private float DropHitVelocity = -15f;

    // Arrow Command Constraint
    private static Dictionary<CharacterEnum.ArrowType, float> ArrowCoolDownPeriodDict = new Dictionary<CharacterEnum.ArrowType, float> {
        { CharacterEnum.ArrowType.Target, 0.5f },
        { CharacterEnum.ArrowType.Straight, 0.5f },
        { CharacterEnum.ArrowType.Triple, 0.5f }
    };

    // Turn Command Constraint
    [SerializeField] private float RepelFromWallDist = 0.05f;

    // TODO : Set commandDict to empty
    private Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> situationToCommandDict = new Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> {
        { CharacterEnum.CommandSituation.GroundTap, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.GroundHold, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.GroundRelease, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.AirTap, CharacterEnum.Command.Turn },
        { CharacterEnum.CommandSituation.AirHold, CharacterEnum.Command.Dash },
        { CharacterEnum.CommandSituation.AirRelease, CharacterEnum.Command.Turn }
    };

    private Rigidbody2D rb;
    private float originalGravityScale;

    public CharacterController controller;

    private CharacterEnum.Direction facingDirection;
    private CharacterEnum.Direction movingDirection;
    private CharacterEnum.HorizontalSpeed currentHorizontalSpeed;
    private CharacterEnum.Location currentLocation;
    private bool isAllowMove;
    private int consecutiveJumpCount;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;

    // Command Control
    private CharacterEnum.CommandSituation? currentSituation;
    private CharacterEnum.Command? currentCommand;
    private bool isIgnoreHold;
    private bool isJustHitOnWall;

    // Jump Command Control
    private CharacterEnum.JumpChargeLevel currentJumpChargeLevel;
    private float currentJumpChargedTime;

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

    void Start () {
        if (controller == null) {
            controller = GetComponent<CharacterController> ();
        }

        if (controller == null) {
            Log.PrintWarning ("Player controller is not assigned and cannot be found.");
        } else {
            // Remarks :
            // Currently do not add StartedLeft, StoppedLeft, StartedRight, StoppedRight handling to prevent complicated code.
            // Add them back if found them to be useful for development or debugging.
            controller.Tapped += TriggerTapAction;
            controller.StartedHold += StartHoldAction;
            controller.StoppedHold += StopHoldAction;
        }

        InitPlayer ();
    }

    private void InitPlayer () {
        rb = GetComponent<Rigidbody2D> ();
        originalGravityScale = rb.gravityScale;

        facingDirection = CharacterEnum.Direction.Right;
        movingDirection = facingDirection;
        SetAllowMove (true);
        currentLocation = CharacterEnum.Location.Ground;
        consecutiveJumpCount = 0;

        currentJumpChargeLevel = CharacterEnum.JumpChargeLevel.Zero;
        currentJumpChargedTime = 0;

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
        isIgnoreHold = false;
        isJustHitOnWall = false;
    }

    // Remarks :
    // Currently all physics all with sharp changes, so they are stick on Update().
    // Change to stick on FixedUpdate() if continuous changes is needed.
    private void Update () {
        if (!isAllowMove) {
            return;
        }

        // Action by situation and command
        var situation = GetCurrentCommandSituation ();
        HandleCommand (situation);

        // Horizontal movement
        HorizontalMovement ();

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;
        isJustHitOnWall = false;

        if (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease) {
            isIgnoreHold = false;
        }

        // TODO : Debug usage only
        if (!(isDropHitting && isAttackCoolingDown)) { // except the case of finished drop hit and cooling down
            if (Mathf.Abs (rb.velocity.x) < 1 && Mathf.Abs (rb.velocity.y) < 1) {
                Log.PrintError ("No velocity! Situation = " + situation + " ; horizontal speed = " + currentHorizontalSpeed);
            }
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

    #region Situation and Command

    #region Situation To Command Dictionary

    public void ClearSituationToCommandDict () {
        situationToCommandDict.Clear ();
    }

    public void SetSituationToCommandDict (CharacterEnum.CommandSituation situation, CharacterEnum.Command command) {
        if (situationToCommandDict.ContainsKey (situation)) {
            situationToCommandDict[situation] = command;
        } else {
            situationToCommandDict.Add (situation, command);
        }
    }

    public void RemoveSituationToCommandDictKey (CharacterEnum.CommandSituation situation) {
        situationToCommandDict.Remove (situation);
    }

    public CharacterEnum.Command? GetCommandBySituation (CharacterEnum.CommandSituation situation) {
        if (situationToCommandDict.ContainsKey (situation)) {
            return situationToCommandDict[situation];
        } else {
            return null;
        }
    }

    #endregion

    private CharacterEnum.CommandSituation? GetCurrentCommandSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return currentLocation == CharacterEnum.Location.Ground ? CharacterEnum.CommandSituation.GroundTap : CharacterEnum.CommandSituation.AirTap;
        } else if (isHolding) {
            return currentLocation == CharacterEnum.Location.Ground ? CharacterEnum.CommandSituation.GroundHold : CharacterEnum.CommandSituation.AirHold;
        } else {
            return currentLocation == CharacterEnum.Location.Ground ? CharacterEnum.CommandSituation.GroundRelease : CharacterEnum.CommandSituation.AirRelease;
        }
    }

    private void HandleCommand (CharacterEnum.CommandSituation? optionalSituation) {
        if (optionalSituation == null) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        var situation = (CharacterEnum.CommandSituation)optionalSituation;

        if (isIgnoreHold && (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold)) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        if (isDropHitting) {
            Log.Print ("Ignore command situation due to drop hitting. situation = " + situation);
            SetCurrentCommandStatus (null, null);

            if (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold) {
                isIgnoreHold = true;
            }
            return;
        }

        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Situation : " + situation + "   Command : " + command);

        // Fisish the hold command
        if (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease) {
            if (currentSituation == CharacterEnum.CommandSituation.GroundHold || currentSituation == CharacterEnum.CommandSituation.AirHold) {
                switch (currentCommand) {
                    case CharacterEnum.Command.Dash:
                        if (command == CharacterEnum.Command.Jump || command == CharacterEnum.Command.Dash) {
                            StopDashing (currentHorizontalSpeed, false);
                        } else {
                            StopDashing (CharacterEnum.HorizontalSpeed.Walk, true);
                        }
                        break;
                }
            }
        }

        if (command == null) {
            SetCurrentCommandStatus (situation, null);
            return;
        }

        command = (CharacterEnum.Command)command;
        var isTriggeredCommand = false;

        switch (command) {
            case CharacterEnum.Command.Jump:
                if (!CheckIsAllowJump ()) {
                    break;
                }

                isTriggeredCommand = true;
                switch (situation) {
                    case CharacterEnum.CommandSituation.GroundTap:
                    case CharacterEnum.CommandSituation.AirTap:
                        Jump (CharacterEnum.JumpChargeLevel.Zero);
                        break;
                    case CharacterEnum.CommandSituation.GroundHold:
                    case CharacterEnum.CommandSituation.AirHold:
                        JumpCharge ();
                        break;
                    case CharacterEnum.CommandSituation.GroundRelease:
                    case CharacterEnum.CommandSituation.AirRelease:
                        // There are 2 cases:
                        // 1. release after JumpCharge by hold input, i.e. currentJumpChargeLevel > 0
                        // 2. release after other hold input command (e.g. dash), i.e. currentJumpChargeLevel = 0
                        Jump (currentJumpChargeLevel);
                        break;
                }
                break;
            case CharacterEnum.Command.Dash:
                if (isDashCoolingDown) {
                    break;
                }

                switch (situation) {
                    case CharacterEnum.CommandSituation.GroundTap:
                    case CharacterEnum.CommandSituation.AirTap:
                        if (!isDashing) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                    case CharacterEnum.CommandSituation.GroundHold:
                    case CharacterEnum.CommandSituation.AirHold:
                        if (currentSituation == situation) {    // Already dashing
                            if (isJustHitOnWall) {
                                isIgnoreHold = true;
                            } else {
                                isTriggeredCommand = true;
                            }
                        } else {
                            if (isDashing) {                    // Trigger hold dash while doing one tap dash
                                isIgnoreHold = true;
                            } else {
                                StartDashing (false);
                                isTriggeredCommand = true;
                            }
                        }
                        break;
                    case CharacterEnum.CommandSituation.GroundRelease:
                    case CharacterEnum.CommandSituation.AirRelease:
                        if (!isDashing) {
                            StartDashing (true);
                            isTriggeredCommand = true;
                        }
                        break;
                }
                break;
            case CharacterEnum.Command.Hit:
                if (isAttackCoolingDown) {
                    if (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold) {
                        isIgnoreHold = true;
                    }
                    break;
                }

                CharacterEnum.HitType? hitType = null;
                switch (situation) {
                    case CharacterEnum.CommandSituation.GroundTap:
                    case CharacterEnum.CommandSituation.AirTap:
                        hitType = CharacterEnum.HitType.Normal;
                        break;
                    case CharacterEnum.CommandSituation.GroundHold:
                        hitType = CharacterEnum.HitType.Charged;
                        isIgnoreHold = true;
                        break;
                    case CharacterEnum.CommandSituation.AirHold:
                        DropHitCharge ();
                        break;
                    case CharacterEnum.CommandSituation.GroundRelease:
                        hitType = CharacterEnum.HitType.Finishing;
                        break;
                    case CharacterEnum.CommandSituation.AirRelease:
                        if (currentCommand == CharacterEnum.Command.Hit) {  // That means, AirHold command is also Hit
                            hitType = CharacterEnum.HitType.Drop;
                        } else {
                            hitType = CharacterEnum.HitType.Finishing;
                        }
                        break;
                }

                if (hitType != null) {
                    Hit ((CharacterEnum.HitType)hitType);
                }
                isTriggeredCommand = true;
                
                break;
            case CharacterEnum.Command.Arrow:
                if (isAttackCoolingDown) {
                    if (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold) {
                        isIgnoreHold = true;
                    }
                    break;
                }

                CharacterEnum.ArrowType? arrowType = null;
                switch (situation) {
                    case CharacterEnum.CommandSituation.GroundTap:
                    case CharacterEnum.CommandSituation.AirTap:
                        arrowType = CharacterEnum.ArrowType.Target;
                        break;
                    case CharacterEnum.CommandSituation.GroundHold:
                    case CharacterEnum.CommandSituation.AirHold:
                        arrowType = CharacterEnum.ArrowType.Straight;
                        isIgnoreHold = true;
                        break;
                    case CharacterEnum.CommandSituation.GroundRelease:
                    case CharacterEnum.CommandSituation.AirRelease:
                        arrowType = CharacterEnum.ArrowType.Triple;
                        break;
                }

                if (arrowType != null) {
                    ShootArrow ((CharacterEnum.ArrowType)arrowType);
                }
                isTriggeredCommand = true;

                break;
            case CharacterEnum.Command.Turn:
                switch (situation) {
                    case CharacterEnum.CommandSituation.GroundTap:
                    case CharacterEnum.CommandSituation.GroundRelease:
                        ChangeFacingDirection (true);
                        isTriggeredCommand = true;
                        break;
                    case CharacterEnum.CommandSituation.AirTap:
                    case CharacterEnum.CommandSituation.AirRelease:
                        if (currentLocation == CharacterEnum.Location.Wall) {
                            ChangeFacingDirection (true);
                            var directionMultiplier = facingDirection == CharacterEnum.Direction.Right ? -1 : 1;
                            transform.position = transform.position + new Vector3 (RepelFromWallDist, 0, 0) * directionMultiplier;

                            ReleaseFromWallSliding ();
                        } else {
                            ChangeFacingDirection (false);
                        }
                        isTriggeredCommand = true;
                        break;
                    case CharacterEnum.CommandSituation.GroundHold:
                    case CharacterEnum.CommandSituation.AirHold:
                        Log.PrintWarning ("No action of Turn command is defined for holding. Please check.");
                        isIgnoreHold = true;
                        break;
                }
                
                break;
        }

        SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);
    }

    private void SetCurrentCommandStatus (CharacterEnum.CommandSituation? situation, CharacterEnum.Command? command) {
        currentSituation = situation;
        currentCommand = command;
    }

    #endregion

    #region Horizontal movement

    private void HorizontalMovement () {
        if (!isAllowMove) {
            return;
        }

        var directionMultiplier = movingDirection == CharacterEnum.Direction.Right ? 1 : -1;
        var horizontalSpeed = 0f;

        switch (currentHorizontalSpeed) {
            case CharacterEnum.HorizontalSpeed.Zero:
                horizontalSpeed = 0;
                break;
            case CharacterEnum.HorizontalSpeed.Walk:
                horizontalSpeed = WalkingSpeed;
                break;
            case CharacterEnum.HorizontalSpeed.Dash:
                horizontalSpeed = DashingSpeed;
                break;
        }

        rb.velocity = new Vector3 (horizontalSpeed * directionMultiplier, rb.velocity.y);

        // TODO : Think of idle/walk animation
    }

    private void StartIdling () {
        rb.velocity = Vector2.zero; // Need to set velocity here because if set isAllowMove = false, HorizontalMovement() logic will bypass
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Zero;

        // TODO : Idle animation
    }

    private void StartWalking () {
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;

        // TODO : Walk animation
    }

    #endregion

    #region Dash

    private void StartDashing (bool isOneShot) {
        Log.Print ("Dash : isOneShot = " + isOneShot);

        StopDashing (currentHorizontalSpeed, false);  // To ensure do not trigger 2 dash coroutines at the same time

        if (currentLocation == CharacterEnum.Location.Wall) {
            currentLocation = CharacterEnum.Location.Air;
        }

        if (isOneShot) {
            dashCoroutine = StartCoroutine (OneShotDashCoroutine ());
        } else {
            SetDashing ();
        }
    }

    private void StopDashing (CharacterEnum.HorizontalSpeed speedAfterStopDashing, bool isNeedDashCoolDown) {
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
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Dash;

        // TODO : Dash animation
    }

    private IEnumerator OneShotDashCoroutine () {
        SetDashing ();

        var startTime = Time.time;

        while (Time.time - startTime < OneShotDashPeriod) {
            if (isJustHitOnWall) {
                break;
            }

            yield return null;
        }

        StopDashing (CharacterEnum.HorizontalSpeed.Walk, true);
    }

    private IEnumerator DashCoolDownCoroutine () {
        isDashCoolingDown = true;

        yield return new WaitForSeconds (DashCoolDownPeriod);

        isDashCoolingDown = false;
        dashCoolDownCoroutine = null;
    }

    #endregion

    #region Jump

    private bool CheckIsAllowJump () {
        return consecutiveJumpCount < MaxConsecutiveJump;
    }

    private void Jump (CharacterEnum.JumpChargeLevel level) {
        Log.Print ("Jump : JumpChargeLevel = " + level);

        if (JumpInitSpeedDict.ContainsKey(level)) {
            StopDashing (currentHorizontalSpeed, false);
            var jumpInitSpeed = JumpInitSpeedDict[level];

            rb.velocity = new Vector3 (rb.velocity.x, jumpInitSpeed);
            if (currentLocation == CharacterEnum.Location.Wall) {
                currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;
                rb.gravityScale = originalGravityScale;
            }

            consecutiveJumpCount++;
            currentLocation = CharacterEnum.Location.Air;
        } else {
            Log.PrintError ("Jump Init Speed is missing. Cannot perform jump action. Please check. Level : " + level);
        }

        ResetJumpCharge ();

        // TODO : Jump animation
    }

    private void JumpCharge () {
        switch (currentJumpChargeLevel) {
            case CharacterEnum.JumpChargeLevel.Zero:
                currentJumpChargeLevel = CharacterEnum.JumpChargeLevel.One;
                currentJumpChargedTime = 0;
                Log.Print ("Jump Charge Level -> " + currentJumpChargeLevel);
                break;
            default:
                currentJumpChargedTime += Time.deltaTime;
                var nextLevel = (CharacterEnum.JumpChargeLevel)((int)currentJumpChargeLevel + 1);
                if (Enum.IsDefined (typeof (CharacterEnum.JumpChargeLevel), nextLevel)) {
                    if (JumpChargeTimeDict.ContainsKey (nextLevel)) {
                        if (currentJumpChargedTime >= JumpChargeTimeDict[nextLevel]) {
                            currentJumpChargeLevel = nextLevel;
                            Log.Print ("Jump Charge Level -> " + currentJumpChargeLevel);
                        }
                    } else {
                        Log.PrintWarning ("Jump Charge Time is missing. Cannot charge to level : " + nextLevel);
                    }
                }
                break;
        }

        // TODO : Jump charge animation
    }

    private void ResetJumpCharge () {
        currentJumpChargeLevel = CharacterEnum.JumpChargeLevel.Zero;
        currentJumpChargedTime = 0;
    }

    #endregion

    #region Hit

    private void Hit (CharacterEnum.HitType hitType) {
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
            case CharacterEnum.HitType.Normal:
            case CharacterEnum.HitType.Charged:
            case CharacterEnum.HitType.Finishing:
                Log.PrintWarning ("Hit!!!    " + hitType);
                // TODO : Implementation of actual hit
                attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (hitType));
                break;
            case CharacterEnum.HitType.Drop:
                DropHit ();
                break;
        }
    }

    private void DropHit () {
        Log.PrintWarning ("Hit!!!    Drop");
        isDropHitting = true;

        isDropHitCharging = false;
        // TODO : remove drop hit charge animation

        // TODO : Implementation of actual hit

        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Zero;
        rb.gravityScale = 0;
        rb.velocity = new Vector3 (0, DropHitVelocity);
    }

    private void FinishDropHit () {
        Log.Print ("FinishDropHit");
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Zero;
        rb.gravityScale = originalGravityScale;
        rb.velocity = new Vector3 (0, 0);

        // TODO : Drop Hit Landing animation

        attackCoolDownCoroutine = StartCoroutine (HitCoolDownCoroutine (CharacterEnum.HitType.Drop));
    }

    private void DropHitCharge () {
        if (!isDropHitCharging) {
            isDropHitCharging = true;
            // TODO : drop hit charge animation
        }
    }

    private IEnumerator HitCoolDownCoroutine (CharacterEnum.HitType hitType) {
        isAttackCoolingDown = true;

        var hitCoolDownPeriod = 0f;

        if (HitCoolDownPeriodDict.ContainsKey(hitType)) {
            hitCoolDownPeriod = HitCoolDownPeriodDict[hitType];
        } else {
            Log.PrintWarning ("Not yet set hit cool down period for HitType : " + hitType + " . Assume cool down period to be 0s");
        }

        yield return new WaitForSeconds (hitCoolDownPeriod);

        isAttackCoolingDown = false;

        if (hitType == CharacterEnum.HitType.Drop) {
            isDropHitting = false;
            currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;
        }

        attackCoolDownCoroutine = null;
    }

    #endregion

    #region Arrow

    private void ShootArrow (CharacterEnum.ArrowType arrowType) {
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
            case CharacterEnum.ArrowType.Target:
            case CharacterEnum.ArrowType.Straight:
            case CharacterEnum.ArrowType.Triple:
                Log.PrintWarning ("Arrow!!!    " + arrowType);
                // TODO : Implementation of actual arrow shooting
                attackCoolDownCoroutine = StartCoroutine (ArrowCoolDownCoroutine (arrowType));
                break;
        }
    }

    private IEnumerator ArrowCoolDownCoroutine (CharacterEnum.ArrowType arrowType) {
        isAttackCoolingDown = true;

        var arrowCoolDownPeriod = 0f;

        if (ArrowCoolDownPeriodDict.ContainsKey (arrowType)) {
            arrowCoolDownPeriod = ArrowCoolDownPeriodDict[arrowType];
        } else {
            Log.PrintWarning ("Not yet set arrow cool down period for ArrowType : " + arrowType + " . Assume cool down period to be 0s");
        }

        yield return new WaitForSeconds (arrowCoolDownPeriod);

        isAttackCoolingDown = false;

        attackCoolDownCoroutine = null;
    }
    #endregion

    #region Change Direction

    private void ChangeFacingDirection (bool isAlignMovingDirection) {
        Log.PrintDebug ("ChangeFacingDirection : isAlignMovingDirection = " + isAlignMovingDirection);
        if (facingDirection == CharacterEnum.Direction.Left) {
            facingDirection = CharacterEnum.Direction.Right;
        } else {
            facingDirection = CharacterEnum.Direction.Left;
        }

        if (isAlignMovingDirection) {
            movingDirection = facingDirection;
        }
    }

    private void ChangeMovingDirection () {
        // Remarks : Changing moving direction must also align facing direction

        Log.PrintDebug ("ChangeMovingDirection");
        if (movingDirection == CharacterEnum.Direction.Left) {
            movingDirection = CharacterEnum.Direction.Right;
        } else {
            movingDirection = CharacterEnum.Direction.Left;
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

        Log.Print ("Char Collision : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType);

        switch (collideType) {
            case GameVariable.GroundTag:
                LandToGround ();
                break;
            case GameVariable.WallTag:
                HitOnWall ();
                break;
        }
    }

    private void LandToGround () {
        Log.PrintDebug ("LandToGround");
        consecutiveJumpCount = 0;
        currentLocation = CharacterEnum.Location.Ground;
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;

        rb.gravityScale = originalGravityScale;
        rb.velocity = new Vector3 (rb.velocity.x, 0);

        AlignMovingWithFacingDirection ();

        // Special Handling
        if (currentJumpChargeLevel != CharacterEnum.JumpChargeLevel.Zero) {     // "AirHold - Jump" command
            Log.Print ("LandToGround : Reset jump charge and ignore hold/release.");
            isIgnoreHold = true;

            // TODO : Remove charge animation
        } else if (isDropHitCharging) {                                         // "AirHold - Hit" command
            Log.Print ("LandToGround : Reset hit charge and ignore hold/release.");
            isDropHitCharging = false;
            isIgnoreHold = true;

            // TODO : Remove drop hit charge animation
        } else if (isDropHitting) {                                             // "AirRelease - Hit" command
            FinishDropHit ();
        }

        // Always reset jump charge to prevent the case that user started to hold the key while in air
        // but he land to ground so fast that even not yet come to JumpChargeLevel One
        ResetJumpCharge ();

        // TODO : Landing animation
    }

    private void HitOnWall () {
        isJustHitOnWall = true;
        ChangeMovingDirection ();

        if (isDashing) {
            StopDashing (CharacterEnum.HorizontalSpeed.Walk, true);
        }

        if (currentLocation == CharacterEnum.Location.Air) {
            SlideOnWall ();
        }
    }

    private void SlideOnWall () {
        currentLocation = CharacterEnum.Location.Wall;
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Zero;
        consecutiveJumpCount = 1;   // Allow jump in air again

        // Slide down with constant speed
        rb.gravityScale = 0;
        rb.velocity = new Vector3 (0, SlideDownVelocity);

        // TODO : Sliding animation
    }

    private void ReleaseFromWallSliding () {
        currentLocation = CharacterEnum.Location.Air;

        rb.gravityScale = originalGravityScale;
        rb.velocity = new Vector3 (0, 0);

        // TODO : Release from wall sliding animation
    }

    #endregion

    #region Event Handler

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