using System;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

// TODO : Think of getting mission events details by JSON
public class MissionEventManager : MonoBehaviour {
    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MapManager mapManager;

    #region All mission events

    private static readonly List<MissionEvent> AllMissionEvents = new List<MissionEvent> () {
        new MissionEvent (
            MissionEventEnum.EventType.Command_Hit,
            new DialogSubEvent (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Hit"),
            new CommandPanelSubEvent (CharEnum.Command.Hit, CharEnum.InputSituation.GroundTap, null, "Event_Command_Hit_Panel", "Event_Command_Hit_Panel_Confirm")
        ),
    };

    #endregion

    private static MissionEvent GetMissionEvent (MissionEventEnum.EventType eventType) {
        foreach (var missionEvent in AllMissionEvents) {
            if (missionEvent.EventType == eventType) {
                return missionEvent;
            }
        }

        Log.PrintWarning ("Cannot find mission event with event type : " + eventType, LogTypes.MissionEvent);
        return null;
    }

    public void StartEvent (MissionEventEnum.EventType eventType, Action onFinished = null) {
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

        StartSubEventRecursive (eventType, subEventListClone, onFinished);
    }

    private void StartSubEventRecursive (MissionEventEnum.EventType eventType, List<SubEventBase> remainingSubEventList, Action onAllFinished = null) {
        if (remainingSubEventList != null && remainingSubEventList.Count > 0) {
            var currentSubEvent = remainingSubEventList[0];
            remainingSubEventList.RemoveAt (0);

            Action onSubEventFinished = () => {
                StartSubEventRecursive (eventType, remainingSubEventList, onAllFinished);
            };

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
            if (string.IsNullOrEmpty (subEvent.AfterConfirmLocalizationKeyBase)) {
                onFinished?.Invoke ();
            } else {
                uiManager.ShowInstructionPanel (subEvent.AfterConfirmLocalizationKeyBase, onFinished);
            }
        };

        Action onAfterSetCommandFinished = () => {
            // TODO
        };

        Action onSetCommandFinished = () => {
            if (string.IsNullOrEmpty (subEvent.AfterSetCommandLocalizationKeyBase)) {
                onAfterSetCommandFinished ();
            } else {
                uiManager.ShowInstructionPanel (subEvent.AfterSetCommandLocalizationKeyBase, onAfterSetCommandFinished);
            }
        };

        Action onBeforeSetCommandFinished = () => {
            // TODO
        };

        if (string.IsNullOrEmpty(subEvent.BeforeSetCommandLocalizationKeyBase)) {
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

    private void StartMapSwitchSubEvent (MapSwitchSubEvent subEvent, Action onFinished = null) {
        mapManager.SwitchOnMapSwitch (subEvent, onFinished);
    }

    #endregion
}