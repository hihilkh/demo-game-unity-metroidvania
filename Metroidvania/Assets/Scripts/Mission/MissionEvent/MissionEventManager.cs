using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

// TODO : Think of getting mission events details by JSON
public class MissionEventManager : MonoBehaviour {
    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private TutorialFinger tutorialFinger;
    [SerializeField] private CommandPanel commandPanel;

    public static MissionEvent CurrentMissionEvent { get; private set; } = null;
    public static SubEventBase CurrentMissionSubEvent { get; private set; } = null;

    #region All mission events

    private static readonly List<MissionEvent> AllMissionEvents = new List<MissionEvent> () {
        new MissionEvent (
            MissionEventEnum.EventType.Command_Hit,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Hit")),
            new CommandPanelSubEvent (CharEnum.Command.Hit, CharEnum.InputSituation.GroundTap, "Event_Command_Hit_Panel", null)
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Command_Jump,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Jump")),
            new CommandPanelSubEvent (CharEnum.Command.Jump, CharEnum.InputSituation.GroundHold, "Event_Command_Jump_Panel", "Event_Command_Jump_Panel_Done"),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundRelease, "Event_ChargeJump_Instruction")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.FirstHit,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_FirstHit")),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundTap, "Event_FirstHit_Instruction"),
            new WaitSubEvent (1.5f),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundTap, "Event_SecondHit_Instruction")
        ),
    };

    #endregion

    private void OnDestroy () {
        CurrentMissionEvent = null;
        CurrentMissionSubEvent = null;
    }

    #region getter

    private static MissionEvent GetMissionEvent (MissionEventEnum.EventType eventType) {
        foreach (var missionEvent in AllMissionEvents) {
            if (missionEvent.EventType == eventType) {
                return missionEvent;
            }
        }

        Log.PrintWarning ("Cannot find mission event with event type : " + eventType, LogTypes.MissionEvent);
        return null;
    }

    #endregion

    #region Start event

    public void StartEvent (MissionEventEnum.EventType eventType, bool isFromCollectable, Action onFinished = null) {
        var missionEvent = GetMissionEvent (eventType);

        if (missionEvent == null) {
            Log.PrintWarning ("Start event failed. eventType : " + eventType, LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        var subEventListClone = missionEvent.GetSubEventListClone ();
        if (subEventListClone == null || subEventListClone.Count <= 0) {
            Log.PrintWarning ("Start event failed. The sub event list is empty. eventType : " + eventType, LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        Log.Print ("Start Mission Event : eventType = " + eventType, LogTypes.MissionEvent);

        CurrentMissionEvent = missionEvent;
        Time.timeScale = 0;

        Action onEventFinished = () => {
            CurrentMissionEvent = null;
            CurrentMissionSubEvent = null;

            Time.timeScale = 1;

            // If it is from collectable, set mission event done together with UserManager.CollectCollectable ()
            if (!isFromCollectable) {
                UserManager.SetMissionEventDone (missionEvent.EventType);
            }

            onFinished?.Invoke ();
        };

        StartSubEventRecursive (eventType, subEventListClone, onEventFinished);
    }

    private void StartSubEventRecursive (MissionEventEnum.EventType eventType, List<SubEventBase> remainingSubEventList, Action onAllFinished = null) {
        if (remainingSubEventList != null && remainingSubEventList.Count > 0) {
            var currentSubEvent = remainingSubEventList[0];
            remainingSubEventList.RemoveAt (0);

            Action onSubEventFinished = () => {
                StartSubEventRecursive (eventType, remainingSubEventList, onAllFinished);
            };

            Log.Print ("Start Mission SubEvent : subEventType = " + currentSubEvent.SubEventType, LogTypes.MissionEvent);

            CurrentMissionSubEvent = currentSubEvent;

            switch (currentSubEvent.SubEventType) {
                case MissionEventEnum.SubEventType.Dialog:
                    StartDialogSubEvent ((DialogSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.CommandPanel:
                    StartCommandPanelSubEvent ((CommandPanelSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.CameraInput:
                    StartCameraInputSubEvent ((CameraInputSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.CommandInput:
                    StartCommandInputSubEvent ((CommandInputSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.MapSwitch:
                    StartMapSwitchSubEvent ((MapSwitchSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.Wait:
                    StartCoroutine (StartWaitSubEvent ((WaitSubEvent)currentSubEvent, onSubEventFinished));
                    break;
            }
        } else {
            onAllFinished?.Invoke ();
        }
    }

    #region SubEvents

    private void StartDialogSubEvent (DialogSubEvent subEvent, Action onFinished = null) {
        uiManager.ShowDialogPanel (subEvent, onFinished);
    }

    private void StartCommandPanelSubEvent (CommandPanelSubEvent subEvent, Action onFinished = null) {
        Action onConfirmed = () => {
            tutorialFinger.Hide ();
            onFinished?.Invoke ();
        };

        Action onAfterSetCommandFinished = () => {
            tutorialFinger.ShowTap (commandPanel.GetConfirmBtnRectTransform ());
        };

        Action onSetCommandFinished = () => {
            tutorialFinger.Hide ();
            if (string.IsNullOrEmpty (subEvent.AfterSetCommandLocalizationKeyBase)) {
                onAfterSetCommandFinished ();
            } else {
                uiManager.ShowInstructionPanel (subEvent.AfterSetCommandLocalizationKeyBase, onAfterSetCommandFinished);
            }
        };

        var targets = commandPanel.Show (subEvent, onSetCommandFinished, onConfirmed);

        Action onBeforeSetCommandFinished = () => {
            tutorialFinger.ShowDragAndDrop (targets.Item1, targets.Item2);
        };

        if (string.IsNullOrEmpty (subEvent.BeforeSetCommandLocalizationKeyBase)) {
            onBeforeSetCommandFinished ();
        } else {
            uiManager.ShowInstructionPanel (subEvent.BeforeSetCommandLocalizationKeyBase, onBeforeSetCommandFinished);
        }
    }

    private void StartCameraInputSubEvent (CameraInputSubEvent subEvent, Action onFinished = null) {
        Action onInputFinished = () => {
            if (string.IsNullOrEmpty (subEvent.AfterInputLocalizationKeyBase)) {
                onFinished?.Invoke ();
            } else {
                uiManager.ShowInstructionPanel (subEvent.AfterInputLocalizationKeyBase, onFinished);
            }
        };

        Action onBeforeInputFinished = () => {
            // TODO
        };

        if (string.IsNullOrEmpty (subEvent.BeforeInputLocalizationKeyBase)) {
            onBeforeInputFinished ();
        } else {
            uiManager.ShowInstructionPanel (subEvent.BeforeInputLocalizationKeyBase, onBeforeInputFinished);
        }
    }

    private void StartCommandInputSubEvent (CommandInputSubEvent subEvent, Action onFinished = null) {
        Action onInputFinished = () => {
            tutorialFinger.Hide ();
            onFinished?.Invoke ();
        };

        Action onBeforeInputFinished = () => {
            if (subEvent.InputSituation == CharEnum.InputSituation.GroundTap || subEvent.InputSituation == CharEnum.InputSituation.AirTap) {
                tutorialFinger.ShowLoopTap_RightScreen ();
            } else {
                tutorialFinger.ShowLoopHoldRelease_RightScreen ();
            }

            var charModel = GameUtils.FindOrSpawnChar ();
            charModel.SetAllowUserControl (true, true);
            charModel.SetMissionEventTapHandler (onInputFinished);
        };

        if (string.IsNullOrEmpty (subEvent.BeforeInputLocalizationKeyBase)) {
            onBeforeInputFinished ();
        } else {
            uiManager.ShowInstructionPanel (subEvent.BeforeInputLocalizationKeyBase, onBeforeInputFinished);
        }
    }

    private void StartMapSwitchSubEvent (MapSwitchSubEvent subEvent, Action onFinished = null) {
        mapManager.SwitchOnMapSwitch (subEvent, onFinished);
    }

    private IEnumerator StartWaitSubEvent (WaitSubEvent subEvent, Action onFinished = null) {
        GameUtils.FindOrSpawnChar ().SetAllowUserControl (false);
        Time.timeScale = 1;

        yield return new WaitForSeconds (subEvent.WaitTime);

        GameUtils.FindOrSpawnChar ().SetAllowUserControl (true);
        Time.timeScale = 0;

        onFinished?.Invoke ();
    }

    #endregion

    #endregion
}