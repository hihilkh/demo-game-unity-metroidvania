using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandPanel : GeneralPanel {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI airText;
    [SerializeField] private TextMeshProUGUI groundText;
    [SerializeField] private TextMeshProUGUI tapText;
    [SerializeField] private TextMeshProUGUI holdText;
    [SerializeField] private TextMeshProUGUI releaseText;
    [SerializeField] private TextMeshProUGUI confirmBtnText;

    [SerializeField] private CommandDisplay commandContainer_AirTap;
    [SerializeField] private CommandDisplay commandContainer_AirHold;
    [SerializeField] private CommandDisplay commandContainer_AirRelease;
    [SerializeField] private CommandDisplay commandContainer_GroundTap;
    [SerializeField] private CommandDisplay commandContainer_GroundHold;
    [SerializeField] private CommandDisplay commandContainer_GroundRelease;

    [SerializeField] private Transform commandPickerBaseTransform;
    [SerializeField] private Transform commandPickerDraggingBaseTransform;
    [SerializeField] private CommandDisplay commandDisplayTemplate;

    private GraphicRaycaster graphicRaycaster;

    private bool isInitialized = false;

    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDisplayDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private Dictionary<CommandDisplay, CharEnum.InputSituation> commandContainerToSituationDict = new Dictionary<CommandDisplay, CharEnum.InputSituation> ();

    private CharEnum.Command currentDraggingCommand;
    private List<CommandDisplay> currentTargetContainerList = new List<CommandDisplay> ();

    private bool isGroundHoldBinding = false;
    private bool isAirHoldBinding = false;

    private void Start () {
        var list = new List<CharEnum.Command> ();
        list.Add (CharEnum.Command.Hit);
        list.Add (CharEnum.Command.Jump);

        var settings = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();
        settings.Add (CharEnum.InputSituation.AirHold, CharEnum.Command.Hit);

        Show (list, settings);
    }

    private void Init () {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        // Raycaster
        graphicRaycaster = GetComponentInParent<GraphicRaycaster> ();
        if (graphicRaycaster == null) {
            Log.PrintError ("Cannot find GraphicRaycaster (which should be in Canvas GameObject). Please check.", LogType.UI);
        }

        // CommandContainer
        commandContainerToSituationDict.Clear ();
        commandContainerToSituationDict.Add (commandContainer_AirTap, CharEnum.InputSituation.AirTap);
        commandContainerToSituationDict.Add (commandContainer_AirHold, CharEnum.InputSituation.AirHold);
        commandContainerToSituationDict.Add (commandContainer_AirRelease, CharEnum.InputSituation.AirRelease);
        commandContainerToSituationDict.Add (commandContainer_GroundTap, CharEnum.InputSituation.GroundTap);
        commandContainerToSituationDict.Add (commandContainer_GroundHold, CharEnum.InputSituation.GroundHold);
        commandContainerToSituationDict.Add (commandContainer_GroundRelease, CharEnum.InputSituation.GroundRelease);

        // TODO
        //localization

        CommandDragFollower.BeganDragEvent += OnCommandBeganDrag;
        CommandDragFollower.DraggingEvent += OnCommandDragging;
        CommandDragFollower.EndedDragEvent += OnCommandEndedDrag;
        CommandDisplay.UserRemovedCommandEvent += OnUserRemovedCommand;
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ConfirmCommand, OnConfirmCommandClick);
    }

    protected override void OnDestroy () {
        if (isInitialized) {
            CommandDragFollower.BeganDragEvent -= OnCommandBeganDrag;
            CommandDragFollower.DraggingEvent -= OnCommandDragging;
            CommandDragFollower.EndedDragEvent -= OnCommandEndedDrag;
            CommandDisplay.UserRemovedCommandEvent -= OnUserRemovedCommand;
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_ConfirmCommand, OnConfirmCommandClick);
        }

        base.OnDestroy ();
    }

    public void Show (List<CharEnum.Command> enabledCommandList, Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        Init ();

        GenerateCommandPickers (enabledCommandList);
        ResetCommandContainers (defaultCommandSettings);
    }

    private void GenerateCommandPickers (List<CharEnum.Command> enabledCommandList) {
        // Display
        foreach (var command in enabledCommandList) {
            if (!commandPickerDisplayDict.ContainsKey (command)) {
                var commandDisplay = Instantiate (commandDisplayTemplate, commandPickerBaseTransform);
                commandDisplay.Reset (CommandDisplay.Type.Display);
                commandDisplay.SetTapCommand (command, false);

                commandPickerDisplayDict.Add (command, commandDisplay);
            }
        }

        // Pickers
        StartCoroutine (WaitAndGenerateCommandPickersFromDisplay ());
    }

    private IEnumerator WaitAndGenerateCommandPickersFromDisplay () {
        yield return null;

        foreach (var pair in commandPickerDisplayDict) {
            CommandDisplay picker;
            if (!commandPickerDict.ContainsKey (pair.Key)) {
                picker = Instantiate (commandDisplayTemplate, commandPickerBaseTransform);
                picker.gameObject.AddComponent<CommandDragFollower> ();
                var layoutElement = picker.gameObject.AddComponent<LayoutElement> ();
                layoutElement.ignoreLayout = true;
                picker.Reset (CommandDisplay.Type.Picker);
                picker.SetTapCommand (pair.Key, false);
                commandPickerDict.Add (pair.Key, picker);
            } else {
                picker = commandPickerDict[pair.Key];
            }

            var follower = picker.GetComponent<CommandDragFollower> ();
            follower.Init (pair.Value.transform.localPosition, commandPickerBaseTransform, commandPickerDraggingBaseTransform);
        }
    }

    private void ResetCommandContainers (Dictionary<CharEnum.InputSituation, CharEnum.Command> commandSettings) {
        isGroundHoldBinding = false;
        isAirHoldBinding = false;

        CharEnum.Command? groundHoldCommand = null;
        CharEnum.Command? airHoldCommand = null;

        if (commandSettings.ContainsKey (CharEnum.InputSituation.GroundHold)) {
            groundHoldCommand = commandSettings[CharEnum.InputSituation.GroundHold];
        }

        if (commandSettings.ContainsKey (CharEnum.InputSituation.AirHold)) {
            airHoldCommand = commandSettings[CharEnum.InputSituation.AirHold];
        }

        foreach (var pair in commandContainerToSituationDict) {
            pair.Key.Reset (CommandDisplay.Type.Container);


            if (commandSettings.ContainsKey (pair.Value)) {
                var command = commandSettings[pair.Value];

                switch (pair.Value) {
                    case CharEnum.InputSituation.GroundTap:
                        pair.Key.SetTapCommand (command, false);
                        break;
                    case CharEnum.InputSituation.AirTap:
                        pair.Key.SetTapCommand (command, true);
                        break;
                    case CharEnum.InputSituation.GroundHold:
                        pair.Key.SetHoldCommand (command, false);
                        break;
                    case CharEnum.InputSituation.AirHold:
                        pair.Key.SetHoldCommand (command, true);
                        break;
                    case CharEnum.InputSituation.GroundRelease:
                        var isSame = command == groundHoldCommand;
                        pair.Key.SetReleaseCommand (command, false, isSame);
                        if (isSame) {
                            if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, false)) {
                                isGroundHoldBinding = true;
                            }
                        }
                        break;
                    case CharEnum.InputSituation.AirRelease:
                        var isSame2 = command == airHoldCommand;
                        pair.Key.SetReleaseCommand (command, true, isSame2);
                        if (isSame2) {
                            if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, true)) {
                                isAirHoldBinding = true;
                            }
                        }
                        break;
                }
            }
        }
    }

    private CommandDisplay GetBindedCommandContainer (CommandDisplay baseDisplay, CharEnum.Command command, bool isAlsoGetBindingFromRelease) {
        if (baseDisplay == null) {
            return null;
        }

        var isInAir = false;
        CommandDisplay target = null;

        if (commandContainerToSituationDict.ContainsKey (baseDisplay)) {
            var situation = commandContainerToSituationDict[baseDisplay];

            switch (situation) {
                case CharEnum.InputSituation.GroundHold:
                    isInAir = false;
                    target = commandContainer_GroundRelease;
                    break;
                case CharEnum.InputSituation.AirHold:
                    isInAir = true;
                    target = commandContainer_AirRelease;
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == commandContainer_GroundHold.command) {
                            isInAir = false;
                            target = commandContainer_GroundHold;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == commandContainer_AirHold.command) {
                            isInAir = true;
                            target = commandContainer_AirHold;
                        }
                    }
                    break;
            }
        }

        if (target == null) {
            return null;
        }

        if (CommandPanelInfo.CheckIsHoldBindedWithRelease (command, isInAir)) {
            return target;
        } else {
            return null;
        }
    }

    #region Events

    private void OnCommandBeganDrag (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        var draggingCommandDisplay = sender.GetComponent<CommandDisplay> ();
        if (draggingCommandDisplay.command == null) {
            Log.PrintError ("Dragging command is null. Please check.", LogType.UI | LogType.Input | LogType.Char);
            return;
        }

        currentDraggingCommand = (CharEnum.Command)draggingCommandDisplay.command;

        var disallowList = CommandPanelInfo.GetDisallowInputSituationList (currentDraggingCommand);
        foreach (var pair in commandContainerToSituationDict) {
            pair.Key.SetClickable (false);

            if (disallowList.Contains (pair.Value)) {
                pair.Key.SetTargetable (false);
            }
        }
    }

    private void OnCommandDragging (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        CommandDisplay target = null;

        var raycastResults = new List<RaycastResult> ();
        graphicRaycaster.Raycast (eventData, raycastResults);

        foreach (var result in raycastResults) {
            if (result.gameObject == sender.gameObject) {
                continue;
            }

            var commandDisplay = result.gameObject.GetComponentInParent<CommandDisplay> ();
            if (commandDisplay != null) {
                if (commandContainerToSituationDict.ContainsKey (commandDisplay)) {
                    target = commandDisplay;
                    break;
                }
            }
        }

        var binding = GetBindedCommandContainer (target, currentDraggingCommand, false);

        var containersToLoop = new List<CommandDisplay> (currentTargetContainerList);
        foreach (var container in containersToLoop) {
            if (container == target) {
                target = null;
                continue;
            }

            if (container == binding) {
                binding = null;
                continue;
            }

            container.SetTargeting (false);
            currentTargetContainerList.Remove (container);
        }

        if (target != null) {
            target.SetTargeting (true);
            currentTargetContainerList.Add (target);
        }

        if (binding != null) {
            binding.SetTargeting (true);
            currentTargetContainerList.Add (binding);
        }

    }

    private void OnCommandEndedDrag (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        foreach (var target in currentTargetContainerList) {
            target.SetTargeting (false);

            if (!commandContainerToSituationDict.ContainsKey (target)) {
                Log.PrintError ("commandContainerToSituationDict do not contain target container. Please check", LogType.UI | LogType.Input | LogType.Char);
                continue;
            }

            var situation = commandContainerToSituationDict[target];

            switch (situation) {
                case CharEnum.InputSituation.GroundTap:
                    target.SetTapCommand (currentDraggingCommand, false);
                    break;
                case CharEnum.InputSituation.AirTap:
                    target.SetTapCommand (currentDraggingCommand, true);
                    break;
                case CharEnum.InputSituation.GroundHold:
                    target.SetHoldCommand (currentDraggingCommand, false);

                    if (currentTargetContainerList.Count > 1) {
                        isGroundHoldBinding = true;
                    } else {
                        if (isGroundHoldBinding) {
                            commandContainer_GroundRelease.RemoveCommand (false);
                            isGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirHold:
                    target.SetHoldCommand (currentDraggingCommand, true);

                    if (currentTargetContainerList.Count > 1) {
                        isAirHoldBinding = true;
                    } else {
                        if (isAirHoldBinding) {
                            commandContainer_AirRelease.RemoveCommand (false);
                            isGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    var isSame = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, false, isSame);
                    if (!isSame) {
                        if (isGroundHoldBinding) {
                            commandContainer_GroundHold.RemoveCommand (false);
                            isGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    var isSame2 = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, true, isSame2);
                    if (!isSame2) {
                        if (isAirHoldBinding) {
                            commandContainer_AirHold.RemoveCommand (false);
                            isAirHoldBinding = false;
                        }
                    }
                    break;
            }
        }

        currentTargetContainerList.Clear ();

        foreach (var pair in commandContainerToSituationDict) {
            pair.Key.SetClickable (true);
            pair.Key.SetTargetable (true);
        }
    }

    private void OnUserRemovedCommand (CommandDisplay sender, CharEnum.Command removedCommand) {
        var binding = GetBindedCommandContainer (sender, removedCommand, true);

        if (binding != null) {
            binding.RemoveCommand (false);

            var situation = commandContainerToSituationDict[binding];
            switch (situation) {
                case CharEnum.InputSituation.GroundHold:
                case CharEnum.InputSituation.GroundRelease:
                    isGroundHoldBinding = false;
                    break;
                case CharEnum.InputSituation.AirHold:
                case CharEnum.InputSituation.AirRelease:
                    isAirHoldBinding = false;
                    break;
            }
        }

    }


    private void OnConfirmCommandClick (HIHIButton btn) {
        // TODO
    }

    #endregion
}