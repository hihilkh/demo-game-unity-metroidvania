using UnityEngine;

public class EnemyAnimSMBUtils : MonoBehaviour {
    private enum AccelerationMode {
        None,
        Direction,
        ToIdle,
    }

    [SerializeField] private EnemyModelBase _model;
    public EnemyModelBase Model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D RB => _rb;

    // Acceleration
    private AccelerationMode currentAccelerationMode = AccelerationMode.None;
    private Vector2 acceleratingVector;
    private float acceleratingMaxSpeed;
    private float acceleratingMaxSpeedSqr;
    private const float NearZeroSpeedSqr = 0.2F;

    private void Awake () {
        // event handler
        Model.FacingDirectionChanged += FacingDirectionChangedHandler;
        if (Model.MovementType == EnemyEnum.MovementType.Flying) {
            Model.FlyingInfoUpdated += FlyingInfoUpdatedHandler;
        }
    }

    private void OnDestroy () {
        Model.FacingDirectionChanged -= FacingDirectionChangedHandler;
        if (Model.MovementType == EnemyEnum.MovementType.Flying) {
            Model.FlyingInfoUpdated -= FlyingInfoUpdatedHandler;
        }
    }

    private void FixedUpdate () {
        switch (currentAccelerationMode) {
            case AccelerationMode.Direction:
                RB.AddForce (acceleratingVector, ForceMode2D.Force);

                if (RB.velocity.sqrMagnitude > acceleratingMaxSpeedSqr) {
                    RB.velocity = Vector2.ClampMagnitude (RB.velocity, acceleratingMaxSpeed);
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
            default:
                // do nothing
                break;
        }
    }

    private void UpdateFacingDirection (LifeEnum.HorizontalDirection facingDirection) {
        var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
        Model.DisplayBaseTransform.localScale = new Vector3 (scale, 1, 1);
    }

    private void FlyingInfoUpdatedHandler (Vector2 normalizedChasingDirection, float maxSpeed, float? acceleration) {
        UpdateVelocity (false, normalizedChasingDirection, maxSpeed, acceleration);
    }

    public void UpdateHorizontalVelocity () {
        UpdateHorizontalVelocity (Model.FacingDirection);
    }

    private void UpdateHorizontalVelocity (LifeEnum.HorizontalDirection facingDirection) {
        var targetDir = (facingDirection == LifeEnum.HorizontalDirection.Right) ? Vector2.right : Vector2.left;

        float? acceleration = null;
        if (Model.Params.IsSpeedAccelerate) {
            acceleration = Model.Params.Acceleration;
        }
        UpdateVelocity (true, targetDir, Model.Params.MaxMovementSpeed, acceleration);
    }

    private void UpdateVelocity (bool isUpdateHorizontalOnly, Vector2 targetNormalizedDir, float maxSpeed, float? acceleration) {
        if (acceleration != null) {
            currentAccelerationMode = AccelerationMode.Direction;
            acceleratingVector = (float)acceleration * targetNormalizedDir;
            acceleratingMaxSpeed = maxSpeed;
            acceleratingMaxSpeedSqr = maxSpeed * maxSpeed;
        } else {
            currentAccelerationMode = AccelerationMode.None;
            var velocity = maxSpeed * targetNormalizedDir;
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