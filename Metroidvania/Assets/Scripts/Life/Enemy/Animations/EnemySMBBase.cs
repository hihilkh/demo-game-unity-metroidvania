﻿using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class EnemySMBBase : StateMachineBehaviour {
    protected EnemyAnimUtils animUtils { get; private set; }

    public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateEnter : " + this, LogType.Animation | LogType.Enemy);

        if (animUtils == null) {
            animUtils = animator.GetComponentInChildren<EnemyAnimUtils> ();

            if (animUtils == null) {
                Log.PrintError ("Cannot find corresponding EnemyAnimUtils script.", LogType.Animation | LogType.Enemy);
            }
        }
    }

    public override void OnStateExit (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit (animator, stateInfo, layerIndex);

        Log.PrintDebug ("Enemy OnStateExit : " + this, LogType.Animation | LogType.Enemy);
    }
}
