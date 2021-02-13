using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
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

    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDisplayDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDict = new Dictionary<CharEnum.Command, CommandDisplay> ();

    private CharEnum.Command currentDraggingCommand;
    private List<CommandDisplay> currentTargetContainerList = new List<CommandDisplay> ();

    #region CommandMatrixPanel

    protected override bool isCommandMatrixDisplayOnly => false;

    protected override bool Init () {
        if (!base.Init ()) {
            return false;
        }

        // Raycaster
        graphicRaycaster = GetComponentInParent<GraphicRaycaster> ();
        if (graphicRaycaster == null) {
            Log.PrintError ("Cannot find GraphicRaycaster (which should be in Canvas GameObject). Please check.", LogType.UI);
        }

        // Localization
        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, "CommandPanel_Title"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (confirmBtnText, "Confirm"));
        LangManager.SetTexts (localizedTextDetailsList);

        // Events
        CommandDragFollower.BeganDragEvent += OnCommandBeganDrag;
        CommandDragFollower.DraggingEvent += OnCommandDragging;
        CommandDragFollower.EndedDragEvent += OnCommandEndedDrag;
        CommandDisplay.UserRemovedCommandEvent += OnUserRemovedCommand;
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ConfirmCommand, OnConfirmCommandClick);

        return true;
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

    private CommandDisplay GetBindedCommandContainer (CommandDisplay baseDisplay, CharEnum.Command command, bool isAlsoGetBindingFromRelease) {
        if (baseDisplay == null) {
            return null;
        }

        var isInAir = false;
        CommandDisplay target = null;

        if (commandMatrixToSituationDict.ContainsKey (baseDisplay)) {
            var situation = commandMatrixToSituationDict[baseDisplay];

            switch (situation) {
                case CharEnum.InputSituation.GroundHold:
                    isInAir = false;
                    target = commandMatrix_GroundRelease;
                    break;
                case CharEnum.InputSituation.AirHold:
                    isInAir = true;
                    target = commandMatrix_AirRelease;
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == commandMatrix_GroundHold.command) {
                            isInAir = false;
                            target = commandMatrix_GroundHold;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    if (isAlsoGetBindingFromRelease) {
                        if (command == commandMatrix_AirHold.command) {
                            isInAir = true;
                            target = commandMatrix_AirHold;
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
        foreach (var pair in commandMatrixToSituationDict) {
            if (pair.Key.command != null) {
                confirmBtn.SetInteractable (true);
                return;
            }
        }

        confirmBtn.SetInteractable (false);
    }

    private void UpdateCharCommandSettings () {
        var commandSettings = new Dictionary<CharEnum.InputSituation, CharEnum.Command> ();

        foreach (var pair in commandMatrixToSituationDict) {
            if (pair.Key.command != null) {
                commandSettings.Add (pair.Value, (CharEnum.Command)pair.Key.command);
            }
        }

        GameUtils.FindOrSpawnChar ().SetCommandSettings (commandSettings);
        UserManager.SetCommandSettingsCache (commandSettings);
    }

    #endregion

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
        foreach (var pair in commandMatrixToSituationDict) {
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
                if (commandMatrixToSituationDict.ContainsKey (commandDisplay)) {
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

            if (!commandMatrixToSituationDict.ContainsKey (target)) {
                Log.PrintError ("commandMatrixToSituationDict do not contain target container. Please check", LogType.UI | LogType.Input | LogType.Char);
                continue;
            }

            var situation = commandMatrixToSituationDict[target];

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
                            commandMatrix_GroundRelease.RemoveCommand (false);
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
                            commandMatrix_AirRelease.RemoveCommand (false);
                            isGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.GroundRelease:
                    var isSame = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, false, isSame);
                    if (!isSame) {
                        if (isGroundHoldBinding) {
                            commandMatrix_GroundHold.RemoveCommand (false);
                            isGroundHoldBinding = false;
                        }
                    }
                    break;
                case CharEnum.InputSituation.AirRelease:
                    var isSame2 = currentTargetContainerList.Count > 1;
                    target.SetReleaseCommand (currentDraggingCommand, true, isSame2);
                    if (!isSame2) {
                        if (isAirHoldBinding) {
                            commandMatrix_AirHold.RemoveCommand (false);
                            isAirHoldBinding = false;
                        }
                    }
                    break;
            }
        }

        currentTargetContainerList.Clear ();

        foreach (var pair in commandMatrixToSituationDict) {
            pair.Key.SetClickable (true);
            pair.Key.SetTargetable (true);
        }

        UpdateConfirmBtn ();
    }

    private void OnUserRemovedCommand (CommandDisplay sender, CharEnum.Command removedCommand) {
        var binding = GetBindedCommandContainer (sender, removedCommand, true);

        if (binding != null) {
            binding.RemoveCommand (false);

            var situation = commandMatrixToSituationDict[binding];
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

        UpdateConfirmBtn ();
    }


    private void OnConfirmCommandClick (HIHIButton sender) {
        UpdateCharCommandSettings ();
        Hide ();
    }

    #endregion
}