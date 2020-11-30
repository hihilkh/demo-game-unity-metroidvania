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
            ""name"": ""Character"",
            ""id"": ""5a410ea8-fb55-4f39-941d-604e38279bb6"",
            ""actions"": [
                {
                    ""name"": ""Left"",
                    ""type"": ""Button"",
                    ""id"": ""87bad1ca-71d6-40be-ac0e-7f91fe0d8877"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Right"",
                    ""type"": ""Button"",
                    ""id"": ""ca8761d2-ebb7-4e5d-b48b-d11e4429437f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Tap"",
                    ""type"": ""Button"",
                    ""id"": ""da240b9c-66e1-4cbb-a107-64a5770ac52d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Tap""
                },
                {
                    ""name"": ""Hold"",
                    ""type"": ""Button"",
                    ""id"": ""ff56f1c7-4dc4-4fae-829e-85e859c63c5a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Hold""
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""76004bcd-a7b6-43b1-b632-82c757f62ac8"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Left"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1f3a251d-14cf-4fce-a43a-69339f96dc1e"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Right"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""f09a7ae3-20ad-4b78-96f2-96e333d24a85"",
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
                    ""id"": ""145f8015-595b-4109-b111-39850daab7a7"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hold"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Character
        m_Character = asset.FindActionMap("Character", throwIfNotFound: true);
        m_Character_Left = m_Character.FindAction("Left", throwIfNotFound: true);
        m_Character_Right = m_Character.FindAction("Right", throwIfNotFound: true);
        m_Character_Tap = m_Character.FindAction("Tap", throwIfNotFound: true);
        m_Character_Hold = m_Character.FindAction("Hold", throwIfNotFound: true);
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

    // Character
    private readonly InputActionMap m_Character;
    private ICharacterActions m_CharacterActionsCallbackInterface;
    private readonly InputAction m_Character_Left;
    private readonly InputAction m_Character_Right;
    private readonly InputAction m_Character_Tap;
    private readonly InputAction m_Character_Hold;
    public struct CharacterActions
    {
        private @UserInput m_Wrapper;
        public CharacterActions(@UserInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Left => m_Wrapper.m_Character_Left;
        public InputAction @Right => m_Wrapper.m_Character_Right;
        public InputAction @Tap => m_Wrapper.m_Character_Tap;
        public InputAction @Hold => m_Wrapper.m_Character_Hold;
        public InputActionMap Get() { return m_Wrapper.m_Character; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CharacterActions set) { return set.Get(); }
        public void SetCallbacks(ICharacterActions instance)
        {
            if (m_Wrapper.m_CharacterActionsCallbackInterface != null)
            {
                @Left.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnLeft;
                @Left.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnLeft;
                @Left.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnLeft;
                @Right.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnRight;
                @Right.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnRight;
                @Right.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnRight;
                @Tap.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnTap;
                @Tap.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnTap;
                @Tap.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnTap;
                @Hold.started -= m_Wrapper.m_CharacterActionsCallbackInterface.OnHold;
                @Hold.performed -= m_Wrapper.m_CharacterActionsCallbackInterface.OnHold;
                @Hold.canceled -= m_Wrapper.m_CharacterActionsCallbackInterface.OnHold;
            }
            m_Wrapper.m_CharacterActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Left.started += instance.OnLeft;
                @Left.performed += instance.OnLeft;
                @Left.canceled += instance.OnLeft;
                @Right.started += instance.OnRight;
                @Right.performed += instance.OnRight;
                @Right.canceled += instance.OnRight;
                @Tap.started += instance.OnTap;
                @Tap.performed += instance.OnTap;
                @Tap.canceled += instance.OnTap;
                @Hold.started += instance.OnHold;
                @Hold.performed += instance.OnHold;
                @Hold.canceled += instance.OnHold;
            }
        }
    }
    public CharacterActions @Character => new CharacterActions(this);
    public interface ICharacterActions
    {
        void OnLeft(InputAction.CallbackContext context);
        void OnRight(InputAction.CallbackContext context);
        void OnTap(InputAction.CallbackContext context);
        void OnHold(InputAction.CallbackContext context);
    }
}
