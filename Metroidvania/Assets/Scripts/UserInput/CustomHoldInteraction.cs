using System.ComponentModel;
using HihiFramework.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting;

/// <summary>
/// This custom script is to prevent the error of original script that
/// if triggering start hold and release hold at the same frame, the release trigger will be ignored and the hold input is keep here until next tap / hold input.
/// </summary>
[Preserve]
[DisplayName ("CustomHold")]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class CustomHoldInteraction : IInputInteraction {
    /// <summary>
    /// Duration in seconds that the control must be pressed for the hold to register.
    /// </summary>
    /// <remarks>
    /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultHoldTime"/> is used.
    ///
    /// Duration is expressed in real time and measured against the timestamps of input events
    /// (<see cref="LowLevel.InputEvent.time"/>) not against game time (<see cref="Time.time"/>).
    /// </remarks>
    public float holdTime = InputSystem.settings.defaultHoldTime;

    /// <summary>
    /// Magnitude threshold that must be crossed by an actuated control for the control to
    /// be considered pressed.
    /// </summary>
    /// <remarks>
    /// If this is less than or equal to 0 (the default), <see cref="InputSettings.defaultButtonPressPoint"/> is used instead.
    /// </remarks>
    /// <seealso cref="InputControl.EvaluateMagnitude()"/>
    public float pressPoint = InputSystem.settings.defaultButtonPressPoint;

    private float durationOrDefault => holdTime > 0.0 ? holdTime : InputSystem.settings.defaultHoldTime;
    private float pressPointOrDefault => pressPoint > 0.0 ? pressPoint : InputSystem.settings.defaultButtonPressPoint;// ButtonControl.s_GlobalDefaultButtonPressPoint;

    private double m_TimePressed;

    /// <inheritdoc />
    public void Process (ref InputInteractionContext context) {
        if (context.timerHasExpired) {
            context.PerformedAndStayPerformed ();
            return;
        }

        switch (context.phase) {
            case InputActionPhase.Waiting:
                if (context.ControlIsActuated (pressPointOrDefault)) {
                    m_TimePressed = context.time;

                    context.Started ();
                    context.SetTimeout (durationOrDefault);
                }
                break;

            case InputActionPhase.Started:
                // If we've reached our hold time threshold, perform the hold.
                // We do this regardless of what state the control changed to.
                if (context.time - m_TimePressed >= durationOrDefault) {
                    if (context.control.IsPressed ()) {
                        // Normal start hold situation
                        context.PerformedAndStayPerformed ();
                    } else {
                        Log.PrintWarning ("Seems start holding and release holding is triggered at the same frame. Treat as failed to start hold.", LogTypes.Input);
                        context.Canceled ();
                    }
                } else if (!context.ControlIsActuated ()) {
                    // Control is no longer actuated and we haven't performed a hold yet,
                    // so cancel.
                    context.Canceled ();
                }
                break;

            case InputActionPhase.Performed:
                if (!context.ControlIsActuated (pressPointOrDefault))
                    context.Canceled ();
                break;
        }
    }

    /// <inheritdoc />
    public void Reset () {
        m_TimePressed = 0;
    }

    static CustomHoldInteraction () {
        InputSystem.RegisterInteraction<CustomHoldInteraction> ();
    }

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize () {
        // Will execute the static constructor as a side effect.
    }
}