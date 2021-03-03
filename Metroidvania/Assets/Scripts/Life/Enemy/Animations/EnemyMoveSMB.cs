using HihiFramework.Core;
using UnityEngine;

public class EnemyMoveSMB : EnemySMBBase {

    private float startChasingCharTime;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        if (!AnimUtils.Model.Params.IsChaseChar) {
            if (AnimUtils.Model.MovementType == EnemyEnum.MovementType.Walking) {
                AnimUtils.UpdateHorizontalVelocity ();
            }
        }
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (AnimUtils.Model.Params.IsChaseChar) {
            if (AnimUtils.Model.MovementType == EnemyEnum.MovementType.Flying) {
                if (Time.time - startChasingCharTime >= AnimUtils.Model.Params.ChangeChaseCharDirPeriod) {
                    var direction = AnimUtils.Model.GetChaseCharNormalizedDir ();
                    Log.PrintError (direction);
                    AnimUtils.UpdateVelocity (direction);

                    startChasingCharTime = Time.time;
                }
            }

            // TODO : Walking type

            var currentFacingDirection = AnimUtils.Model.FacingDirection;
            if (currentFacingDirection == LifeEnum.HorizontalDirection.Left && AnimUtils.RB.velocity.x > 0) {
                AnimUtils.Model.ChangeFacingDirection ();
            } else if (currentFacingDirection == LifeEnum.HorizontalDirection.Right && AnimUtils.RB.velocity.x < 0) {
                AnimUtils.Model.ChangeFacingDirection ();
            }
        }
    }
}
