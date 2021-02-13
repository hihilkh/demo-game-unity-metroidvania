using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandDisplay : Selectable {

    public enum Type {
        Picker,
        Container,
        Display,
    }

    [SerializeField] private Image baseImage;
    [SerializeField] private Image commandImage;
    [SerializeField] private HIHIButton crossBtn;
    [SerializeField] private GameObject targetEffect;
    [SerializeField] private List<Graphic> targetRaycastList;

    public Type type { get; private set; }
    public CharEnum.Command? command { get; private set; }

    private bool isInitialized = false;
    private bool isAddedEventListeners = false;

    public static event Action<CommandDisplay, CharEnum.Command> UserRemovedCommandEvent;

    protected override void OnDestroy () {
        RemoveEventListeners ();

        base.OnDestroy ();
    }

    public void Reset (Type type) {
        this.type = type;

        SetClickable (true);

        RemoveCommand (false);

        if (!isInitialized) {
            switch (type) {
                case Type.Container:
                    AddEventListeners ();
                    break;
            }
        }

        isInitialized = true;
    }

    #region Set command

    public void RemoveCommand (bool isFromUser) {
        var temp = command;
        command = null;

        commandImage.enabled = false;
        commandImage.sprite = null;

        switch (type) {
            case Type.Display:
                SetDisplay (false);
                break;
            case Type.Container:
                if (isFromUser && temp != null) {
                    UserRemovedCommandEvent?.Invoke (this, (CharEnum.Command)temp);
                }
                break;
        }
    }

    public void SetTapCommand (CharEnum.Command command, bool isInAir) {
        var resourcesName = CommandPanelInfo.GetTapCommandSpriteResourcesName (command, isInAir);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate tap command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir, LogType.UI | LogType.Asset | LogType.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    public void SetHoldCommand (CharEnum.Command command, bool isInAir) {
        var resourcesName = CommandPanelInfo.GetHoldCommandSpriteResourcesName (command, isInAir);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate hold command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir, LogType.UI | LogType.Asset | LogType.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    public void SetReleaseCommand (CharEnum.Command command, bool isInAir, bool isSameWithHoldCommand) {
        var resourcesName = CommandPanelInfo.GetReleaseCommandSpriteResourcesName (command, isInAir, isSameWithHoldCommand);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate release command sprite failed : resourcesName is empty for command : " + command + " , isInAir : " + isInAir + " , isSameWithHoldCommand : " + isSameWithHoldCommand, LogType.UI | LogType.Asset | LogType.Char);
            return;
        }

        SetCommandCommon (command, resourcesName);
    }

    private void SetCommandCommon (CharEnum.Command command, string spriteResourcesName) {
        this.command = command;

        var sprite = Resources.Load<Sprite> (spriteResourcesName);
        if (sprite == null) {
            Log.PrintError ("Generate command sprite failed : Cannot load sprite for resourcesName : " + spriteResourcesName, LogType.UI | LogType.Asset | LogType.Char);
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
        if (isClickable && type == Type.Container) {
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

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            UIEventManager.AddEventHandler (BtnOnClickType.Game_RemoveCommand, OnRemoveCommandClick);
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_RemoveCommand, OnRemoveCommandClick);

            isAddedEventListeners = false;
        }
    }

    private void OnRemoveCommandClick (HIHIButton sender) {
        if (sender == crossBtn) {
            RemoveCommand (true);

            crossBtn.gameObject.SetActive (false);
        }
    }

    public override void OnSelect (BaseEventData eventData) {
        base.OnSelect (eventData);

        switch (type) {
            case Type.Container:
                if (command != null) {
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
}