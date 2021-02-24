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

    public static event Action MissionEventStarted;
    public static event Action MissionEventFinished;

    #region All mission events

    private const int BossMapSwitchId = 11;

    private static readonly List<MissionEvent> AllMissionEvents = new List<MissionEvent> () {
        new MissionEvent (
            MissionEventEnum.EventType.Command_Hit,
            false,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Hit")),
            new CommandPanelSubEvent (CharEnum.Command.Hit, CharEnum.InputSituation.GroundTap, "Event_Command_Hit_Panel", null)
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Command_Jump,
            false,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Jump")),
            new CommandPanelSubEvent (CharEnum.Command.Jump, CharEnum.InputSituation.GroundHold, "Event_Command_Jump_Panel", "Event_Command_Jump_Panel_Done"),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundRelease, "Event_Command_Jump_Instruction")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Command_Dash,
            false,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Command_Dash")),
            new CommandPanelSubEvent (CharEnum.Command.Dash, CharEnum.InputSituation.AirHold, "Event_Command_Dash_Panel", null)
        ),

        new MissionEvent (
            MissionEventEnum.EventType.FirstHit,
            false,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_FirstHit")),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundTap, "Event_FirstHit_Instruction0"),
            new WaitSubEvent (1.5f),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundTap, "Event_FirstHit_Instruction1")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.TouchWallAndTurn,
            false,
            true,
            new InstructionSubEvent ("Event_TouchWallAndTurn")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.AirJump,
            true,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_AirJump")),
            new CommandPanelSubEvent (CharEnum.Command.Jump, CharEnum.InputSituation.AirTap, "Event_AirJump_Panel", null),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundRelease, "Event_AirJump_Instruction0"),
            new WaitSubEvent (0.6f),
            new CommandInputSubEvent (CharEnum.InputSituation.AirTap, "Event_AirJump_Instruction1")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.WallAction,
            true,
            true,
            new CommandInputSubEvent (CharEnum.InputSituation.GroundRelease, "Event_WallAction_Instruction0"),
            new WaitSubEvent (1f),
            new CommandInputSubEvent (CharEnum.InputSituation.AirTap, "Event_WallAction_Instruction1"),
            new WaitSubEvent (0.8f),
            new CommandInputSubEvent (CharEnum.InputSituation.AirTap, "Event_WallAction_Instruction2")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.AirFinishingHit,
            true,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_AirFinishingHit")),
            new CommandPanelSubEvent (CharEnum.Command.Hit, CharEnum.InputSituation.AirRelease, "Event_AirFinishingHit_Panel", null),
            new CommandInputSubEvent (CharEnum.InputSituation.GroundRelease, "Event_AirFinishingHit_Instruction0"),
            new WaitSubEvent (1f),
            new CommandInputSubEvent (CharEnum.InputSituation.AirRelease, "Event_AirFinishingHit_Instruction1")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.CameraMovement,
            true,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_CameraMovement")),
            new CameraInputSubEvent (CharEnum.LookDirections.Down, "Event_CameraMovement_Instruction0", "Event_CameraMovement_Instruction1"),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_CameraMovement_ValleyTip"))
        ),

        new MissionEvent (
            MissionEventEnum.EventType.WarningIfNoDash,
            true,
            false,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_WarningIfNoDash")),
            new CameraInputSubEvent (CharEnum.LookDirections.Right, null, null),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Event_WarningIfNoDash_Tip"))
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Boss_NotYetBeaten,
            true,
            false,
            new MapSwitchSubEvent (BossMapSwitchId),
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Event_Boss_NotYetBeaten_Dialog0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Event_Boss_NotYetBeaten_Dialog1"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Event_Boss_NotYetBeaten_Dialog2"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Event_Boss_NotYetBeaten_Dialog3")
            )
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Boss_Beaten,
            true,
            false,
            new MapSwitchSubEvent (BossMapSwitchId),
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Event_Boss_Beaten_Dialog")
            )
        ),

        new MissionEvent (
            MissionEventEnum.EventType.Opening,
            false,
            true,
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Confused, "Event_Opening0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_Opening1")
            )
        ),

        new MissionEvent (
            MissionEventEnum.EventType.FirstTimeCommandPanel,
            false,
            true,
            new InstructionSubEvent ("Event_FirstTimeCommandPanel")
        ),

        new MissionEvent (
            MissionEventEnum.EventType.BackToCaveEntry,
            false,
            false,
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Confused, "Event_BackToCaveEntry0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_BackToCaveEntry1")
            )
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

    public void StartEvent (MissionEventEnum.EventType eventType, Action onFinished = null, bool isFromCollectable = false) {
        var missionEvent = GetMissionEventWithSpecialEventChecking (eventType);

        if (missionEvent == null) {
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
        MissionEventStarted?.Invoke ();
        var charModel = GameUtils.FindOrSpawnChar ();

        Action reallyStartEventAction = () => {
            Log.Print ("Really start Mission Event : eventType = " + eventType, LogTypes.MissionEvent);

            Time.timeScale = 0;

            if (missionEvent.IsNeedToStopChar) {
                charModel.CancelStopChar ();
            }

            Action onEventFinished = () => {
                CurrentMissionEvent = null;
                CurrentMissionSubEvent = null;

                Time.timeScale = 1;

                // If it is from collectable, set mission event done together with UserManager.CollectCollectable ()
                if (!isFromCollectable) {
                    CheckAndSetMissionEventDone (missionEvent.EventType);

                    if (missionEvent.CheckHasSubEventType (MissionEventEnum.SubEventType.CommandPanel)) {
                        commandPanel.UpdateCharCommandSettings (true);
                    }
                }

                MissionEventFinished?.Invoke ();
                onFinished?.Invoke ();
            };

            StartSubEventRecursive (eventType, subEventListClone, onEventFinished);
        };

        if (missionEvent.IsNeedToStopChar) {
            charModel.StopChar (reallyStartEventAction);
        } else {
            charModel.BreakUserControl ();
            reallyStartEventAction ();
        }
    }

    private MissionEvent GetMissionEventWithSpecialEventChecking (MissionEventEnum.EventType eventType) {
        var runtimeEventType = eventType;
        switch (eventType) {
            case MissionEventEnum.EventType.WarningIfNoDash:
                if (UserManager.EnabledCommandList.Contains (CharEnum.Command.Dash)) {
                    return null;
                }
                break;
            case MissionEventEnum.EventType.Boss:
                if (UserManager.GetAllCollectedCollectable ().Contains (Collectable.Type.Ending_1)) {
                    runtimeEventType = MissionEventEnum.EventType.Boss_Beaten;
                } else {
                    runtimeEventType = MissionEventEnum.EventType.Boss_NotYetBeaten;
                }
                break;
        }

        return GetMissionEvent (runtimeEventType);
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
                case MissionEventEnum.SubEventType.Instructrion:
                    StartInstructionSubEvent ((InstructionSubEvent)currentSubEvent, onSubEventFinished);
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

    private void StartInstructionSubEvent (InstructionSubEvent subEvent, Action onFinished = null) {
        uiManager.ShowInstructionPanel (subEvent.LocalizationKeyBase, onFinished);
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
            tutorialFinger.Hide ();
            if (string.IsNullOrEmpty (subEvent.AfterInputLocalizationKeyBase)) {
                onFinished?.Invoke ();
            } else {
                uiManager.ShowInstructionPanel (subEvent.AfterInputLocalizationKeyBase, onFinished);
            }
        };

        Action onBeforeInputFinished = () => {
            tutorialFinger.ShowDragAndDrop_LeftScreen (subEvent.LookDirections);
            GameUtils.FindOrSpawnChar ().SetCameraInputMissionEvent (subEvent, onInputFinished);
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
            charModel.SetAllowUserControl (true, false, true);
            charModel.SetCommandInputMissionEvent (subEvent, onInputFinished);
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

    #region Done Event

    public static void CheckAndSetMissionEventDone (MissionEventEnum.EventType eventType) {
        var missionEvent = GetMissionEvent (eventType);

        if (missionEvent == null) {
            Log.PrintWarning ("CheckAndSetMissionEventDone failed. MissionEvent is null. eventType : " + eventType, LogTypes.MissionEvent);
            return;
        }

        CheckAndSetMissionEventDone (missionEvent);
    }

    public static void CheckAndSetMissionEventDone (MissionEvent missionEvent) {
        if (missionEvent == null) {
            Log.PrintWarning ("CheckAndSetMissionEventDone failed. missionEvent is null.", LogTypes.MissionEvent);
            return;
        }

        if (missionEvent.IsOneTimeEvent) {
            UserManager.SetMissionEventDone (missionEvent.EventType);
        }
    }

    #endregion
}