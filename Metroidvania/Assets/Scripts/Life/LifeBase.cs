using System.Collections;
using HihiFramework.Core;
using UnityEngine;

public abstract class LifeBase : MapDisposableBase {
    [SerializeField] private Transform _baseTransform;
    protected Transform BaseTransform => _baseTransform;
    [SerializeField] private LifeCollision _collisionScript;
    protected LifeCollision CollisionScript => _collisionScript;
    [SerializeField] private LifeHPView hpView;
    public virtual LifeEnum.HorizontalDirection FacingDirection { get; protected set; }
    [SerializeField] public virtual LifeEnum.Location CurrentLocation { get; protected set; }

    protected abstract int PosZ { get; }
    protected abstract int InvincibleLayer { get; }
    protected abstract int TotalHP { get; }

    private int _currentHP;
    protected int CurrentHP {
        get { return _currentHP; }
        set {
            if (_currentHP != value) {
                _currentHP = value;
                hpView.UpdateView (TotalHP, value);
            }
        }
    }

    // Status
    public abstract bool IsBeatingBack { get; protected set; }
    public abstract bool IsInvincible { get; protected set; }
    public abstract bool IsDying { get; protected set; }

    protected abstract float InvinciblePeriod { get; }

    protected bool IsInitialized { get; private set; } = false;

    protected override void OnDestroy () {
        base.OnDestroy ();

        if (IsInitialized) {
            RemoveCollisionEventHandlers ();
        }
    }

    /// <summary>
    /// If not yet initialized, it will initialize. Otherwise, it will reset.
    /// </summary>
    /// <returns>has initialized before</returns>
    protected virtual bool Reset (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        var hasInitializedBefore = false;
        if (IsInitialized) {
            Log.Print (gameObject.name + " : Reset", LogTypes.Life);
            hasInitializedBefore = true;
        } else {
            Log.Print (gameObject.name + " : Initialize", LogTypes.Life);
            IsInitialized = true;

            CollisionScript.Init (InvincibleLayer);
            AddCollisionEventHandlers ();
        }

        CurrentHP = TotalHP;
        SetPosAndDirection (pos, direction);

        return hasInitializedBefore;
    }

    // TODO : Handle the cases that change pos after init
    protected virtual void SetPosAndDirection (Vector2 pos, LifeEnum.HorizontalDirection direction) {
        SetPos (pos);
        FacingDirection = direction;

        StartCoroutine (ResetCurrentLocation ());
    }

    public virtual void SetPos (Vector2 pos) {
        BaseTransform.position = new Vector3 (pos.x, pos.y, PosZ);
    }

    public virtual Vector3 GetPos () {
        return BaseTransform.position;
    }

    public virtual void SetPosByOffset (Vector2 offset) {
        BaseTransform.position += (Vector3)offset;
    }

    #region HP

    /// <summary>
    /// Hurt the life by <paramref name="dp"/> (damage point) and if hp come to zero, it calls Die ()
    /// </summary>
    /// <returns>isAlive</returns>
    public virtual bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection) {
        CurrentHP = Mathf.Max (0, CurrentHP - dp);

        if (CurrentHP == 0) {
            Die (hurtDirection);
            return false;
        } else {
            StartBeatingBack (hurtDirection);
            StartCoroutine (SetInvincible (true));

            return true;
        }
    }

    protected virtual void Die (LifeEnum.HorizontalDirection dieDirection) {
        CurrentHP = 0;
        IsDying = true;

        StartBeatingBack (dieDirection);
        StartCoroutine (SetInvincible (false));
    }

    protected virtual void StartBeatingBack (LifeEnum.HorizontalDirection hurtDirection) {
        IsBeatingBack = true;
    }

    protected virtual void StopBeatingBack () {
        IsBeatingBack = false;
    }

    protected IEnumerator SetInvincible (bool isTempInvincible) {
        StartInvincible ();

        if (!isTempInvincible) {
            yield break;
        }

        yield return new WaitForSeconds (InvinciblePeriod);

        StopInvincible ();
    }

    protected virtual void StartInvincible () {
        CollisionScript.SetLayer (true);
        IsInvincible = true;
    }

    protected virtual void StopInvincible () {
        CollisionScript.SetLayer (false);
        IsInvincible = false;
    }

    #endregion

    #region Location

    protected virtual IEnumerator ResetCurrentLocation () {
        CurrentLocation = LifeEnum.Location.Unknown;

        yield return null;

        // Wait for a frame and see if currentLocation has already been assigned (e.g. by collision event).
        // If not, assume it is in air.
        if (CurrentLocation == LifeEnum.Location.Unknown) {
            if (CollisionScript.CheckIsTouchingGround ()) {
                CurrentLocation = LifeEnum.Location.Ground;
            } else {
                CurrentLocation = LifeEnum.Location.Air;
            }
        }
    }

    #endregion

    #region Collision

    protected abstract void AddCollisionEventHandlers ();
    protected abstract void RemoveCollisionEventHandlers ();

    #endregion
}