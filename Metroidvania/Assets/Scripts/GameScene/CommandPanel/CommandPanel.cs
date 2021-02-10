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

    [SerializeField] private CommandDisplay commandDisplay_AirTap;
    [SerializeField] private CommandDisplay commandDisplay_AirHold;
    [SerializeField] private CommandDisplay commandDisplay_AirRelease;
    [SerializeField] private CommandDisplay commandDisplay_GroundTap;
    [SerializeField] private CommandDisplay commandDisplay_GroundHold;
    [SerializeField] private CommandDisplay commandDisplay_GroundRelease;

    [SerializeField] private Transform commandPickerBaseTransform;
    [SerializeField] private Transform commandPickerDraggingBaseTransform;
    [SerializeField] private CommandDisplay commandDisplayTemplate;

    private GraphicRaycaster graphicRaycaster;

    private bool isInitialized = false;

    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDisplayDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private Dictionary<CharEnum.Command, CommandDisplay> commandPickerDict = new Dictionary<CharEnum.Command, CommandDisplay> ();
    private Dictionary<CharEnum.InputSituation, CommandDisplay> commandContainerDict = new Dictionary<CharEnum.InputSituation, CommandDisplay> ();

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
        commandContainerDict.Clear ();
        commandContainerDict.Add (CharEnum.InputSituation.AirTap, commandDisplay_AirTap);
        commandContainerDict.Add (CharEnum.InputSituation.AirHold, commandDisplay_AirHold);
        commandContainerDict.Add (CharEnum.InputSituation.AirRelease, commandDisplay_AirRelease);
        commandContainerDict.Add (CharEnum.InputSituation.GroundTap, commandDisplay_GroundTap);
        commandContainerDict.Add (CharEnum.InputSituation.GroundHold, commandDisplay_GroundHold);
        commandContainerDict.Add (CharEnum.InputSituation.GroundRelease, commandDisplay_GroundRelease);

        // TODO
        //localization

        CommandDragFollower.EndedDragEvent += OnCommandEndedDrag;
        UIEventManager.AddEventHandler (BtnOnClickType.Game_ConfirmCommand, OnConfirmCommandClick);
    }

    protected override void OnDestroy () {
        if (isInitialized) {
            CommandDragFollower.EndedDragEvent -= OnCommandEndedDrag;
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
            if (!commandPickerDisplayDict.ContainsKey(command)) {
                var commandDisplay = Instantiate (commandDisplayTemplate, commandPickerBaseTransform);
                commandDisplay.Init (CommandDisplay.Type.Display, command, CharEnum.InputSituation.GroundTap);
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
                picker.Init (CommandDisplay.Type.Picker, pair.Key, CharEnum.InputSituation.GroundTap);
                commandPickerDict.Add (pair.Key, picker);
            } else {
                picker = commandPickerDict[pair.Key];
            }

            var follower = picker.GetComponent<CommandDragFollower> ();
            follower.Init (pair.Value.transform.localPosition, commandPickerBaseTransform, commandPickerDraggingBaseTransform);
        }
    }

    private void ResetCommandContainers (Dictionary<CharEnum.InputSituation, CharEnum.Command> commandSettings) {
        foreach (var pair in commandContainerDict) {
            if (commandSettings.ContainsKey(pair.Key)) {
                var command = commandSettings[pair.Key];
                pair.Value.Init (CommandDisplay.Type.Container, command, pair.Key);
            } else {
                pair.Value.Init (CommandDisplay.Type.Container, null, null);
            }
        }
    }

    #region Events

    private void OnCommandEndedDrag (UIDragFollower sender, PointerEventData eventData) {
        if (!(sender is CommandDragFollower)) {
            return;
        }

        var raycastResults = new List<RaycastResult> ();
        graphicRaycaster.Raycast (eventData, raycastResults);

        foreach (var result in raycastResults) {
            if (result.gameObject == sender.gameObject) {
                continue;
            }

            var commandDisplay = result.gameObject.GetComponent<CommandDisplay> ();
            if (commandDisplay != null) {
                foreach (var pair in commandContainerDict) {
                    if (commandDisplay == pair.Value) {
                        var situation = pair.Key;
                        var draggingCommandDisplay = sender.GetComponent<CommandDisplay> ();

                        commandDisplay.SetCommand ((CharEnum.Command)draggingCommandDisplay.command, situation);
                    }
                }
            }
        }
    }

    private void OnConfirmCommandClick (HIHIButton btn) {
        // TODO
    }

    #endregion
}
