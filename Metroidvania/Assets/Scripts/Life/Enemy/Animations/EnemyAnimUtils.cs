using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public class EnemyAnimUtils : MonoBehaviour {
    [SerializeField] private EnemyModelBase _model;
    public EnemyModelBase model => _model;

    [SerializeField] private Rigidbody2D _rb;
    public Rigidbody2D rb => _rb;

    [SerializeField] private Transform _animBaseTransform;
    public Transform animBaseTransform => _animBaseTransform;

    private void Awake () {
        // event handler
        model.facingDirectionChangedEvent += UpdateFacingDirectionAndVelocity;
    }

    private void Start () {
        // Start movement
        switch (model.movementType) {
            case EnemyEnum.MovementType.Walking:
                UpdateHorizontalVelocity (model.facingDirection);
                break;
        }
    }

    private void OnDestroy () {
        model.facingDirectionChangedEvent -= UpdateFacingDirectionAndVelocity;
    }

    #region Movement Related

    private void UpdateFacingDirectionAndVelocity (LifeEnum.HorizontalDirection facingDirection) {
        UpdateFacingDirection (facingDirection);

        switch (model.movementType) {
            case EnemyEnum.MovementType.Walking:
                UpdateHorizontalVelocity (facingDirection);
                break;
            case EnemyEnum.MovementType.Flying:
                // TODO
                break;
            default:
                Log.PrintError ("Not yet implement update velocity method of enemy MovementType : " + model.movementType, LogType.Animation);
                break;

        }
    }

    private void UpdateFacingDirection (LifeEnum.HorizontalDirection facingDirection) {
        var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
        animBaseTransform.localScale = new Vector3 (scale, 1, 1);
    }

    private void UpdateHorizontalVelocity (LifeEnum.HorizontalDirection facingDirection) {
        var scale = (facingDirection == LifeEnum.HorizontalDirection.Right) ? 1 : -1;
        var velocityX = model.enemyParams.movementSpeed * scale;

        rb.velocity = new Vector3 (velocityX, rb.velocity.y);
    }



    #endregion
}