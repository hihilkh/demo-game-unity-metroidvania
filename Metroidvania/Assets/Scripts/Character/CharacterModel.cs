using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public class CharacterModel : MonoBehaviour {
    // TODO : remove SerializeField and set const
    [SerializeField] private bool IsAutoMoveMode = true;
    [SerializeField] private float WalkingSpeed = 5f;
    [SerializeField] private float JumpInitSpeed = 8f;

    private const int MaxConsecutiveJump = 2;

    private Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command?> commandDict = new Dictionary<CharacterEnum.CommandSituation, CharacterEnum.Command?> {
        { CharacterEnum.CommandSituation.GroundTap, null },
        { CharacterEnum.CommandSituation.GroundHold, null },
        { CharacterEnum.CommandSituation.GroundRelease, null },
        { CharacterEnum.CommandSituation.AirTap, null },
        { CharacterEnum.CommandSituation.AirHold, null },
        { CharacterEnum.CommandSituation.AirRelease, null }
    };

    private Rigidbody2D rb;

    public CharacterController controller;

    private CharacterEnum.Direction facingDirection;
    private CharacterEnum.Direction? movingDirection;
    private CharacterEnum.Animation charAnim;
    private bool isInAir;
    private int consecutiveJumpCount;

    private bool isJustTapped;
    private bool isHolding;
    private bool isJustReleaseHold;

    // Temp control flags
    private bool isMoveLeft;
    private bool isMoveRight;

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

        if (IsAutoMoveMode) {
            StartAutoMove ();
        }
    }

    private void Update () {
        var situation = GetCurrentCommandSituation ();
        if (situation != null) {
            Log.Print (situation);
            HandleCommnand ((CharacterEnum.CommandSituation)situation);
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

        isJustTapped = false;
        isJustReleaseHold = false;
    }

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

    private void HandleCommnand (CharacterEnum.CommandSituation situation) {
        // TODO
        var isJump = false;
        if (situation == CharacterEnum.CommandSituation.GroundHold) {
            isJump = true;
        }

        if (situation == CharacterEnum.CommandSituation.GroundTap || situation == CharacterEnum.CommandSituation.AirTap) {
            isJump = true;
        }

        if (isJump && CheckIsAllowJump ()) {
            Jump ();
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

    private void Jump () {
        Log.PrintDebug ("Jump");

        rb.velocity = new Vector3 (rb.velocity.x, JumpInitSpeed);

        consecutiveJumpCount++;
        isInAir = true;
        charAnim = CharacterEnum.Animation.Jumping;
    }

    private void LandToGround () {
        Log.PrintDebug ("LandToGround");
        consecutiveJumpCount = 0;
        isInAir = false;

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