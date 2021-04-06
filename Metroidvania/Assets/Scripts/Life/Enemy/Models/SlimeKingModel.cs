using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class SlimeKingModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.SlimeKing;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Walking;

    private Coroutine keepFacingToCharCoroutine = null;

    public SlimeKingParams GetParams () {
        if (Params is SlimeKingParams) {
            return (SlimeKingParams)Params;
        }

        Log.PrintError ("Params is not with type SlimeKingParams. Please check.", LogTypes.Enemy);
        return null;
    }

    /// <returns>Is jump success</returns>
    protected override bool Jump () {
        var distVector = GetChasingCharDirection (false);
        var horizontalDistanceFromChar = Mathf.Abs (distVector.x);

        if (horizontalDistanceFromChar < GetParams ().AttackHorizontalHalfRange) {
            return AttackJump ();
        } else {
            return base.Jump ();
        }
    }

    private bool AttackJump () {
        Log.PrintDebug (gameObject.name + " : AttackJump", LogTypes.Enemy);

        if (CurrentLocation != LifeEnum.Location.Ground) {
            Log.Print (gameObject.name + " : Do not AttackJump. It is not in ground", LogTypes.Enemy);
            return false;
        }

        IsJustBeforeJumpedUp = true;
        SetAnimatorTrigger (EnemyAnimConstant.AttackTriggerName);

        if (keepFacingToCharCoroutine == null) {
            keepFacingToCharCoroutine = StartCoroutine (KeepFacingToChar ());
        }

        return true;
    }

    private IEnumerator KeepFacingToChar () {
        while (true) {
            yield return null;
            FacingDirection = GetChasingCharHorizontalDirection ();
        }
    }

    private void StopKeepFacingToChar () {
        if (keepFacingToCharCoroutine != null) {
            StopCoroutine (keepFacingToCharCoroutine);
            keepFacingToCharCoroutine = null;
        }
    }

    public void AttackStarted () {
        StopKeepFacingToChar ();
    }

    protected override void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        base.StartBeatingBack (hurtDirection);

        StopKeepFacingToChar ();
    }
}