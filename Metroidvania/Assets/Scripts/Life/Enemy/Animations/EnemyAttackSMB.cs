using UnityEngine;

public class EnemyAttackSMB : EnemySMBBase {
    private bool isHandleStateUpdate = false;

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        isHandleStateUpdate = true;
    }

    public override void OnStateUpdate (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate (animator, stateInfo, layerIndex);

        if (!isHandleStateUpdate) {
            return;
        }

        switch (AnimUtils.Model.EnemyType) {
            case EnemyEnum.EnemyType.SlimeKing:

                if (stateInfo.normalizedTime >= 1) {
                    isHandleStateUpdate = false;

                    if (animator.GetNextAnimatorClipInfo (0).Length == 0) {
                        var slimeKingModel = (SlimeKingModel)AnimUtils.Model;
                        slimeKingModel.AttackStarted ();
                        var velocity = slimeKingModel.GetParams ().AttackJumpInitVelocity;
                        if (AnimUtils.Model.FacingDirection == LifeEnum.HorizontalDirection.Left) {
                            velocity = Vector2.Scale (velocity, new Vector2 (-1, 1));
                        }
                        AnimUtils.RB.velocity = velocity;
                    }
                }
                break;
        }
    }
}