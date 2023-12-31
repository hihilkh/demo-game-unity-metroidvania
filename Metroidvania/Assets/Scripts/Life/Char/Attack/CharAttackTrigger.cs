﻿using System;
using HihiFramework.Core;
using UnityEngine;

public class CharAttackTrigger : MonoBehaviour {
    /// <summary>
    /// Input :<br />
    /// LifeBase : the LifeBase that the attack has hit<br />
    /// bool : isHurt
    /// </summary>
    public event Action<LifeBase, bool> HitLife;

    /// <summary>
    /// Input :<br />
    /// Transform : colliderTransform
    /// </summary>
    public event Action<Transform> HitEnvironment;

    public event Action<MapSwitch> HitArrowSwitch;
    public event Action<MapSwitch> HitDropHitSwitch;

    private void OnTriggerEnter2D (Collider2D collision) {
        switch (collision.tag) {
            case GameVariable.PlayerTag:
            case GameVariable.EnemyTag:
                var lifeBase = collision.GetComponentInParent<LifeBase> ();
                if (lifeBase == null) {
                    Log.PrintWarning ("No LifeBase is found : " + collision.gameObject.name + " . Please check.");
                    return;
                }

                var isHurt = !lifeBase.IsInvincible;
                if (isHurt) {
                    if (collision.gameObject.layer == GameVariable.EnemyDefendLayer) {
                        isHurt = false;
                    }
                }

                HitLife?.Invoke (lifeBase, isHurt);
                return;
            case GameVariable.ArrowSwitchTag:
            case GameVariable.DropHitSwitchTag:
                var mapSwitch = collision.GetComponent<MapSwitch> ();
                if (mapSwitch == null) {
                    Log.PrintWarning ("No MapSwitch is found : " + collision.gameObject.name + " . Please check.");
                    return;
                }

                if (collision.tag == GameVariable.ArrowSwitchTag) {
                    HitArrowSwitch?.Invoke (mapSwitch);
                } else {
                    HitDropHitSwitch?.Invoke (mapSwitch);
                }
                return;
            default:
                HitEnvironment?.Invoke (collision.transform);
                return;
        }
    }
}
