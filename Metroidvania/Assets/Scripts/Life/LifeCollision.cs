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

    private const string WallColliderType = "Wall";
    private const string RoofColliderType = "Roof";

    private static readonly Dictionary<string, LifeEnum.CollisionType> CollisionTypeDict = new Dictionary<string, LifeEnum.CollisionType> () {
        { GameVariable.GroundTag, LifeEnum.CollisionType.Ground },
        { WallColliderType, LifeEnum.CollisionType.Wall },
        { GameVariable.SlippyWallTag, LifeEnum.CollisionType.Wall },
        { RoofColliderType, LifeEnum.CollisionType.Roof },
        { GameVariable.DeathTag, LifeEnum.CollisionType.Death },
        { GameVariable.PlayerTag, LifeEnum.CollisionType.Char },
        { GameVariable.EnemyTag, LifeEnum.CollisionType.Enemy },
    };

    private int originalLayer;
    private int invincibleLayer;

    private void Awake () {
        originalLayer = gameObject.layer;
    }

    private void OnEnable () {
        collisionDict.Clear ();
    }

    public void Init (int invincibleLayer) {
        this.invincibleLayer = invincibleLayer;
    }

    #region Layer

    public void SetLayer (bool isInvincible) {
        gameObject.layer = isInvincible ? invincibleLayer : originalLayer;
    }

    /// <summary>
    /// Use it with care. Normally you won't need to specifically assign the layer.
    /// </summary>
    public void SetLayer (int layer) {
        gameObject.layer = layer;
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
                collideType = RoofColliderType;
            } else if (collideType == GameVariable.GroundTag) {
                if (collisionNormal.y >= 0 && collisionNormal.y < absX) {
                    collideType = WallColliderType;
                } else if (collisionNormal.y < 0 && -collisionNormal.y < absX) {
                    collideType = WallColliderType;
                }
            } else if (collideType == GameVariable.SlippyWallTag) {
                if (collisionNormal.y > 0 && collisionNormal.y > absX) {
                    collideType = GameVariable.GroundTag;
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

    #endregion
}