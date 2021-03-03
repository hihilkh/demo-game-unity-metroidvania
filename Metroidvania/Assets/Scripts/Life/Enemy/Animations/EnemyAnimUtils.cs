using HihiFramework.Core;
using UnityEngine;

public class EnemyAnimUtils : MonoBehaviour {
    private enum AccelerationMode {
        None,
        Direction,
        ToIdle,
    }

    [SerializeField] private EnemyModelBase _model;
    public EnemyModelBase Model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D RB => _rb;

    [SerializeField] private Transform _animBaseTransform;
    public Transform AnimBaseTransform => _animBaseTransform;

    private float maxMovementSpeedSqr;
    private AccelerationMode currentAccelerationMode = AccelerationMode.None;

    private const float NearZeroSpeedSqr = 0.2F;

    /// <summary>
    /// It is assumed to be nomalized
    /// </summary>
    private Vector2 acceleratingDirection;

    private void Awake () {
        // event handler
        Model.FacingDirectionChanged += FacingDirectionChangedHandler;

        maxMovementSpeedSqr = Model.Params.MaxMovementSpeed * Model.Params.MaxMovementSpeed;
    }

    private void OnDestroy () {
        Model.FacingDirectionChanged -= FacingDirectionChangedHandler;
    }

    private void FixedUpdate () {
        switch (currentAccelerationMode) {
            case AccelerationMode.Direction:
                var acceleration = Model.Params.Acceleration * acceleratingDirection;
                RB.AddForce (acceleration, ForceMode2D.Force);

                if (RB.velocity.sqrMagnitude > maxMovementSpeedSqr) {
                    RB.velocity = Vector2.ClampMagnitude (RB.velocity, Model.Params.MaxMovementSpeed);
                }
                break;
            case AccelerationMode.ToIdle:
                RB.velocity = RB.velocity * Model.Params.DecelerationFactor;

                if (RB.velocity.sqrMagnitude <= NearZeroSpeedSqr) {
                    RB.velocity = Vector2.zero;
                    currentAccelerationMode = AccelerationMode.None;
                }
                break;
        }
    }

    #region Movement Related

    private void FacingDirectionChangedHandler (LifeEnum.HorizontalDirection facingDirection) {
        UpdateFacingDirection (facingDirection);

        switch (Model.MovementType) {
            case EnemyEnum.MovementType.Walking:
                // if beating back, just leave the velocity controlled by physics
                if (!Model.IsBeatingBack) {
                    UpdateHorizontalVelocity (facingDirection);
                }
                break;
            case EnemyEnum.MovementType.Flying:
                // TODO
                break;
            default:
                Log.PrintError ("Not yet implement update velocity method of enemy MovementType : " + Model.MovementType, LogTypes.Animation);
                break;

        }
    }

    private void UpdateFacingDirection (LifeEnum.HorizontalDirection facingDirection) {
        var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
        AnimBaseTransform.localScale = new Vector3 (scale, 1, 1);
    }

    public void UpdateHorizontalVelocity () {
        UpdateHorizontalVelocity (Model.FacingDirection);
    }

    private void UpdateHorizontalVelocity (LifeEnum.HorizontalDirection facingDirection) {
        var targetDir = (facingDirection == LifeEnum.HorizontalDirection.Right) ? Vector2.right : Vector2.left;
        UpdateVelocity (targetDir, true);
    }

    /// <param name="targetDir">It is assumed to be nomalized</param>
    public void UpdateVelocity (Vector2 targetDir) {
        UpdateVelocity (targetDir, false);
    }

    /// <param name="targetDir">It is assumed to be nomalized</param>
    private void UpdateVelocity (Vector2 targetDir, bool isUpdateHorizontalOnly) {
        if (Model.Params.IsSpeedAccelerate) {
            currentAccelerationMode = AccelerationMode.Direction;
            acceleratingDirection = targetDir;
        } else {
            currentAccelerationMode = AccelerationMode.None;
            var velocity = Model.Params.MaxMovementSpeed * targetDir;
            if (isUpdateHorizontalOnly) {
                RB.velocity = new Vector2 (velocity.x, RB.velocity.y);
            } else {
                RB.velocity = velocity;
            }
        }
    }

    public void StartUpdateToIdleVelocity () {
        if (Model.Params.IsSpeedAccelerate) {
            currentAccelerationMode = AccelerationMode.ToIdle;
        } else {
            currentAccelerationMode = AccelerationMode.None;
            RB.velocity = Vector2.zero;
        }
    }

    #endregion
}