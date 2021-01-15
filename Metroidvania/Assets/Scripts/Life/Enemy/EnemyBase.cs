using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : LifeBase {
    #region HP related

    public override bool Hurt (int dp) {
        var isAlive = base.Hurt (dp);

        if (isAlive) {
            // TODO : Hurt Animation
        }

        return isAlive;
    }

    protected override void Die () {
        base.Die ();

        // TODO
    }

    #endregion
}
