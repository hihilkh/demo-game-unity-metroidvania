using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class SlimeKingModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.SlimeKing;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Walking;

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
        var distanceSquareFromChar = distVector.sqrMagnitude;

        if (distanceSquareFromChar < GetParams ().AttackRangeDistanceSquare) {
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

        isJustJumpedUp = true;
        SetAnimatorTrigger (EnemyAnimConstant.AttackTriggerName);

        return true;
    }
}