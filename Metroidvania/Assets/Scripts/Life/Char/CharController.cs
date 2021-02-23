using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour, UserInput.ICharActionActions, UserInput.ICameraMovementActions {

    [SerializeField] private CharCameraParams cameraParams;
    private UserInput userInput;

    // Not In Use
    //public event Action StartedLeft;
    //public event Action StoppedLeft;
    //public event Action StartedRight;
    //public event Action StoppedRight;
    public event Action Tapped;
    public event Action StartedHold;
    public event Action StoppedHold;
    public event Action<CharEnum.LookDirections> Looked;

    private bool isHolding = false;

    private void Awake () {
        userInput = new UserInput ();
        userInput.CharAction.SetCallbacks (this);
        userInput.CameraMovement.SetCallbacks (this);

        SetAllInputActive (true);
    }

    public void SetAllInputActive (bool isActive) {
        SetCharActionInputActive (isActive);
        SetCameraMovementInputActive (isActive);
    }

    public void SetCharActionInputActive (bool isActive) {
        if (isActive) {
            userInput.CharAction.Enable ();
        } else {
            userInput.CharAction.Disable ();
        }
    }

    public void SetCameraMovementInputActive (bool isActive) {
        if (isActive) {
            userInput.CameraMovement.Enable ();
        } else {
            userInput.CameraMovement.Disable ();
        }
    }

    // Not In Use
    //public void OnLeft (InputAction.CallbackContext context) {
    //    Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
    //    if (context.phase == InputActionPhase.Started) {
    //        StartedLeft?.Invoke ();
    //    } else if (context.phase == InputActionPhase.Canceled) {
    //        StoppedLeft?.Invoke ();
    //    }
    //}

    //public void OnRight (InputAction.CallbackContext context) {
    //    Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
    //    if (context.phase == InputActionPhase.Started) {
    //        StartedRight?.Invoke ();
    //    } else if (context.phase == InputActionPhase.Canceled) {
    //        StoppedRight?.Invoke ();
    //    }
    //}

    #region UserInput.ICharActionActions

    public void OnTap (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
        if (context.phase == InputActionPhase.Performed) {
            Tapped?.Invoke ();
        }
    }

    public void OnHold (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
        if (context.phase == InputActionPhase.Performed) {
            isHolding = true;
            StartedHold?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            if (isHolding) {
                StoppedHold?.Invoke ();
            }
            isHolding = false;
        }
    }

    #endregion

    #region UserInput.ICameraMovementActions

    public void OnLook (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);

        if (context.phase == InputActionPhase.Performed) {
            var value = context.ReadValue<Vector2> ();

            var lookDirections = CharEnum.LookDirections.None;
            if (value.x >= cameraParams.LookThreshold) {
                lookDirections = lookDirections | CharEnum.LookDirections.Right;
            } else if (value.x <= -cameraParams.LookThreshold) {
                lookDirections = lookDirections | CharEnum.LookDirections.Left;
            }

            if (value.y >= cameraParams.LookThreshold) {
                lookDirections = lookDirections | CharEnum.LookDirections.Up;
            } else if (value.y <= -cameraParams.LookThreshold) {
                lookDirections = lookDirections | CharEnum.LookDirections.Down;
            }

            Looked?.Invoke (lookDirections);
        } else if (context.phase == InputActionPhase.Canceled) {
            Looked?.Invoke (CharEnum.LookDirections.None);
        }
    }

    #endregion
}