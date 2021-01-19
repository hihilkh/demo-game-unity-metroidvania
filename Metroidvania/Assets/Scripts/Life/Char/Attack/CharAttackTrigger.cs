using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class CharAttackTrigger : MonoBehaviour {
    /// <summary>
    /// Input :<br />
    /// LifeBase : the LifeBase that the attack has hit<br />
    /// Transform : colliderTransform<br />
    /// bool : isInvincible
    /// </summary>
    public event Action<EnemyModelBase, Transform, bool> HitEnemyEvent;

    private void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == GameVariable.PlayerTag || collision.tag == GameVariable.AttackTag) {
            return;
        }

        if (collision.tag == GameVariable.EnemyTag) {
            var lifeCollision = collision.GetComponent<LifeCollision> ();
            if (lifeCollision == null) {
                Log.PrintWarning ("No LifeCollision script for collider : " + collision.gameObject.name + " . Please check.");
                return;
            }

            var lifeBase = lifeCollision.GetLifeBase ();
            if (lifeBase == null) {
                Log.PrintWarning ("No LifeBase attached to LifeCollision : " + lifeCollision.gameObject.name + " . Please check.");
                return;
            }

            var convert = (EnemyModelBase)lifeBase;

            HitEnemyEvent?.Invoke (convert, collision.transform, convert.GetIsInvincible ());
        }
    }
}
