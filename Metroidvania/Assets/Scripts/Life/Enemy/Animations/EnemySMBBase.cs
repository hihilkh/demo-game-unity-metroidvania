using HihiFramework.Core;
using UnityEngine;

public class EnemySMBBase : StateMachineBehaviour {
    protected EnemyAnimUtils AnimUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateEnter : " + this, LogTypes.Animation | LogTypes.Enemy);

        if (AnimUtils == null) {
            AnimUtils = animator.GetComponentInChildren<EnemyAnimUtils> ();

            if (AnimUtils == null) {
                Log.PrintError ("Cannot find corresponding EnemyAnimUtils script.", LogTypes.Animation | LogTypes.Enemy);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateExit : " + this, LogTypes.Animation | LogTypes.Enemy);
    }
}
