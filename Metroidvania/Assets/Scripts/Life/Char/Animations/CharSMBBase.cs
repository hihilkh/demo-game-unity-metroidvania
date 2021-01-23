﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public class CharSMBBase : StateMachineBehaviour {
    protected CharAnimUtils animUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateEnter : " + this, LogType.Animation | LogType.Char);

        if (animUtils == null) {
            animUtils = animator.GetComponent<CharAnimUtils> ();

            if (animUtils == null) {
                Log.PrintError ("Cannot find corresponding CharAnimUtils script.", LogType.Animation | LogType.Char);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Char OnStateExit : " + this, LogType.Animation | LogType.Char);
    }
}