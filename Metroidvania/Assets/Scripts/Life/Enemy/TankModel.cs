using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public class TankModel : EnemyModelBase {
    public override EnemyEnum.EnemyType EnemyType => EnemyEnum.EnemyType.Tank;
    public override EnemyEnum.MovementType MovementType => EnemyEnum.MovementType.Walking;

    private TankParams GetParams () {
        if (Params is TankParams) {
            return (TankParams)Params;
        }

        Log.PrintError ("Params is not with type TankParams. Please check.", LogTypes.Enemy);
        return null;
    }

    protected override void TouchWallAction (LifeEnum.HorizontalDirection wallPosition) {
        Log.PrintDebug (gameObject.name + " : Touch wall", LogTypes.Enemy | LogTypes.Collision);
        if (wallPosition == FacingDirection) {
            SetAnimatorTrigger (EnemyAnimConstant.IdleTriggerName);
            StartCoroutine (WaitToTurnAndMove (GetParams ().TouchWallWaitPeriod));
        }
    }

    private IEnumerator WaitToTurnAndMove (float waitPeriod) {
        yield return new WaitForSeconds (waitPeriod);

        ChangeFacingDirection ();
        SetAnimatorTrigger (EnemyAnimConstant.MoveTriggerName);
    }

    protected override void CharLostHandler () {
        // Once char is detected, keep moving
    }
}