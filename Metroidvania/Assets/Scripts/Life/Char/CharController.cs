using System;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour, UserInput.ICharacterActions {

    [SerializeField] private CharCameraParams cameraParams;
    private UserInput userInput;

    public event Action StartedLeft;
    public event Action StoppedLeft;
    public event Action StartedRight;
    public event Action StoppedRight;
    public event Action Tapped;
    public event Action StartedHold;
    public event Action StoppedHold;
    public event Action<CharEnum.LookDirections> Looked;

    private bool isHolding = false;

    void OnEnable () {
        if (userInput == null) {
            userInput = new UserInput ();
            userInput.Character.SetCallbacks (this);
        }

        userInput.Character.Enable ();
    }

    void OnDisable () {
        userInput?.Character.Disable ();
    }

    public void OnLeft (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
        if (context.phase == InputActionPhase.Started) {
            StartedLeft?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedLeft?.Invoke ();
        }
    }

    public void OnRight (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogTypes.Input);
        if (context.phase == InputActionPhase.Started) {
            StartedRight?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedRight?.Invoke ();
        }
    }

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
}