﻿using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CommandPanel : CommandMatrixPanel {
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI confirmBtnText;

    [SerializeField] private HIHIButton confirmBtn;

    [SerializeField] private Transform commandPickerBaseTransform;
    [SerializeField] private Transform commandPickerDraggingBaseTransform;
    [SerializeField] private CommandDisplay commandDisplayTemplate;

    private GraphicRaycaster graphicRaycaster;

    private readonly Dictionary<CharEnum.Command, CommandDisplay> commandPickerDisplayDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private readonly Dictionary<CharEnum.Command, CommandDisplay> commandPickerDict = new Dictionary<CharEnum.Command, CommandDisplay> ();

    private CharEnum.Command currentDraggingCommand;
    private readonly List<CommandDisplay> currentTargetContainerList = new List<CommandDisplay> ();

    #region CommandMatrixPanel

    protected override bool IsCommandMatrixDisplayOnly => false;

    protected override bool Init () {
        if (!base.Init ()) {
            return false;
        }

        // Raycaster
        graphicRaycaster = GetComponentInParent<GraphicRaycaster> ();
        if (graphicRaycaster == null) {
            Log.PrintError ("Cannot find GraphicRaycaster (which should be in Canvas GameObject). Please check.", LogTypes.UI);
        }

        // Localization
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "CommandPanel_Title"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (confirmBtnText, "Confirm"));
        LangManager.SetTexts (localizedTextDetailsList);

        // Events
        CommandDragFollower.BeganDrag += CommandBeganDragHandler;
        CommandDragFollower.Dragging += CommandDraggingHandler;
        CommandDragFollower.EndedDrag += CommandEndedDragHandler;
        CommandDisplay.UserRemovedCommand += UserRemovedCommandHandler;
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ConfirmCommand, ConfirmCommandBtnClickedHandler);

        return true;
    }

    protected override void OnDestroy () {
        if (IsInitialized) {
            CommandDragFollower.BeganDrag -= CommandBeganDragHandler;
            CommandDragFollower.Dragging -= CommandDraggingHandler;
            CommandDragFollower.EndedDrag -= CommandEndedDragHandler;
            CommandDisplay.UserRemovedCommand -= UserRemovedCommandHandler;
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_ConfirmCommand, ConfirmCommandBtnClickedHandler);
        }

        base.OnDestroy ();
    }

    protected override void ResetCommandMatrix (Dictionary<CharEnum.InputSituation, CharEnum.Command> commandSettings) {
        base.ResetCommandMatrix (commandSettings);

        UpdateConfirmBtn ();
    }

    #endregion

    public void Show (List<CharEnum.Command> enabledCommandList, Dictionary<CharEnum.InputSituation, CharEnum.Command> defaultCommandSettings) {
        base.Show (defaultCommandSettings);

        GenerateCommandPickers (enabledCommandList);
    }

    private void GenerateCommandPickers (List<CharEnum.Command> enabledCommandList) {
        // Display
        foreach (var command in enabledCommandList) {
            if (!commandPickerDisplayDict.ContainsKey (command)) {
                var commandDisplay = Instantiate (commandDisplayTemplate, commandPickerBaseTransform);
                commandDisplay.Reset (CommandDisplay.DisplayType.Display);
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
                picker.Reset (CommandDisplay.DisplayType.Picker);
                picker.SetTapCommand (pair.Key, false);
                commandPickerDict.Add (pair.Key, picker);
            } else {
                picker = commandPickerDict[pair.Key];
            }

            var follower = picker.GetComponent<CommandDragFollower> ();
            follower.Init (pair.Value.transform.localPosition, commandPickerBaseTransform, commandPickerDraggingBaseTransform);
        }
    }

    private CommandDisplay GetBindedCommandContainer (CommandDisplay baseDisplay, CharEnum.Command command, bool isAlsoGetBindingFromRelease) {
        if (baseDisplay == null) {
            return null;
        }

        var isInAir = false;
        CommandDisplay target = null;

        if (CommandMatrixToSituationDict.ContainsKey (baseDisplay)) {
            var situation = CommandMatrixToSituationDict[baseDisplay];

            switch (situation) {
                case CharEnum.InputSituation.GroundHold:
                    isInAir = false;
                    target = CommandMatrix_GroundRelease;
                    break;
                case CharEnum.InputSituation.AirHold:
                    isInAir = true;
                    target = CommandMatrix_AirRelease;
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == CommandMatrix_GroundHold.Command) {
                            isInAir = false;
                            target = CommandMatrix_GroundHold;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == CommandMatrix_AirHold.Command) {
                            isInAir = true;
                            target = CommandMatrix_AirHold;
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

    #region Command Settings

    private void UpdateConfirmBtn () {
        foreach (var pair in CommandMatrixToSituationDict) {
            if (pair.Key.Command != null) {
                confirmBtn.SetInteractable (true);
                return;
            }
        }

        confirmBtn.SetInteractable (false);
    }

    private void UpdateCharCommandSettings () {
        var commandSettings = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();

        foreach (var pair in CommandMatrixToSituationDict) {
            if (pair.Key.Command != null) {
                commandSettings.Add (pair.Value, (CharEnum.Command)pair.Key.Command);
            }
        }

        GameUtils.FindOrSpawnChar ().SetCommandSettings (commandSettings);
        UserManager.SetCommandSettingsCache (commandSettings);
    }

    #endregion

    #region Events

    private void CommandBeganDragHandler (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        var draggingCommandDisplay = sender.GetComponent<CommandDisplay> ();
        if (draggingCommandDisplay.Command == null) {
            Log.PrintError ("Dragging command is null. Please check.", LogTypes.UI | LogTypes.Input | LogTypes.Char);
            return;
        }

        currentDraggingCommand = (CharEnum.Command)draggingCommandDisplay.Command;

        var disallowList = CommandPanelInfo.GetDisallowInputSituationList (currentDraggingCommand);
        foreach (var pair in CommandMatrixToSituationDict) {
            pair.Key.SetClickable (false);

            if (disallowList.Contains (pair.Value)) {
                pair.Key.SetTargetable (false);
            }
        }
    }

    private void CommandDraggingHandler (UIDragFollower sender, PointerEventData eventData) {
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
                if (CommandMatrixToSituationDict.ContainsKey (commandDisplay)) {
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

    private void CommandEndedDragHandler (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        foreach (var target in currentTargetContainerList) {
            target.SetTargeting (false);

            if (!CommandMatrixToSituationDict.ContainsKey (target)) {
                Log.PrintError ("commandMatrixToSituationDict do not contain target container. Please check", LogTypes.UI | LogTypes.Input | LogTypes.Char);
                continue;
            }

            var situation = CommandMatrixToSituationDict[target];

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
                        IsGroundHoldBinding = true;
                    } else {
                        if (IsGroundHoldBinding) {
                            CommandMatrix_GroundRelease.RemoveCommand (false);
                            IsGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirHold:
                    target.SetHoldCommand (currentDraggingCommand, true);

                    if (currentTargetContainerList.Count > 1) {
                        IsAirHoldBinding = true;
                    } else {
                        if (IsAirHoldBinding) {
                            CommandMatrix_AirRelease.RemoveCommand (false);
                            IsGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    var isSame = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, false, isSame);
                    if (!isSame) {
                        if (IsGroundHoldBinding) {
                            CommandMatrix_GroundHold.RemoveCommand (false);
                            IsGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    var isSame2 = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, true, isSame2);
                    if (!isSame2) {
                        if (IsAirHoldBinding) {
                            CommandMatrix_AirHold.RemoveCommand (false);
                            IsAirHoldBinding = false;
                        }
                    }
                    break;
            }
        }

        currentTargetContainerList.Clear ();

        foreach (var pair in CommandMatrixToSituationDict) {
            pair.Key.SetClickable (true);
            pair.Key.SetTargetable (true);
        }

        UpdateConfirmBtn ();
    }

    private void UserRemovedCommandHandler (CommandDisplay sender, CharEnum.Command removedCommand) {
        var binding = GetBindedCommandContainer (sender, removedCommand, true);

        if (binding != null) {
            binding.RemoveCommand (false);

            var situation = CommandMatrixToSituationDict[binding];
            switch (situation) {
                case CharEnum.InputSituation.GroundHold:
                case CharEnum.InputSituation.GroundRelease:
                    IsGroundHoldBinding = false;
                    break;
                case CharEnum.InputSituation.AirHold:
                case CharEnum.InputSituation.AirRelease:
                    IsAirHoldBinding = false;
                    break;
            }
        }

        UpdateConfirmBtn ();
    }


    private void ConfirmCommandBtnClickedHandler (HIHIButton sender) {
        UpdateCharCommandSettings ();
        Hide ();
    }

    #endregion
}