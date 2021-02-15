using HihiFramework.Core;
using UnityEngine;

public class EnemyAnimUtils : MonoBehaviour {
    [SerializeField] private EnemyModelBase _model;
    public EnemyModelBase Model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D RB => _rb;

    [SerializeField] private Transform _animBaseTransform;
    public Transform AnimBaseTransform => _animBaseTransform;

    private void Awake () {
        // event handler
        Model.FacingDirectionChanged += FacingDirectionChangedHandler;
    }

    private void OnDestroy () {
        Model.FacingDirectionChanged -= FacingDirectionChangedHandler;
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
        var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
        var velocityX = Model.Params.MovementSpeed * scale;
        RB.velocity = new Vector3 (velocityX, RB.velocity.y);
    }

    #endregion
}