using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class LifeBase : MonoBehaviour {
    protected virtual int zLayer => 0;
    protected abstract int totalHP { get; }
    protected int currentHP;

    [SerializeField] protected Transform baseTransform;
    [SerializeField] protected LifeCollision lifeCollision;
    public LifeEnum.HorizontalDirection facingDirection { get; protected set; }
    public LifeEnum.Location currentLocation { get; protected set; }

    private bool isInitialized = false;

    protected virtual void OnDestroy () {
        if (isInitialized) {
            UnregisterCollisionEventHandler ();
        }
    }

    /// <returns>has initialized before</returns>
    public virtual bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        if (isInitialized) {
            Log.PrintWarning (gameObject.name + " is already initialized. Do not initialize again. Please check.", LogType.General);
            return true;
        }

        isInitialized = true;
        currentHP = totalHP;
        SetPosAndDirection (pos, direction);
        RegisterCollisionEventHandler ();

        return false;
    }

    // TODO : Handle the cases that change pos after init
    public virtual void SetPosAndDirection (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        SetPos (pos);
        facingDirection = direction;

        StartCoroutine (ResetCurrentLocation ());
    }

    public virtual void SetPos (Vector2 pos) {
        baseTransform.position = new Vector3 (pos.x, pos.y, zLayer);
    }

    public virtual void SetPosByOffset (Vector2 offset) {
        baseTransform.position += (Vector3)offset;
    }

    #region HP

    /// <summary>
    /// Hurt the life by <paramref name="dp"/> (damage point) and if hp come to zero, it calls Die ()
    /// </summary>
    /// <returns>isAlive</returns>
    public virtual bool Hurt (int dp) {
        currentHP = Mathf.Max (0, currentHP - dp);

        if (currentHP == 0) {
            Die ();
            return false;
        } else {
            return true;
        }
    }

    protected virtual void Die () {
        currentHP = 0;
    }

    #endregion

    #region Location

    private IEnumerator ResetCurrentLocation () {
        currentLocation = LifeEnum.Location.Unknown;

        yield return null;

        // Wait for a frame and see if currentLocation has already been assigned (e.g. by collision event).
        // If not, assume it is in air.
        if (currentLocation == LifeEnum.Location.Unknown) {
            currentLocation = LifeEnum.Location.Air;
        }
    }

    #endregion

    #region Collision

    protected abstract void RegisterCollisionEventHandler ();
    protected abstract void UnregisterCollisionEventHandler ();

    #endregion
}