using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class LifeBase : MonoBehaviour {
    [SerializeField] protected Transform baseTransform;
    [SerializeField] protected LifeCollision lifeCollision;
    [SerializeField] private LifeHPView hpView;
    public virtual LifeEnum.HorizontalDirection facingDirection { get; protected set; }
    [SerializeField] public virtual LifeEnum.Location currentLocation { get; protected set; }

    protected abstract int posZ { get; }
    protected abstract int invincibleLayer { get; }
    protected abstract int totalHP { get; }

    private int _currentHP;
    protected int currentHP {
        get { return _currentHP; }
        set {
            if (_currentHP != value) {
                _currentHP = value;
                hpView.UpdateView (totalHP, value);
            }
        }
    }

    // Status
    public abstract bool isBeatingBack { get; protected set; }
    public abstract bool isInvincible { get; protected set; }
    public abstract bool isDying { get; protected set; }

    protected abstract float invinciblePeriod { get; }

    protected bool isInitialized = false;

    protected virtual void OnDestroy () {
        if (isInitialized) {
            UnregisterCollisionEventHandler ();
        }
    }

    /// <returns>has initialized before</returns>
    public virtual bool Init (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        if (isInitialized) {
            Log.PrintWarning (gameObject.name + " is already initialized. Do not initialize again. Please check.", LogType.Life);
            return true;
        }

        isInitialized = true;

        currentHP = totalHP;
        SetPosAndDirection (pos, direction);

        lifeCollision.Init (invincibleLayer);
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
        baseTransform.position = new Vector3 (pos.x, pos.y, posZ);
    }

    public virtual void SetPosByOffset (Vector2 offset) {
        baseTransform.position += (Vector3)offset;
    }

    #region HP

    /// <summary>
    /// Hurt the life by <paramref name="dp"/> (damage point) and if hp come to zero, it calls Die ()
    /// </summary>
    /// <returns>isAlive</returns>
    public virtual bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        currentHP = Mathf.Max (0, currentHP - dp);

        if (currentHP == 0) {
            Die (hurtDirection);
            return false;
        } else {
            StartBeatingBack (hurtDirection);
            StartCoroutine (SetInvincible (true));

            return true;
        }
    }

    protected virtual void Die (LifeEnum.HorizontalDirection dieDirection) {
        currentHP = 0;
        isDying = true;

        StartBeatingBack (dieDirection);
        StartCoroutine (SetInvincible (false));
    }

    protected virtual void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        isBeatingBack = true;
    }

    protected virtual void StopBeatingBack () {
        isBeatingBack = false;
    }

    protected IEnumerator SetInvincible (bool isTempInvincible) {
        StartInvincible ();

        if (!isTempInvincible) {
            yield break;
        }

        yield return new WaitForSeconds (invinciblePeriod);

        StopInvincible ();
    }

    protected virtual void StartInvincible () {
        lifeCollision.SetLayer (true);
        isInvincible = true;
    }

    protected virtual void StopInvincible () {
        lifeCollision.SetLayer (false);
        isInvincible = false;
    }

    #endregion

    #region Location

    protected virtual IEnumerator ResetCurrentLocation () {
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