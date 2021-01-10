using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using UnityEngine.InputSystem;
using System;

public class CharController : MonoBehaviour, UserInput.ICharacterActions {

    private UserInput userInput;

    public event Action StartedLeftEvent;
    public event Action StoppedLeftEvent;
    public event Action StartedRightEvent;
    public event Action StoppedRightEvent;
    public event Action StartedPressEvent;
    public event Action TappedEvent;
    public event Action StartedHoldEvent;
    public event Action StoppedHoldEvent;

    private bool isHolding = false;

    void OnEnable () {
        if (userInput == null) {
            userInput = new UserInput ();
            userInput.Character.SetCallbacks (this);
        }

        userInput.Character.Enable ();
    }

    void OnDisable () {
        if (userInput != null) {
            userInput.Character.Disable ();
        }
    }

    public void OnLeft (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogType.Input);
        if (context.phase == InputActionPhase.Started) {
            StartedLeftEvent?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedLeftEvent?.Invoke ();
        }
    }

    public void OnRight (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogType.Input);
        if (context.phase == InputActionPhase.Started) {
            StartedRightEvent?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedRightEvent?.Invoke ();
        }
    }

    public void OnTap (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogType.Input);
        if (context.phase == InputActionPhase.Performed) {
            TappedEvent?.Invoke ();
        } else if (context.phase == InputActionPhase.Started) {
            StartedPressEvent?.Invoke ();
        }
    }

    public void OnHold (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase, LogType.Input);
        if (context.phase == InputActionPhase.Performed) {
            isHolding = true;
            StartedHoldEvent?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            if (isHolding) {
                StoppedHoldEvent?.Invoke ();
            }
            isHolding = false;
        }
    }
}