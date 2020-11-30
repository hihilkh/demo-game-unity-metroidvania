using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;
using UnityEngine.InputSystem;
using System;

public class CharacterController : MonoBehaviour, UserInput.ICharacterActions {

    private UserInput userInput;

    public event Action StartedLeft;
    public event Action StoppedLeft;
    public event Action StartedRight;
    public event Action StoppedRight;
    public event Action Tapped;
    public event Action StartedHold;
    public event Action StoppedHold;

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
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase);
        if (context.phase == InputActionPhase.Started) {
            StartedLeft?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedLeft?.Invoke ();
        }
    }

    public void OnRight (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase);
        if (context.phase == InputActionPhase.Started) {
            StartedRight?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedRight?.Invoke ();
        }
    }

    public void OnTap (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase);
        if (context.phase == InputActionPhase.Performed) {
            Tapped?.Invoke ();
        }
    }

    public void OnHold (InputAction.CallbackContext context) {
        Log.PrintDebug ("Action name : " + context.action.name + " , Phase : " + context.phase);
        if (context.phase == InputActionPhase.Performed) {
            StartedHold?.Invoke ();
        } else if (context.phase == InputActionPhase.Canceled) {
            StoppedHold?.Invoke ();
        }
    }
}