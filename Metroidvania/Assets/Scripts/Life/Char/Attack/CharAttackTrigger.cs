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
    public event Action<LifeBase, Transform, bool> HitLifeEvent;

    /// <summary>
    /// Input :<br />
    /// Transform : colliderTransform
    /// </summary>
    public event Action<Transform> HitEnvironmentEvent;

    private void OnTriggerEnter2D (Collider2D collision) {
        if (collision.tag == GameVariable.PlayerTag || collision.tag == GameVariable.AttackTag) {
            return;
        }

        if (collision.tag == GameVariable.EnemyTag) {
            var lifeBase = collision.GetComponentInParent<LifeBase> ();
            if (lifeBase == null) {
                Log.PrintWarning ("No LifeBase is found : " + collision.gameObject.name + " . Please check.");
                return;
            }

            HitLifeEvent?.Invoke (lifeBase, collision.transform, lifeBase.isInvincible);
        } else {
            HitEnvironmentEvent?.Invoke (collision.transform);
        }
    }
}
