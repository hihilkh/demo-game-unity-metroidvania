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

    public event Action<MapSwitch> HitArrowSwitchEvent;
    public event Action<MapSwitch> HitDropHitSwitchEvent;

    private void OnTriggerEnter2D (Collider2D collision) {
        switch (collision.tag) {
            case GameVariable.PlayerTag:
                return;
            case GameVariable.EnemyTag:
                var lifeBase = collision.GetComponentInParent<LifeBase> ();
                if (lifeBase == null) {
                    Log.PrintWarning ("No LifeBase is found : " + collision.gameObject.name + " . Please check.");
                    return;
                }

                HitLifeEvent?.Invoke (lifeBase, collision.transform, lifeBase.isInvincible);
                return;
            case GameVariable.ArrowSwitchTag:
            case GameVariable.DropHitSwitchTag:
                var mapSwitch = collision.GetComponent<MapSwitch> ();
                if (mapSwitch == null) {
                    Log.PrintWarning ("No MapSwitch is found : " + collision.gameObject.name + " . Please check.");
                    return;
                }

                if (collision.tag == GameVariable.ArrowSwitchTag) {
                    HitArrowSwitchEvent?.Invoke (mapSwitch);
                } else {
                    HitDropHitSwitchEvent?.Invoke (mapSwitch);
                }
                return;
            default:
                HitEnvironmentEvent?.Invoke (collision.transform);
                return;
        }
    }
}
