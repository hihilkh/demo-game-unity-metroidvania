using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public class CharMovementSMBBase : CharSMBBase {
    protected Rigidbody2D rb { get; private set; }
    private float originalGravityScale;

    override public void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter (animator, stateInfo, layerIndex);

        // rigid body
        if (rb == null) {
            rb = animator.GetComponentInParent<Rigidbody2D> ();

            if (rb == null) {
                Log.PrintError ("Cannot find corresponding Rigidbody2D.");
            } else {
                originalGravityScale = rb.gravityScale;
            }
        }
    }

    protected void UpdateHorizontalVelocity () {
        var multiplier = (model.movingDirection == CharEnum.Direction.Right) ? 1 : -1;

        var horizontalSpeed = 0f;

        switch (model.currentHorizontalSpeed) {
            case CharEnum.HorizontalSpeed.Zero:
                horizontalSpeed = 0;
                break;
            case CharEnum.HorizontalSpeed.Walk:
                horizontalSpeed = model.characterParams.walkingSpeed;
                break;
            case CharEnum.HorizontalSpeed.Dash:
                horizontalSpeed = model.characterParams.dashingSpeed;
                break;
        }

        rb.velocity = new Vector3 (horizontalSpeed * multiplier, rb.velocity.y);

        // TODO : Debug usage only
        if (Mathf.Abs (rb.velocity.x) < 1 && Mathf.Abs (rb.velocity.y) < 1) {
            Log.PrintError ("No velocity! ; horizontal speed = " + model.currentHorizontalSpeed);
            //Log.PrintError ("No velocity! Situation = " + model.situation + " ; horizontal speed = " + model.currentHorizontalSpeed);
        }
    }

    protected void UpdateFacingDirection () {
        var scale = (model.facingDirection == CharEnum.Direction.Right) ? 1 : -1;
        rb.transform.localScale = new Vector3 (scale, 1, 1);
    }

    protected void ResetGravity () {
        rb.gravityScale = originalGravityScale;
    }

    protected void RemoveGravity () {
        rb.gravityScale = 0;
    }
}
