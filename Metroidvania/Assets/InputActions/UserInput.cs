// GENERATED AUTOMATICALLY FROM 'Assets/InputActions/UserInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @UserInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @UserInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""UserInput"",
    ""maps"": [
        {
            ""name"": ""CharAction"",
            ""id"": ""3e25b07e-430e-49c3-8419-b10ed7a35ab3"",
            ""actions"": [
                {
                    ""name"": ""Tap"",
                    ""type"": ""Button"",
                    ""id"": ""595f3166-015c-4aef-9b8e-c4092407bef9"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap""
                },
                {
                    ""name"": ""Hold"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b372810b-5ee8-46a5-8545-109327f921ed"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3a590e88-ad79-4692-85fa-8a4399570fcb"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""632f5d12-29ff-4b75-acfc-59b8853a6181"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Tap"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""13cbd21c-6111-42dc-80f7-0aa9eef5d2ad"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2f3d93fa-865b-4562-b085-abc58f4630f3"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""CameraMovement"",
            ""id"": ""5a410ea8-fb55-4f39-941d-604e38279bb6"",
            ""actions"": [
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""2cae8e21-7ebf-4c5e-a5f1-10de5d76f85f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""Dpad"",
                    ""id"": ""9d4c9ebd-751c-4767-a157-7f99c9929acb"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""37d1f05f-6bdb-482a-bc5d-1b20d826699c"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""4e50864f-3cff-4cde-aa42-c83d4a7e06af"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""56850f81-77c6-43aa-8b87-f93cec3cbf6a"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""35179f1a-4a62-427a-90ee-6edca9beef06"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c728026b-8565-4c25-bba1-afb97709a947"",
                    ""path"": ""<Gamepad>/leftStick"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""NotInUse"",
            ""id"": ""42d44bae-1405-4aec-8418-32a4d9efb517"",
            ""actions"": [
                {
                    ""name"": ""Left"",
                    ""type"": ""Button"",
                    ""id"": ""ab1ecac5-bbe1-4225-ae32-c5cbab64ef58"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Right"",
                    ""type"": ""Button"",
                    ""id"": ""d93d08da-7e7f-4441-b39d-baf9ad1afe8c"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""87bfd08e-5aed-48b7-8c0e-689f82fe2856"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f95d97cf-c3b0-47ff-b1e5-232b52a7f4c2"",
                    ""path"": ""<Keyboard>/g"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // CharAction
        m_CharAction = asset.FindActionMap("CharAction", throwIfNotFound: true);
        m_CharAction_Tap = m_CharAction.FindAction("Tap", throwIfNotFound: true);
        m_CharAction_Hold = m_CharAction.FindAction("Hold", throwIfNotFound: true);
        // CameraMovement
        m_CameraMovement = asset.FindActionMap("CameraMovement", throwIfNotFound: true);
        m_CameraMovement_Look = m_CameraMovement.FindAction("Look", throwIfNotFound: true);
        // NotInUse
        m_NotInUse = asset.FindActionMap("NotInUse", throwIfNotFound: true);
        m_NotInUse_Left = m_NotInUse.FindAction("Left", throwIfNotFound: true);
        m_NotInUse_Right = m_NotInUse.FindAction("Right", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // CharAction
    private readonly InputActionMap m_CharAction;
    private ICharActionActions m_CharActionActionsCallbackInterface;
    private readonly InputAction m_CharAction_Tap;
    private readonly InputAction m_CharAction_Hold;
    public struct CharActionActions
    {
        private @UserInput m_Wrapper;
        public CharActionActions(@UserInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Tap => m_Wrapper.m_CharAction_Tap;
        public InputAction @Hold => m_Wrapper.m_CharAction_Hold;
        public InputActionMap Get() { return m_Wrapper.m_CharAction; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CharActionActions set) { return set.Get(); }
        public void SetCallbacks(ICharActionActions instance)
        {
            if (m_Wrapper.m_CharActionActionsCallbackInterface != null)
            {
                @Tap.started -= m_Wrapper.m_CharActionActionsCallbackInterface.OnTap;
                @Tap.performed -= m_Wrapper.m_CharActionActionsCallbackInterface.OnTap;
                @Tap.canceled -= m_Wrapper.m_CharActionActionsCallbackInterface.OnTap;
                @Hold.started -= m_Wrapper.m_CharActionActionsCallbackInterface.OnHold;
                @Hold.performed -= m_Wrapper.m_CharActionActionsCallbackInterface.OnHold;
                @Hold.canceled -= m_Wrapper.m_CharActionActionsCallbackInterface.OnHold;
            }
            m_Wrapper.m_CharActionActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Tap.started += instance.OnTap;
                @Tap.performed += instance.OnTap;
                @Tap.canceled += instance.OnTap;
                @Hold.started += instance.OnHold;
                @Hold.performed += instance.OnHold;
                @Hold.canceled += instance.OnHold;
            }
        }
    }
    public CharActionActions @CharAction => new CharActionActions(this);

    // CameraMovement
    private readonly InputActionMap m_CameraMovement;
    private ICameraMovementActions m_CameraMovementActionsCallbackInterface;
    private readonly InputAction m_CameraMovement_Look;
    public struct CameraMovementActions
    {
        private @UserInput m_Wrapper;
        public CameraMovementActions(@UserInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Look => m_Wrapper.m_CameraMovement_Look;
        public InputActionMap Get() { return m_Wrapper.m_CameraMovement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraMovementActions set) { return set.Get(); }
        public void SetCallbacks(ICameraMovementActions instance)
        {
            if (m_Wrapper.m_CameraMovementActionsCallbackInterface != null)
            {
                @Look.started -= m_Wrapper.m_CameraMovementActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_CameraMovementActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_CameraMovementActionsCallbackInterface.OnLook;
            }
            m_Wrapper.m_CameraMovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
            }
        }
    }
    public CameraMovementActions @CameraMovement => new CameraMovementActions(this);

    // NotInUse
    private readonly InputActionMap m_NotInUse;
    private INotInUseActions m_NotInUseActionsCallbackInterface;
    private readonly InputAction m_NotInUse_Left;
    private readonly InputAction m_NotInUse_Right;
    public struct NotInUseActions
    {
        private @UserInput m_Wrapper;
        public NotInUseActions(@UserInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Left => m_Wrapper.m_NotInUse_Left;
        public InputAction @Right => m_Wrapper.m_NotInUse_Right;
        public InputActionMap Get() { return m_Wrapper.m_NotInUse; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(NotInUseActions set) { return set.Get(); }
        public void SetCallbacks(INotInUseActions instance)
        {
            if (m_Wrapper.m_NotInUseActionsCallbackInterface != null)
            {
                @Left.started -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnLeft;
                @Left.performed -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnLeft;
                @Left.canceled -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnLeft;
                @Right.started -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnRight;
                @Right.performed -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnRight;
                @Right.canceled -= m_Wrapper.m_NotInUseActionsCallbackInterface.OnRight;
            }
            m_Wrapper.m_NotInUseActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Left.started += instance.OnLeft;
                @Left.performed += instance.OnLeft;
                @Left.canceled += instance.OnLeft;
                @Right.started += instance.OnRight;
                @Right.performed += instance.OnRight;
                @Right.canceled += instance.OnRight;
            }
        }
    }
    public NotInUseActions @NotInUse => new NotInUseActions(this);
    public interface ICharActionActions
    {
        void OnTap(InputAction.CallbackContext context);
        void OnHold(InputAction.CallbackContext context);
    }
    public interface ICameraMovementActions
    {
        void OnLook(InputAction.CallbackContext context);
    }
    public interface INotInUseActions
    {
        void OnLeft(InputAction.CallbackContext context);
        void OnRight(InputAction.CallbackContext context);
    }
}
