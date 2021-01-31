﻿using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class LifeCollision : MonoBehaviour {
    private Dictionary<Collider2D, string> currentCollisionDict = new Dictionary<Collider2D, string> ();

    private const string WallColliderType = "Wall";
    private const string RoofColliderType = "Roof";

    public event Action TouchedGroundEvent;
    public event Action LeftGroundEvent;
    public event Action TouchedRoofEvent;
    public event Action LeftRoofEvent;
    /// <summary>
    /// Input :<br />
    /// LifeEnum.HorizontalDirection : wallPosition<br />
    /// bool : isSlippyWall
    /// </summary>
    public event Action<LifeEnum.HorizontalDirection, bool> TouchedWallEvent;
    /// <summary>
    /// Input :<br />
    /// bool : isSlippyWall
    /// </summary>
    public event Action<bool> LeftWallEvent;
    public event Action TouchedDeathTagEvent;
    /// <summary>
    /// Input :<br />
    /// CharModel : The CharModel of touched character<br />
    /// Vector2 : collisionNormal
    /// </summary>
    public event Action<CharModel, Vector2> TouchedCharEvent;
    /// <summary>
    /// Input :<br />
    /// CharModel : The EnemyModelBase of touched enemy<br />
    /// Vector2 : collisionNormal
    /// </summary>
    public event Action<EnemyModelBase, Vector2> TouchedEnemyEvent;

    private int originalLayer;
    private int invincibleLayer;

    private void Awake () {
        originalLayer = gameObject.layer;
    }

    public void Init (int invincibleLayer) {
        this.invincibleLayer = invincibleLayer;
    }

    #region Layer

    public void SetLayer (bool isInvincible) {
        gameObject.layer = isInvincible ? invincibleLayer : originalLayer;
    }

    #endregion

    #region Collision Event

    public bool CheckIsTouchingGround () {
        foreach (var pair in currentCollisionDict) {
            if (pair.Value == GameVariable.GroundTag) {
                return true;
            }
        }

        return false;
    }

    public void OnCollisionEnter2D (Collision2D collision) {
        var collideType = collision.gameObject.tag;

        var collisionNormal = collision.GetContact (0).normal;

        if (collideType == GameVariable.GroundTag) {
            var absX = Mathf.Abs (collisionNormal.x);
            if (collisionNormal.y >= 0 && collisionNormal.y < absX) {
                collideType = WallColliderType;
            } else if (collisionNormal.y < 0 && -collisionNormal.y < absX) {
                collideType = WallColliderType;
            }
        }

        if ((collideType == GameVariable.GroundTag || collideType == GameVariable.SlippyWallTag) && collisionNormal.y < 0) {
            collideType = RoofColliderType;
        }
        Log.Print (gameObject.name + " : Life Collision Enter : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType + " ; collisionNormal = " + collisionNormal, LogType.Collision | LogType.Life);

        var isOriginallyTouchingGround = CheckIsTouchingGround ();  // Remarks : Check before amending currentCollisionDict
        if (currentCollisionDict.ContainsKey (collision.collider)) {
            currentCollisionDict[collision.collider] = collideType;
        } else {
            currentCollisionDict.Add (collision.collider, collideType);
        }

        switch (collideType) {
            case GameVariable.GroundTag:
                if (!isOriginallyTouchingGround) {
                    TouchedGroundEvent?.Invoke ();
                }
                break;
            case RoofColliderType:
                TouchedRoofEvent?.Invoke ();
                break;
            case WallColliderType:
            case GameVariable.SlippyWallTag:
                var wallPosition = (collisionNormal.x <= 0) ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                TouchedWallEvent?.Invoke (wallPosition, collideType == GameVariable.SlippyWallTag);
                break;
            case GameVariable.DeathTag:
                TouchedDeathTagEvent?.Invoke ();
                break;
            case GameVariable.PlayerTag:
            case GameVariable.EnemyTag:
                var lifeBase = collision.gameObject.GetComponentInParent<LifeBase> ();
                if (lifeBase == null) {
                    Log.PrintWarning ("No LifeBase is found : " + collision.gameObject.name + " . Please check.", LogType.Collision | LogType.Life);
                    break;
                }

                if (collideType == GameVariable.PlayerTag) {
                    TouchedCharEvent?.Invoke ((CharModel)lifeBase, collisionNormal);
                } else {
                    TouchedEnemyEvent?.Invoke ((EnemyModelBase)lifeBase, collisionNormal);
                }
                break;
            default:
                Log.PrintDebug (gameObject.name + " : No event is implemented for Collision Enter of collideType : " + collideType, LogType.Collision | LogType.Life);
                break;
        }
    }

    public void OnCollisionExit2D (Collision2D collision) {
        if (!currentCollisionDict.ContainsKey (collision.collider)) {
            Log.PrintError (gameObject.name + " : Missing key in currentCollisionDict. collision name : " + collision.gameObject.name, LogType.Collision | LogType.Life);
            return;
        }

        var collideType = currentCollisionDict[collision.collider];
        currentCollisionDict.Remove (collision.collider);

        Log.Print (gameObject.name + " : Life Collision Exit : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType, LogType.Collision | LogType.Life);

        switch (collideType) {
            case GameVariable.GroundTag:
                if (!CheckIsTouchingGround ()) {
                    LeftGroundEvent?.Invoke ();
                }
                break;
            case RoofColliderType:
                LeftRoofEvent?.Invoke ();
                break;
            case WallColliderType:
                LeftWallEvent?.Invoke (false);
                break;
            case GameVariable.SlippyWallTag:
                LeftWallEvent?.Invoke (true);
                break;
            default:
                Log.PrintDebug (gameObject.name + " : No event is implemented for Collision Exit of collideType : " + collideType, LogType.Collision | LogType.Life);
                break;
        }
    }

    #endregion
}