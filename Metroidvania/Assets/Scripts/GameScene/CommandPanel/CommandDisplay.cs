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

    public Type type { get; private set; }
    public CharEnum.Command? command { get; private set; }

    public bool isEnable { get; private set; }
    private bool isInitialized = false;
    private bool isAddedEventListeners = false;

    protected override void OnDestroy () {
        RemoveEventListeners ();

        base.OnDestroy ();
    }

    public void Init (Type type, CharEnum.Command? command, CharEnum.InputSituation? situation) {
        if (isInitialized) {
            Log.PrintWarning ("This script has been initialized and is not designed to initial repeatedly. Do not initial again.", LogType.UI | LogType.Char);
            return;
        }

        isInitialized = true;

        this.type = type;

        SetInteractable (true);

        if (command != null && situation != null) {
            SetCommand ((CharEnum.Command)command, (CharEnum.InputSituation)situation);
        } else {
            RemoveCommand ();
        }

        switch (type) {
            case Type.Container:
                AddEventListeners ();
                break;
        }
    }

    private void RemoveCommand () {
        this.command = null;

        commandImage.enabled = false;
        commandImage.sprite = null;

        switch (type) {
            case Type.Display:
                SetEnable (false);
                break;
        }
    }

    public void SetCommand (CharEnum.Command command, CharEnum.InputSituation situation) {
        this.command = command;

        var resourcesName = CommandSpriteMapping.GetCommandSpriteResourcesName (command, situation);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("Generate command sprite failed : resourcesName is empty for command : " + command + " , input situation : " + situation, LogType.UI | LogType.Asset | LogType.Char);
            return;
        }

        var sprite = Resources.Load<Sprite> (resourcesName);
        if (sprite == null) {
            Log.PrintError ("Generate command sprite failed : Cannot load sprite for resourcesName : " + resourcesName, LogType.UI | LogType.Asset | LogType.Char);
            return;
        }

        commandImage.enabled = true;
        commandImage.sprite = sprite;
    }

    public void SetEnable (bool isEnable) {
        this.isEnable = isEnable;
        SetInteractable (isEnable);

        var color = isEnable ? Color.white : GameVariable.DisabledUIMaskColor;
        baseImage.color = color;
        commandImage.color = color;
    }

    private void SetInteractable (bool isInteractable) {
        if (!isInteractable) {
            interactable = false;
            return;
        }

        switch (type) {
            case Type.Container:
                interactable = true;
                return;
            default:
                interactable = false;
                return;
        }
    }

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

    private void OnRemoveCommandClick (HIHIButton btn) {
        if (btn == crossBtn) {
            RemoveCommand ();

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