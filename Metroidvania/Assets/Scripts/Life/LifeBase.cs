using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public abstract class LifeBase<T> : MonoBehaviour where T : LifeParams {
    [SerializeField] protected Transform baseTransform;
    [SerializeField] protected LifeCollision lifeCollision;
    [SerializeField] private T lifeParams;
    public LifeEnum.HorizontalDirection facingDirection { get; protected set; }
    public virtual LifeEnum.Location currentLocation { get; protected set; }
    protected LifeEnum.Status currentStatus;

    protected virtual int zLayer => 0;
    protected int totalHP => lifeParams.totalHP;
    protected int currentHP;

    private bool isInitialized = false;

    public T GetParams () {
        return lifeParams;
    }

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
        currentStatus = LifeEnum.Status.Normal;

        lifeCollision.SetLifeBase (this);
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
        currentStatus = currentStatus | LifeEnum.Status.Dying;

        StartBeatingBack (dieDirection);
        StartCoroutine (SetInvincible (false));
    }

    public bool GetIsBeatingBack () {
        return (currentStatus & LifeEnum.Status.BeatingBack) == LifeEnum.Status.BeatingBack;
    }

    public bool GetIsInvincible () {
        return (currentStatus & LifeEnum.Status.Invincible) == LifeEnum.Status.Invincible;
    }

    public bool GetIsDying () {
        return (currentStatus & LifeEnum.Status.Dying) == LifeEnum.Status.Dying;
    }

    protected virtual void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        currentStatus = currentStatus | LifeEnum.Status.BeatingBack;
    }

    protected virtual void StopBeatingBack () {
        currentStatus = currentStatus & ~LifeEnum.Status.BeatingBack;
    }

    protected IEnumerator SetInvincible (bool isTempInvincible) {
        StartInvincible ();

        if (!isTempInvincible) {
            yield break;
        }

        yield return new WaitForSeconds (lifeParams.invincibleTime);

        StopInvincible ();
    }

    protected virtual void StartInvincible () {
        lifeCollision.SetLayer (true);
        currentStatus = currentStatus | LifeEnum.Status.Invincible;
    }

    protected virtual void StopInvincible () {
        lifeCollision.SetLayer (false);
        currentStatus = currentStatus & ~LifeEnum.Status.Invincible;
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