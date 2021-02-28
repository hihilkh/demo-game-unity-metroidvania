using System;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public class LifeCollision : MonoBehaviour {
    public class CollisionDetails {
        public LifeEnum.CollisionType CollisionType { get; }
        public Vector2 CollisionNormal { get; }
        public object AdditionalDetails { get; }

        public CollisionDetails (LifeEnum.CollisionType collisionType, Vector2 collisionNormal, object additionalDetails = null) {
            CollisionType = collisionType;
            CollisionNormal = collisionNormal;
            AdditionalDetails = additionalDetails;
        }
    }

    private readonly Dictionary<Collider2D, List<CollisionDetails>> collisionDict = new Dictionary<Collider2D, List<CollisionDetails>> ();

    //private readonly Dictionary<Collider2D, string> currentCollisionDict = new Dictionary<Collider2D, string> ();

    private const string WallColliderType = "Wall";
    //private const string RoofColliderType = "Roof";

    private static readonly Dictionary<string, LifeEnum.CollisionType> CollisionTypeDict = new Dictionary<string, LifeEnum.CollisionType> () {
        { GameVariable.GroundTag, LifeEnum.CollisionType.Ground },
        { WallColliderType, LifeEnum.CollisionType.Wall },
        { GameVariable.SlippyWallTag, LifeEnum.CollisionType.Wall },
        { GameVariable.DeathTag, LifeEnum.CollisionType.Death },
        { GameVariable.PlayerTag, LifeEnum.CollisionType.Char },
        { GameVariable.EnemyTag, LifeEnum.CollisionType.Enemy },
    };

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
    //public event Action<CharModel, Vector2> TouchedChar;
    /// <summary>
    /// Input :<br />
    /// CharModel : The EnemyModelBase of touched enemy<br />
    /// Vector2 : collisionNormal
    /// </summary>
    //public event Action<EnemyModelBase, Vector2> TouchedEnemy;

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

    #region Getter / Checking

    /// <summary>
    /// This getter ensure if the dictionary has a key, the coresponding value list has count >= 1
    /// </summary>
    public Dictionary<LifeEnum.CollisionType, List<CollisionDetails>> GetCollisionDetailsDict () {
        var result = new Dictionary<LifeEnum.CollisionType, List<CollisionDetails>> ();
        foreach (var pair in collisionDict) {
            foreach (var details in pair.Value) {
                if (!result.ContainsKey (details.CollisionType)) {
                    result.Add (details.CollisionType, new List<CollisionDetails> ());
                }

                result[details.CollisionType].Add (details);
            }
        }

        return result;
    }

    public bool CheckIsTouchingGround () {
        foreach (var pair in collisionDict) {
            foreach (var details in pair.Value) {
                if (details.CollisionType == LifeEnum.CollisionType.Ground) {
                    return true;
                }
            }
        }
        //foreach (var pair in currentCollisionDict) {
        //    if (pair.Value == GameVariable.GroundTag) {
        //        return true;
        //    }
        //}

        return false;
    }

    #endregion

    #region Collision Event

    public void OnCollisionEnter2D (Collision2D collision) {
        if (!collisionDict.ContainsKey (collision.collider)) {
            collisionDict.Add (collision.collider, new List<CollisionDetails> ());

            Log.PrintDebug (gameObject.name + " : Life Collision Enter : Collision GameObject name = " + collision.gameObject.name + " ; Tag = " + collision.gameObject.tag, LogTypes.Collision | LogTypes.Life);
        }

        AddCollisionDetails (collision);
    }

    public void OnCollisionExit2D (Collision2D collision) {
        if (collisionDict.ContainsKey (collision.collider)) {
            collisionDict.Remove (collision.collider);

            Log.PrintDebug (gameObject.name + " : Life Collision Exit : Collision GameObject name = " + collision.gameObject.name + " ; Tag = " + collision.gameObject.tag, LogTypes.Collision | LogTypes.Life);
        }
    }

    public void OnCollisionStay2D (Collision2D collision) {
        AddCollisionDetails (collision);
    }

    private void AddCollisionDetails (Collision2D collision) {
        if (!collisionDict.ContainsKey (collision.collider)) {
            Log.PrintError ("Somehow the collisionDict does not contain the collision's collider. Please check the logic.", LogTypes.Collision | LogTypes.Life);
            return;
        }

        var collisionDetailsList = collisionDict[collision.collider];
        collisionDetailsList.Clear ();

        for (var i = 0; i < collision.contactCount; i++) {
            var collideType = collision.gameObject.tag;
            var collisionNormal = collision.GetContact (i).normal;

            var absX = Mathf.Abs (collisionNormal.x);
            // Assume the collisions with environment are all with collisionNormal of up, down, left or right
            if ((collideType == GameVariable.GroundTag || collideType == GameVariable.SlippyWallTag) && (collisionNormal.y < 0 && -collisionNormal.y > absX)) {
                // No handling for cases of roof
                continue;
            }

            if (collideType == GameVariable.GroundTag) {
                if (collisionNormal.y >= 0 && collisionNormal.y < absX) {
                    collideType = WallColliderType;
                } else if (collisionNormal.y < 0 && -collisionNormal.y < absX) {
                    collideType = WallColliderType;
                }
            }

            if (!CollisionTypeDict.ContainsKey (collideType)) {
                // No handling for such collideType
                continue;
            }

            var type = CollisionTypeDict[collideType];
            object additionalDetails = null;
            switch (type) {
                case LifeEnum.CollisionType.Wall:
                    if (collideType == GameVariable.SlippyWallTag) {
                        additionalDetails = true;   // Means it is slippy wall
                    } else {
                        additionalDetails = false;   // Means it is not slippy wall
                    }
                    break;
                case LifeEnum.CollisionType.Char:
                case LifeEnum.CollisionType.Enemy:
                    additionalDetails = collision.gameObject.GetComponentInParent<LifeBase> ();
                    if (additionalDetails == null) {
                        Log.PrintError ("No LifeBase is found : " + collision.gameObject.name + " .Do not add to collisionDict. Please check.", LogTypes.Collision | LogTypes.Life);
                        continue;
                    }
                    break;
            }

            //Log.PrintDebug (gameObject.name + " : " + type + "   " + collisionNormal, LogTypes.Collision);

            collisionDetailsList.Add (new CollisionDetails (type, collisionNormal, additionalDetails));
        }
    }

    /* Outdated

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

    */

    #endregion
}