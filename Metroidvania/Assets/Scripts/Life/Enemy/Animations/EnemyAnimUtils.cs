using HihiFramework.Core;
using UnityEngine;

public class EnemyAnimUtils : MonoBehaviour {
    [SerializeField] private EnemyModelBase _model;
    public EnemyModelBase Model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D RB => _rb;

    [SerializeField] private Transform _animBaseTransform;
    public Transform AnimBaseTransform => _animBaseTransform;

    private float maxMovementSpeedSqr;
    private bool isAccelerating = false;
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
        if (isAccelerating) {
            var acceleration = Model.Params.Acceleration * acceleratingDirection;
            Log.Print (acceleration);
            RB.AddForce (acceleration, ForceMode2D.Force);

            if (RB.velocity.sqrMagnitude > maxMovementSpeedSqr) {
                RB.velocity = Vector2.ClampMagnitude (RB.velocity, Model.Params.MaxMovementSpeed);
            }
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
        if (Model.Params.IsSpeedAccelerate) {
            isAccelerating = true;
            acceleratingDirection = (facingDirection == LifeEnum.HorizontalDirection.Right) ? Vector2.right : Vector2.left;
        } else {
            isAccelerating = false;
            var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
            var velocityX = Model.Params.MaxMovementSpeed * scale;
            RB.velocity = new Vector2 (velocityX, RB.velocity.y);
        }
    }

    public void SetIdleVelocity () {
        isAccelerating = false;
        RB.velocity = Vector2.zero;
    }

    #endregion
}