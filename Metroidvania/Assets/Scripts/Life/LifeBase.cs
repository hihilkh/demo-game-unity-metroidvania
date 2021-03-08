using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

public abstract class LifeBase : MapDisposableBase {
    [SerializeField] private Transform _baseTransform;
    protected Transform BaseTransform => _baseTransform;
    [SerializeField] private Transform _displayBaseTransform;
    public Transform DisplayBaseTransform => _displayBaseTransform;
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

    // FireDamage
    private Coroutine fireDamageCoroutine = null;
    private int CurrentFireDamageTriggerCount = 0;

    protected bool IsInitialized { get; private set; } = false;

    #region Initialization related

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
        }

        CurrentHP = TotalHP;
        SetPosAndDirection (pos, direction);
        ForceStopFireDamage ();

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

    #endregion

    protected virtual void Update () {
        var collisionAnalysis = AnalyseCollision ();
        HandleCollision (collisionAnalysis);
    }

    #region HP

    /// <summary>
    /// Hurt the life by <paramref name="dp"/> (damage point) and if hp come to zero, it calls Die ()
    /// </summary>
    /// <returns>isAlive</returns>
    public virtual bool Hurt (int dp, LifeEnum.HorizontalDirection hurtDirection, bool isFireAttack = false) {
        CurrentHP = Mathf.Max (0, CurrentHP - dp);

        if (CurrentHP == 0) {
            Die (hurtDirection);
            return false;
        } else {
            StartBeatingBack (hurtDirection);
            if (InvinciblePeriod > 0) {
                StartCoroutine (SetInvincible (true));
            }

            if (isFireAttack) {
                StartFireDamage ();
            }

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

    private void StartFireDamage () {
        CurrentFireDamageTriggerCount = 0;

        if (fireDamageCoroutine == null) {
            fireDamageCoroutine = StartCoroutine (FireDamageCoroutine ());
        }
    }

    private IEnumerator FireDamageCoroutine () {
        while (CurrentFireDamageTriggerCount < GameVariable.FireAttackNoOfTrigger) {
            yield return new WaitForSeconds (GameVariable.FireAttackTriggerPeriod);

            CurrentFireDamageTriggerCount++;

            CurrentHP = Mathf.Max (0, CurrentHP - GameVariable.FireDamagePerTrigger);
            if (CurrentHP == 0) {
                ForceStopFireDamage ();
                Die (LifeEnum.HorizontalDirection.Right);
            }
        }

        fireDamageCoroutine = null;
        CurrentFireDamageTriggerCount = 0;
    }

    private void ForceStopFireDamage () {
        if (fireDamageCoroutine != null) {
            StopCoroutine (fireDamageCoroutine);
            fireDamageCoroutine = null;
        }

        CurrentFireDamageTriggerCount = 0;
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

    protected class CollisionAnalysis {
        public Dictionary<LifeEnum.CollisionType, List<LifeCollision.CollisionDetails>> CollisionDetailsDict { get; }
        public LifeEnum.CollisionChangedType GroundCollisionChangedType { get; }
        public LifeEnum.CollisionChangedType WallCollisionChangedType { get; }

        public CollisionAnalysis (Dictionary<LifeEnum.CollisionType, List<LifeCollision.CollisionDetails>> collisionDetailsDict, LifeEnum.CollisionChangedType groundCollisionChangedType, LifeEnum.CollisionChangedType wallCollisionChangedType) {
            CollisionDetailsDict = collisionDetailsDict;
            GroundCollisionChangedType = groundCollisionChangedType;
            WallCollisionChangedType = wallCollisionChangedType;
        }
    }

    private bool isTouchingGround = false;
    private bool isTouchingWall = false;

    private CollisionAnalysis AnalyseCollision () {
        var collisionDetailsDict = CollisionScript.GetCollisionDetailsDict ();

        var isNowTouchingGround = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Ground);
        var isNowTouchingWall = collisionDetailsDict.ContainsKey (LifeEnum.CollisionType.Wall);

        var groundCollisionChangedType = LifeEnum.CollisionChangedType.None;
        if (isTouchingGround && !isNowTouchingGround) {
            groundCollisionChangedType = LifeEnum.CollisionChangedType.Exit;
        } else if (!isTouchingGround && isNowTouchingGround) {
            groundCollisionChangedType = LifeEnum.CollisionChangedType.Enter;
        }

        var wallCollisionChangedType = LifeEnum.CollisionChangedType.None;
        if (isTouchingWall && !isNowTouchingWall) {
            wallCollisionChangedType = LifeEnum.CollisionChangedType.Exit;
        } else if (!isTouchingWall && isNowTouchingWall) {
            wallCollisionChangedType = LifeEnum.CollisionChangedType.Enter;
        }

        isTouchingGround = isNowTouchingGround;
        isTouchingWall = isNowTouchingWall;

        return new CollisionAnalysis (collisionDetailsDict, groundCollisionChangedType, wallCollisionChangedType);
    }

    protected abstract void HandleCollision (CollisionAnalysis collisionAnalysis);

    #endregion
}