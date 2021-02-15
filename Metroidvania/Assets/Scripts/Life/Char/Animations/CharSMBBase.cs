using HihiFramework.Core;
using UnityEngine;

public class CharSMBBase : StateMachineBehaviour {
    protected CharAnimUtils AnimUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateEnter : " + this, LogTypes.Animation | LogTypes.Char);

        if (AnimUtils == null) {
            AnimUtils = animator.GetComponent<CharAnimUtils> ();

            if (AnimUtils == null) {
                Log.PrintError ("Cannot find corresponding CharAnimUtils script.", LogTypes.Animation | LogTypes.Char);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateExit : " + this, LogTypes.Animation | LogTypes.Char);
    }
}