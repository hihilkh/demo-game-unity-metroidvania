using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandDisplay : Selectable {

    public enum DisplayType {
        Picker,
        Container,
        Display,
    }

    [SerializeField] private Image baseImage;
    [SerializeField] private Image commandImage;
    [SerializeField] private HIHIButton crossBtn;
    [SerializeField] private GameObject targetEffect;
    [SerializeField] private List<Graphic> targetRaycastList;

    public DisplayType Type { get; private set; }
    public CharEnum.Command? Command { get; private set; }

    private bool isInitialized = false;
    private bool isAddedEventHandlers = false;

    public static event Action<CommandDisplay, CharEnum.Command> UserRemovedCommand;

    protected override void OnDestroy () {
        RemoveEventHandlers ();

        base.OnDestroy ();
    }

    public void Reset (DisplayType type) {
        Type = type;

        SetClickable (true);

        RemoveCommand (false);

        if (!isInitialized) {
            switch (type) {
                case DisplayType.Container:
                    AddEventHandlers ();
                    break;
            }
        }

        isInitialized = true;
    }

    #region Set command

    public void RemoveCommand (bool isFromUser) {
        var temp = Command;
        Command = null;

        commandImage.enabled = false;
        commandImage.sprite = null;

        switch (Type) {
            case DisplayType.Display:
                SetDisplay (false);
                break;
            case DisplayType.Container:
                if (isFromUser && temp != null) {
                    UserRemovedCommand?.Invoke (this, (CharEnum.Command)temp);
                }
                break;
        }
    }

    public void SetTapCommand (CharEnum.Command command, bool isInAir) {
        var resourcesName = CommandPanelInfo.GetTapCommandSpriteResourcesName (command, isInAir);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate tap command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir, LogTypes.UI | LogTypes.Asset | LogTypes.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    public void SetHoldCommand (CharEnum.Command command, bool isInAir) {
        var resourcesName = CommandPanelInfo.GetHoldCommandSpriteResourcesName (command, isInAir);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate hold command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir, LogTypes.UI | LogTypes.Asset | LogTypes.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    public void SetReleaseCommand (CharEnum.Command command, bool isInAir, bool isSameWithHoldCommand) {
        var resourcesName = CommandPanelInfo.GetReleaseCommandSpriteResourcesName (command, isInAir, isSameWithHoldCommand);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate release command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir + " , isSameWithHoldCommand : " + isSameWithHoldCommand, LogTypes.UI | LogTypes.Asset | LogTypes.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    private void SetCommandCommon (CharEnum.Command command, string spriteResourcesName) {
        Command = command;

        var sprite = Resources.Load<Sprite> (spriteResourcesName);
        if (sprite == null) {
            Log.PrintError ("Generate command sprite failed : Cannot load sprite for resourcesName : " + spriteResourcesName, LogTypes.UI | LogTypes.Asset | LogTypes.Char);
            return;
        }

        commandImage.enabled = true;
        commandImage.sprite = sprite;

        SetDisplay (true);
    }

    #endregion

    #region Set status

    private void SetDisplay (bool isEnable) {
        var color = isEnable ? Color.white : GameVariable.DisabledUIMaskColor;
        baseImage.color = color;
        commandImage.color = color;
    }

    public void SetTargetable (bool isTargetable) {
        foreach (var raycast in targetRaycastList) {
            raycast.raycastTarget = isTargetable;
        }

        SetDisplay (isTargetable);
    }

    public void SetClickable (bool isClickable) {
        if (isClickable && Type == DisplayType.Container) {
            interactable = true;
        } else {
            interactable = false;
        }
    }

    public void SetTargeting (bool isTargeting) {
        targetEffect.SetActive (isTargeting);
    }

    #endregion

    #region Events

    private void AddEventHandlers () {
        if (!isAddedEventHandlers) {
            isAddedEventHandlers = true;

            UIEventManager.AddEventHandler (BtnOnClickType.Game_RemoveCommand, RemoveCommandBtnClickedHandler);
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_RemoveCommand, RemoveCommandBtnClickedHandler);

            isAddedEventHandlers = false;
        }
    }

    private void RemoveCommandBtnClickedHandler (HIHIButton sender) {
        if (sender == crossBtn) {
            RemoveCommand (true);

            crossBtn.gameObject.SetActive (false);
        }
    }

    #region Selectable

    public override void OnSelect (BaseEventData eventData) {
        base.OnSelect (eventData);

        switch (Type) {
            case DisplayType.Container:
                if (Command != null) {
                    crossBtn.gameObject.SetActive (true);
                }
                return;
        }
    }

    public override void OnDeselect (BaseEventData eventData) {
        base.OnDeselect (eventData);

        StartCoroutine (DelayCheckDeselect ());
    }

    private IEnumerator DelayCheckDeselect () {
        // It is to prevent disable the cross button too fast
        // that the cross button on click event cannot be triggered

        yield return new WaitForEndOfFrame ();

        if (EventSystem.current.currentSelectedGameObject != crossBtn.gameObject) {
            crossBtn.gameObject.SetActive (false);
        }
    }

    #endregion

    #endregion
}