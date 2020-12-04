using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;

public class CharacterModel : MonoBehaviour {
    // TODO : remove SerializeField and set const
    [SerializeField] private float WalkingSpeed = 5f;
    
    // Jump Command Constraint
    private const int MaxConsecutiveJump = 2;
    private static Dictionary<CharacterEnum.JumpChargeLevel, float> JumpInitSpeedDict = new Dictionary<CharacterEnum.JumpChargeLevel, float> {
        { CharacterEnum.JumpChargeLevel.Zero, 8f },
        { CharacterEnum.JumpChargeLevel.One, 12f },
        { CharacterEnum.JumpChargeLevel.Two, 16f }
    };
    private static Dictionary<CharacterEnum.JumpChargeLevel, float> JumpChargeTimeDict = new Dictionary<CharacterEnum.JumpChargeLevel, float> {
        { CharacterEnum.JumpChargeLevel.Zero, 0f },     // Done by tap
        { CharacterEnum.JumpChargeLevel.One, 0f },      // Done by initial hold input
        { CharacterEnum.JumpChargeLevel.Two, 0.5f }     // Start counting from StartHoldAction
    };

    // Dash Command Constraint
    [SerializeField] private float DashingSpeed = 15f;
    [SerializeField] private float OneShotDashPeriod = 0.5f;
    [SerializeField] private float AfterDashCoolDownPeriod = 0.2f;

    // TODO : Set commandDict to empty
    private Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> situationToCommandDict = new Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> {
        { CharacterEnum.CommandSituation.GroundTap, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.GroundHold, CharacterEnum.Command.Dash },
        { CharacterEnum.CommandSituation.GroundRelease, CharacterEnum.Command.Dash },
        { CharacterEnum.CommandSituation.AirTap, CharacterEnum.Command.Dash },
        { CharacterEnum.CommandSituation.AirHold, CharacterEnum.Command.Dash },
        { CharacterEnum.CommandSituation.AirRelease, CharacterEnum.Command.Jump }
    };

    private Rigidbody2D rb;
    private float originalGravityScale;

    public CharacterController controller;

    private CharacterEnum.Direction facingDirection;
    private CharacterEnum.HorizontalSpeed currentHorizontalSpeed;
    private CharacterEnum.Animation charAnim;
    private bool isAllowMove;
    private bool isInAir;
    private int consecutiveJumpCount;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;

    // Command Control
    private CharacterEnum.CommandSituation? currentSituation;
    private CharacterEnum.Command? currentCommand;
    private bool isIgnoreHold;
    private bool isIgnoreRelease;
    private bool isJustChangedDirection;

    // Jump Command Control
    private CharacterEnum.JumpChargeLevel currentJumpChargeLevel;
    private float currentJumpChargedTime;

    // Dash Command Control
    private bool isDashing;
    private bool isDashCoolingDown;
    private Coroutine dashCoroutine;
    private Coroutine dashCoolDownCoroutine;

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
        SetAllowMove (true);
        isInAir = false;
        consecutiveJumpCount = 0;

        currentJumpChargeLevel = CharacterEnum.JumpChargeLevel.Zero;
        currentJumpChargedTime = 0;

        isDashing = false;
        isDashCoolingDown = false;
        dashCoroutine = null;

        ResetAllUpdateControlFlags ();
    }

    private void ResetAllUpdateControlFlags () {
        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;

        currentSituation = null;
        currentCommand = null;
        isIgnoreHold = false;
        isIgnoreRelease = false;
        isJustChangedDirection = false;
    }

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
        isJustChangedDirection = false;

        if (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease) {
            isIgnoreHold = false;
            isIgnoreRelease = false;
        }

        // TODO : Debug usage only
        if (Mathf.Approximately (rb.velocity.x, 0)) {
            if (situation == null) {
                Log.PrintError ("No horizontal velocity! Situation = null");
            } else {
                Log.PrintError ("No horizontal velocity! Situation = " + situation);
            }
        }
    }

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

    #region Action Preparation

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

    private CharacterEnum.CommandSituation? GetCurrentCommandSituation () {
        if (!isJustTapped && !isHolding && !isJustReleaseHold) {
            return null;
        }

        if (isJustTapped) {
            return isInAir ? CharacterEnum.CommandSituation.AirTap : CharacterEnum.CommandSituation.GroundTap;
        } else if (isHolding) {
            return isInAir ? CharacterEnum.CommandSituation.AirHold : CharacterEnum.CommandSituation.GroundHold;
        } else {
            return isInAir ? CharacterEnum.CommandSituation.AirRelease : CharacterEnum.CommandSituation.GroundRelease;
        }
    }

    private bool CheckIsAllowJump () {
        return consecutiveJumpCount < MaxConsecutiveJump;
    }

    private void ChangeDirection () {
        Log.PrintDebug ("ChangeDirection");
        if (facingDirection == CharacterEnum.Direction.Left) {
            facingDirection = CharacterEnum.Direction.Right;
        } else {
            facingDirection = CharacterEnum.Direction.Left;
        }

        isJustChangedDirection = true;
    }

    #endregion

    #region Player Action

    private void HandleCommand (CharacterEnum.CommandSituation? optionalSituation) {
        if (optionalSituation == null) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        var situation = (CharacterEnum.CommandSituation)optionalSituation;
        var command = GetCommandBySituation (situation);
        Log.PrintDebug ("Situation : " + situation + "   Command : " + command);

        // Fisish the hold command
        if (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease) {
            if (currentSituation == CharacterEnum.CommandSituation.GroundHold || currentSituation == CharacterEnum.CommandSituation.AirHold) {
                switch (currentCommand) {
                    case CharacterEnum.Command.Dash:
                        if (command == CharacterEnum.Command.Jump || command == CharacterEnum.Command.Dash) {
                            StopDashing (true, false);
                        } else {
                            StopDashing (false, true);
                        }
                        break;
                }
            }
        }

        if (isIgnoreHold && (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold)) {
            SetCurrentCommandStatus (null, null);
            return;
        }

        if (isIgnoreRelease && (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease)) {
            SetCurrentCommandStatus (null, null);
            return;
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
                            if (isJustChangedDirection) {
                                StopDashing (false, true);
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
            case CharacterEnum.Command.Arrow:
            case CharacterEnum.Command.Turn:
                break;
        }

        SetCurrentCommandStatus (situation, isTriggeredCommand ? command : null);
    }

    private void SetCurrentCommandStatus (CharacterEnum.CommandSituation? situation, CharacterEnum.Command? command) {
        currentSituation = situation;
        currentCommand = command;
    }

    private void HorizontalMovement () {
        if (!isAllowMove) {
            return;
        }

        var directionMultiplier = facingDirection == CharacterEnum.Direction.Right ? 1 : -1;
        var horizontalSpeed = 0f;

        switch (currentHorizontalSpeed) {
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
        rb.velocity = Vector2.zero;
        charAnim = CharacterEnum.Animation.Idle;
        // TODO : Idle animation
    }

    private void StartWalking () {
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;
        charAnim = CharacterEnum.Animation.Walking;
        // TODO : Walk animation
    }

    private void StartDashing (bool isOneShot) {
        Log.Print ("Dash : isOneShot = " + isOneShot);

        StopDashing (true, false);  // To ensure do not trigger 2 dash coroutines at the same time

        if (isOneShot) {
            dashCoroutine = StartCoroutine (OneShotDashCoroutine ());
        } else {
            SetDashing ();
        }
    }

    private void StopDashing (bool isKeepCurrentSpeed, bool isNeedDashCoolDown) {
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
        if (!isKeepCurrentSpeed) {
            currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;
        }

        // TODO : Walk / fall down animation

        if (isNeedDashCoolDown) {
            dashCoolDownCoroutine = StartCoroutine (DashCoolDownCoroutine ());
        }
    }

    private void SetDashing () {
        isDashing = true;
        rb.gravityScale = 0;  // prevent fall down behaviour from gravity
        rb.velocity = new Vector3 (rb.velocity.x, 0);
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Dash;

        charAnim = CharacterEnum.Animation.Dashing;
        // TODO : Dash animation
    }

    private IEnumerator OneShotDashCoroutine () {
        SetDashing ();

        var startTime = Time.time;

        while (Time.time - startTime < OneShotDashPeriod) {
            if (isJustChangedDirection) {
                break;
            }

            yield return null;
        }

        StopDashing (false, true);
    }

    private IEnumerator DashCoolDownCoroutine () {
        isDashCoolingDown = true;

        yield return new WaitForSeconds (AfterDashCoolDownPeriod);

        isDashCoolingDown = false;
        dashCoolDownCoroutine = null;
    }

    private void Jump (CharacterEnum.JumpChargeLevel level) {
        Log.Print ("Jump : JumpChargeLevel = " + level);

        if (JumpInitSpeedDict.ContainsKey(level)) {
            StopDashing (true, false);
            var jumpInitSpeed = JumpInitSpeedDict[level];
            rb.velocity = new Vector3 (rb.velocity.x, jumpInitSpeed);
            consecutiveJumpCount++;
            isInAir = true;
            charAnim = CharacterEnum.Animation.Jumping;
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

    private void LandToGround () {
        Log.PrintDebug ("LandToGround");
        consecutiveJumpCount = 0;
        isInAir = false;
        currentHorizontalSpeed = CharacterEnum.HorizontalSpeed.Walk;

        // Special handling of "AirHold - Jump" command
        if (currentJumpChargeLevel != CharacterEnum.JumpChargeLevel.Zero) {
            Log.Print ("LandToGround : Reset jump charge and ignore hold/release.");
            isIgnoreHold = true;
            isIgnoreRelease = true;

            // TODO : Remove charge animation
        }
        // Always reset jump charge to prevent the case that user started to hold the key while in air
        // but he land to ground so fast that he even not yet come to JumpChargeLevel One
        ResetJumpCharge ();

        charAnim = CharacterEnum.Animation.Landing;

        // TODO : Landing animation
    }

    #endregion

    #region Collider

    public void OnCollisionEnter2D (Collision2D collision) {
        switch (collision.gameObject.tag) {
            case GameVariable.GroundTag:
                LandToGround ();
                break;
            case GameVariable.WallTag:
                ChangeDirection ();
                break;
        }
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