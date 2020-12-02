using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using System;

public class CharacterModel : MonoBehaviour {
    // TODO : remove SerializeField and set const

    [SerializeField] private bool IsAutoMoveMode = true;
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

    // TODO : Set commandDict to empty
    private Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> situationToCommandDict = new Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command> {
        { CharacterEnum.CommandSituation.GroundTap, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.GroundHold, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.GroundRelease, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.AirTap, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.AirHold, CharacterEnum.Command.Jump },
        { CharacterEnum.CommandSituation.AirRelease, CharacterEnum.Command.Jump }
    };

    private Rigidbody2D rb;

    public CharacterController controller;

    private CharacterEnum.Direction facingDirection;
    private CharacterEnum.Direction? movingDirection;
    private CharacterEnum.Animation charAnim;
    private bool isInAir;
    private int consecutiveJumpCount;

    // User Input Control
    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;
    private bool isMoveLeft;
    private bool isMoveRight;

    // Command Control
    private bool isIgnoreHold;
    private bool isIgnoreRelease;

    // Jump Command Control
    private CharacterEnum.JumpChargeLevel currentJumpChargeLevel;
    private float currentJumpChargedTime;

    void Start () {
        if (controller == null) {
            controller = GetComponent<CharacterController> ();
        }

        if (controller == null) {
            Log.PrintWarning ("Player controller is not assigned and cannot be found.");
        } else {
            controller.StartedLeft += StartMoveLeft;
            controller.StoppedLeft += StopMoveLeft;
            controller.StartedRight += StartMoveRight;
            controller.StoppedRight += StopMoveRight;
            controller.Tapped += TriggerTapAction;
            controller.StartedHold += StartHoldAction;
            controller.StoppedHold += StopHoldAction;
        }

        InitPlayer ();
    }

    private void InitPlayer () {
        rb = GetComponent<Rigidbody2D> ();

        facingDirection = CharacterEnum.Direction.Right;
        movingDirection = null;
        charAnim = CharacterEnum.Animation.Idle;
        isInAir = false;
        consecutiveJumpCount = 0;

        isJustTapped = false;
        isHolding = false;
        isJustReleaseHold = false;
        isMoveLeft = false;
        isMoveRight = false;

        currentJumpChargeLevel = CharacterEnum.JumpChargeLevel.Zero;
        currentJumpChargedTime = 0;

        if (IsAutoMoveMode) {
            StartAutoMove ();
        }
    }

    private void Update () {
        // Action by situation and command
        var situation = GetCurrentCommandSituation ();
        if (situation != null) {
            Log.PrintDebug (situation);
            HandleCommand ((CharacterEnum.CommandSituation)situation);
        }

        // Basic movement
        if (!IsAutoMoveMode) {
            if (isMoveLeft && !isMoveRight) {
                facingDirection = CharacterEnum.Direction.Left;
                movingDirection = facingDirection;
            } else if (!isMoveLeft && isMoveRight) {
                facingDirection = CharacterEnum.Direction.Right;
                movingDirection = facingDirection;
            } else {
                movingDirection = null;
            }
        }

        if (movingDirection != null) {
            Walk ();
        } else {
            Idle ();
        }

        // Reset control flags
        isJustTapped = false;
        isJustReleaseHold = false;

        if (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease) {
            isIgnoreHold = false;
            isIgnoreRelease = false;
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

    private void StartAutoMove () {
        movingDirection = facingDirection;
    }

    private void StopAutoMove () {
        movingDirection = null;
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
        movingDirection = facingDirection;
    }

    #endregion

    #region Player Action

    private void HandleCommand (CharacterEnum.CommandSituation situation) {
        if (isIgnoreHold && (situation == CharacterEnum.CommandSituation.GroundHold || situation == CharacterEnum.CommandSituation.AirHold)) {
            return;
        }

        if (isIgnoreRelease && (situation == CharacterEnum.CommandSituation.GroundRelease || situation == CharacterEnum.CommandSituation.AirRelease)) {
            return;
        }

        var command = GetCommandBySituation (situation);

        if (command == null) {
            return;
        }

        command = (CharacterEnum.Command)command;

        switch (command) {
            case CharacterEnum.Command.Jump:
                if (!CheckIsAllowJump ()) {
                    break;
                }

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
                        Jump (currentJumpChargeLevel);
                        break;
                }
                break;
            // TODO
            case CharacterEnum.Command.Dash:
            case CharacterEnum.Command.Hit:
            case CharacterEnum.Command.Arrow:
            case CharacterEnum.Command.Turn:
                break;
        }
    }

    private void Idle () {
        //Log.PrintDebug ("Idle");
        movingDirection = null;
        charAnim = CharacterEnum.Animation.Idle;
    }

    private void Walk () {
        //Log.PrintDebug ("Walk");
        var multiplier = facingDirection == CharacterEnum.Direction.Right ? 1 : -1;
        transform.position = transform.position + new Vector3 (WalkingSpeed * Time.deltaTime, 0, 0) * multiplier;
        charAnim = CharacterEnum.Animation.Walking;
    }

    private void Jump (CharacterEnum.JumpChargeLevel level) {
        Log.PrintDebug ("Jump");

        if (JumpInitSpeedDict.ContainsKey(level)) {
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

        if (currentJumpChargeLevel != CharacterEnum.JumpChargeLevel.Zero) {
            Log.Print ("LandToGround : Reset jump charge and ignore hold/release.");
            isIgnoreHold = true;
            isIgnoreRelease = true;

            // TODO : Reset char animation
        }
        // Always reset jump charge to prevent the case that user started to hold the key while in air
        // but he land to ground so fast that he even not yet come to JumpChargeLevel One
        ResetJumpCharge ();

        charAnim = CharacterEnum.Animation.Landing;
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

    public void StartMoveLeft () {
        Log.PrintDebug ("StartMoveLeft");
        isMoveLeft = true;
    }

    public void StopMoveLeft () {
        Log.PrintDebug ("StopMoveLeft");
        isMoveLeft = false;
    }

    public void StartMoveRight () {
        Log.PrintDebug ("StartMoveRight");
        isMoveRight = true;
    }

    public void StopMoveRight () {
        Log.PrintDebug ("StopMoveRight");
        isMoveRight = false;
    }

    public void TriggerTapAction () {
        if (isHolding) {
            Log.PrintWarning ("Somehow triggered tap while holding. Do not do tap action.");
            return;
        }

        Log.PrintDebug ("TriggerTapAction");
        isJustTapped = true;
    }

    public void StartHoldAction () {
        if (isJustTapped) {
            Log.PrintWarning ("Somehow triggered hold while just tapped. Do not do hold action.");
            return;
        }

        Log.PrintDebug ("StartHoldAction");
        isHolding = true;
    }

    public void StopHoldAction () {
        Log.PrintDebug ("StopHoldAction");
        isHolding = false;
        isJustReleaseHold = true;
    }

    #endregion
}