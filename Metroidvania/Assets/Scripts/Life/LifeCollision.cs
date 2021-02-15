using System;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class LifeCollision : MonoBehaviour {
    private readonly Dictionary<Collider2D, string> currentCollisionDict = new Dictionary<Collider2D, string> ();

    private const string WallColliderType = "Wall";
    private const string RoofColliderType = "Roof";

    public event Action TouchedGround;
    public event Action LeftGround;
    public event Action TouchedRoof;
    public event Action LeftRoof;
    /// <summary>
    /// Input :<br />
    /// LifeEnum.HorizontalDirection : wallPosition<br />
    /// bool : isSlippyWall
    /// </summary>
    public event Action<LifeEnum.HorizontalDirection, bool> TouchedWall;
    /// <summary>
    /// Input :<br />
    /// bool : isSlippyWall
    /// </summary>
    public event Action<bool> LeftWall;
    public event Action TouchedDeathTag;
    /// <summary>
    /// Input :<br />
    /// CharModel : The CharModel of touched character<br />
    /// Vector2 : collisionNormal
    /// </summary>
    public event Action<CharModel, Vector2> TouchedChar;
    /// <summary>
    /// Input :<br />
    /// CharModel : The EnemyModelBase of touched enemy<br />
    /// Vector2 : collisionNormal
    /// </summary>
    public event Action<EnemyModelBase, Vector2> TouchedEnemy;

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
        Log.Print (gameObject.name + " : Life Collision Enter : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType + " ; collisionNormal = " + collisionNormal, LogTypes.Collision | LogTypes.Life);

        var isOriginallyTouchingGround = CheckIsTouchingGround ();  // Remarks : Check before amending currentCollisionDict
        if (currentCollisionDict.ContainsKey (collision.collider)) {
            currentCollisionDict[collision.collider] = collideType;
        } else {
            currentCollisionDict.Add (collision.collider, collideType);
        }

        switch (collideType) {
            case GameVariable.GroundTag:
                if (!isOriginallyTouchingGround) {
                    TouchedGround?.Invoke ();
                }
                break;
            case RoofColliderType:
                TouchedRoof?.Invoke ();
                break;
            case WallColliderType:
            case GameVariable.SlippyWallTag:
                var wallPosition = (collisionNormal.x <= 0) ? LifeEnum.HorizontalDirection.Right : LifeEnum.HorizontalDirection.Left;
                TouchedWall?.Invoke (wallPosition, collideType == GameVariable.SlippyWallTag);
                break;
            case GameVariable.DeathTag:
                TouchedDeathTag?.Invoke ();
                break;
            case GameVariable.PlayerTag:
            case GameVariable.EnemyTag:
                var lifeBase = collision.gameObject.GetComponentInParent<LifeBase> ();
                if (lifeBase == null) {
                    Log.PrintWarning ("No LifeBase is found : " + collision.gameObject.name + " . Please check.", LogTypes.Collision | LogTypes.Life);
                    break;
                }

                if (collideType == GameVariable.PlayerTag) {
                    TouchedChar?.Invoke ((CharModel)lifeBase, collisionNormal);
                } else {
                    TouchedEnemy?.Invoke ((EnemyModelBase)lifeBase, collisionNormal);
                }
                break;
            default:
                Log.PrintDebug (gameObject.name + " : No event is implemented for Collision Enter of collideType : " + collideType, LogTypes.Collision | LogTypes.Life);
                break;
        }
    }

    public void OnCollisionExit2D (Collision2D collision) {
        if (!currentCollisionDict.ContainsKey (collision.collider)) {
            Log.PrintError (gameObject.name + " : Missing key in currentCollisionDict. collision name : " + collision.gameObject.name, LogTypes.Collision | LogTypes.Life);
            return;
        }

        var collideType = currentCollisionDict[collision.collider];
        currentCollisionDict.Remove (collision.collider);

        Log.Print (gameObject.name + " : Life Collision Exit : Tag = " + collision.gameObject.tag + " ; collideType = " + collideType, LogTypes.Collision | LogTypes.Life);

        switch (collideType) {
            case GameVariable.GroundTag:
                if (!CheckIsTouchingGround ()) {
                    LeftGround?.Invoke ();
                }
                break;
            case RoofColliderType:
                LeftRoof?.Invoke ();
                break;
            case WallColliderType:
                LeftWall?.Invoke (false);
                break;
            case GameVariable.SlippyWallTag:
                LeftWall?.Invoke (true);
                break;
            default:
                Log.PrintDebug (gameObject.name + " : No event is implemented for Collision Exit of collideType : " + collideType, LogTypes.Collision | LogTypes.Life);
                break;
        }
    }

    #endregion
}