using HihiFramework.Core;
using UnityEngine;

public class CharSMBBase : StateMachineBehaviour {
    protected CharAnimUtils AnimUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateEnter : " + this, LogTypes.Animation | LogTypes.Char);

        CheckAndSetAnimUtils (animator);
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateExit : " + this, LogTypes.Animation | LogTypes.Char);

        // Remarks : Also check and set AnimUtils at OnStateExit because sometimes it will only trigger OnStateExit but not OnStateEnter
        CheckAndSetAnimUtils (animator);
    }

    private void CheckAndSetAnimUtils (Animator animator) {
        if (AnimUtils == null) {
            AnimUtils = animator.GetComponent<CharAnimUtils> ();

            if (AnimUtils == null) {
                Log.PrintError ("Cannot find corresponding CharAnimUtils script.", LogTypes.Animation | LogTypes.Char);
            }
        }
    }
}