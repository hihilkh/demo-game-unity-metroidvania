using HihiFramework.Core;
using UnityEngine;

public class EnemySMBBase : StateMachineBehaviour {
    protected EnemyAnimSMBUtils AnimUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateEnter : " + this, LogTypes.Animation | LogTypes.Enemy);

        CheckAndSetAnimUtils (animator);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateExit : " + this, LogTypes.Animation | LogTypes.Enemy);

        // Remarks : Also check and set AnimUtils at OnStateExit because sometimes it will only trigger OnStateExit but not OnStateEnter
        CheckAndSetAnimUtils (animator);
    }

    private void CheckAndSetAnimUtils (Animator animator) {
        if (AnimUtils == null) {
            AnimUtils = animator.GetComponent<EnemyAnimSMBUtils> ();

            if (AnimUtils == null) {
                Log.PrintError ("Cannot find corresponding EnemyAnimUtils script.", LogTypes.Animation | LogTypes.Enemy);
            }
        }
    }
}
