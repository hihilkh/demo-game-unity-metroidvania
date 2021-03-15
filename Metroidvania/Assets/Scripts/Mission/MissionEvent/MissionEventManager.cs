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
    private static SpecialSceneEvent CurrentSpecialSceneEvent = null;
    private static int CurrentSubSpecialSceneIndex = -1;

    public static event Action MissionEventStarted;
    public static event Action MissionEventFinished;

    public static event Action SpecialSceneEventStarted;
    public static event Action SpecialSceneEventSubSceneChanging;
    public static event Action SpecialSceneEventFinished;

    #region All Events

    private const int BossMapSwitchId = 11;
    private const int BurnTreeSwitchId = 12;

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
            MissionEventEnum.EventType.LastNote,
            false,
            true,
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Event_LastNote"))
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

    private static readonly List<SpecialSceneEvent> AllSpecialSceneEvents = new List<SpecialSceneEvent> () {
        new SpecialSceneEvent (
            MissionEventEnum.SpecialSceneType.BurnTree,
            new ChangeSubSpecialSceneSubEvent (),
            new WaitSubEvent (1f),
            new MapSwitchSubEvent (BurnTreeSwitchId)
        ),

        new SpecialSceneEvent (
            MissionEventEnum.SpecialSceneType.Ending_1,
            new ChangeSubSpecialSceneSubEvent (),
            new WaitSubEvent (1f),
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Smiling, false, "Ending1_Dialog0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Smiling, true, "Ending1_Dialog1"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Smiling, false, "Ending1_Dialog2"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Ending1_Dialog3"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Ending1_Dialog4"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Ending1_Dialog5"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Ending1_Dialog6")
            ),
            new WaitSubEvent (1f)
        ),

        new SpecialSceneEvent (
            MissionEventEnum.SpecialSceneType.Ending_2_Boss,
            new ChangeSubSpecialSceneSubEvent (),
            new WaitSubEvent (1f),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.None, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Smiling, false, "Ending2_Dialog0")),
            new ChangeSubSpecialSceneSubEvent (),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Shocked, "Ending2_Dialog1")),
            new WaitSubEvent (3f),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Ending2_Dialog2")),
            new WaitSubEvent (1f),
            new ForceCharWalkSubEvent (LifeEnum.HorizontalDirection.Right),
            new WaitSubEvent (4f)
        ),

        new SpecialSceneEvent (
            MissionEventEnum.SpecialSceneType.Ending_2_NoBoss,
            new ChangeSubSpecialSceneSubEvent (),
            new WaitSubEvent (1f),
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Confused, "Ending2_NoBoss_Dialog0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, "Ending2_NoBoss_Dialog1")
            ),
            new WaitSubEvent (1f),
            new ForceCharWalkSubEvent (LifeEnum.HorizontalDirection.Right),
            new WaitSubEvent (4f)
        ),

        new SpecialSceneEvent (
            MissionEventEnum.SpecialSceneType.Ending_3,
            new ChangeSubSpecialSceneSubEvent (),
            new WaitSubEvent (1f),
            new DialogSubEvent (
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Shocked, false, "Ending3_Dialog0"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Shocked, true, "Ending3_Dialog1"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Ending3_Dialog2"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Ending3_Dialog3"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Ending3_Dialog4"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, true, "Ending3_Dialog5"),
                new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Normal, false, "Ending3_Dialog6")
            ),
            new WaitSubEvent (3f),
            new DialogSubEvent (new DialogSubEvent.DialogDetails (MissionEventEnum.Character.Player, MissionEventEnum.Expression.Normal, MissionEventEnum.Character.Boss, MissionEventEnum.Expression.Smiling, false, "Ending3_Dialog7")),
            new WaitSubEvent (1f)
        ),
    };

    #endregion

    private void OnDestroy () {
        CurrentMissionEvent = null;
        CurrentMissionSubEvent = null;
        CurrentSpecialSceneEvent = null;
        CurrentSubSpecialSceneIndex = -1;
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

    private static SpecialSceneEvent GetSpecialSceneEvent (MissionEventEnum.SpecialSceneType specialSceneTypeeventType) {
        foreach (var specialSceneEvent in AllSpecialSceneEvents) {
            if (specialSceneEvent.SpecialSceneType == specialSceneTypeeventType) {
                return specialSceneEvent;
            }
        }

        Log.PrintWarning ("Cannot find special scene event with special scene type : " + specialSceneTypeeventType, LogTypes.MissionEvent);
        return null;
    }

    #endregion

    #region Start Event

    public void StartEvent (MissionEventEnum.EventType eventType, Action onFinished = null, bool isFromCollectable = false) {
        var missionEvent = GetMissionEventWithSpecialChecking (eventType);

        if (missionEvent == null) {
            onFinished?.Invoke ();
            return;
        }

        var subEventListClone = missionEvent.GetSubEventListClone ();
        if (subEventListClone == null || subEventListClone.Count <= 0) {
            Log.PrintWarning ("Start event failed. The sub event list is empty. EventType : " + eventType, LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        Log.Print ("Start Mission Event : EventType = " + eventType, LogTypes.MissionEvent);

        CurrentMissionEvent = missionEvent;
        MissionEventStarted?.Invoke ();
        var charModel = GameUtils.FindOrSpawnChar ();

        Action reallyStartEventAction = () => {
            Log.Print ("Really start Mission Event : EventType = " + eventType, LogTypes.MissionEvent);

            Time.timeScale = 0;

            if (missionEvent.IsNeedToStopChar) {
                charModel.CancelStopChar ();
            }

            Action onEventFinished = () => {
                CurrentMissionEvent = null;

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

            StartSubEventRecursive (subEventListClone, onEventFinished);
        };

        if (missionEvent.IsNeedToStopChar) {
            charModel.StopChar (reallyStartEventAction);
        } else {
            charModel.BreakUserControl ();
            reallyStartEventAction ();
        }
    }

    private MissionEvent GetMissionEventWithSpecialChecking (MissionEventEnum.EventType eventType) {
        var runtimeEventType = eventType;
        switch (eventType) {
            case MissionEventEnum.EventType.WarningIfNoDash:
                if (UserManager.EnabledCommandList.Contains (CharEnum.Command.Dash)) {
                    return null;
                }
                break;
            case MissionEventEnum.EventType.Boss:
                if (UserManager.GetIsCollectedCollectable (Collectable.Type.Ending_1)) {
                    runtimeEventType = MissionEventEnum.EventType.Boss_Beaten;
                } else {
                    runtimeEventType = MissionEventEnum.EventType.Boss_NotYetBeaten;
                }
                break;
        }

        return GetMissionEvent (runtimeEventType);
    }


    #endregion

    #region Start Special Scene Event

    public void StartSpecialSceneEvent (MissionEventEnum.SpecialSceneType specialSceneType, Action onFinished = null) {
        var specialSceneEvent = GetSpecialSceneEventWithSpecialChecking (specialSceneType);

        if (specialSceneEvent == null) {
            onFinished?.Invoke ();
            return;
        }

        var subEventListClone = specialSceneEvent.GetSubEventListClone ();
        if (subEventListClone == null || subEventListClone.Count <= 0) {
            Log.PrintWarning ("Start Special Scene Event failed. The sub event list is empty. SpecialSceneType : " + specialSceneType, LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        Log.Print ("Start Special Scene Event : SpecialSceneType = " + specialSceneType, LogTypes.MissionEvent);

        SpecialSceneEventStarted?.Invoke ();
        CurrentSpecialSceneEvent = specialSceneEvent;
        CurrentSubSpecialSceneIndex = -1;

        var charModel = GameUtils.FindOrSpawnChar ();
        charModel.SetAllowUserControl (false);

        Action onEventFinished = () => {
            Action onFadeInFinished = () => {
                CurrentSpecialSceneEvent = null;
                CurrentSubSpecialSceneIndex = -1;

                charModel.AttachBackCamera ();
                charModel.SetAllowUserControl (true);

                SpecialSceneEventFinished?.Invoke ();
                onFinished?.Invoke ();
            };

            GameUtils.ScreenFadeIn (false, onFadeInFinished);
        };

        StartSubEventRecursive (subEventListClone, onEventFinished);
    }

    private SpecialSceneEvent GetSpecialSceneEventWithSpecialChecking (MissionEventEnum.SpecialSceneType specialSceneType) {
        var runtimeSpecialSceneType = specialSceneType;
        switch (specialSceneType) {
            case MissionEventEnum.SpecialSceneType.Ending_2:
                if (UserManager.GetIsCollectedCollectable (Collectable.Type.Ending_1)) {
                    runtimeSpecialSceneType = MissionEventEnum.SpecialSceneType.Ending_2_Boss;
                } else {
                    runtimeSpecialSceneType = MissionEventEnum.SpecialSceneType.Ending_2_NoBoss;
                }
                break;
        }

        return GetSpecialSceneEvent (runtimeSpecialSceneType);
    }

    #endregion

    #region SubEvents

    private void StartSubEventRecursive (List<SubEventBase> remainingSubEventList, Action onAllFinished = null) {
        if (remainingSubEventList != null && remainingSubEventList.Count > 0) {
            var currentSubEvent = remainingSubEventList[0];
            remainingSubEventList.RemoveAt (0);

            Action onSubEventFinished = () => {
                StartSubEventRecursive (remainingSubEventList, onAllFinished);
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
                case MissionEventEnum.SubEventType.ChangeSubSpecialScene:
                    StartChangeSubSpecialSceneSubEvent ((ChangeSubSpecialSceneSubEvent)currentSubEvent, onSubEventFinished);
                    break;
                case MissionEventEnum.SubEventType.ForceCharWalk:
                    StartForceCharWalkSubEvent ((ForceCharWalkSubEvent)currentSubEvent, onSubEventFinished);
                    break;
            }
        } else {
            CurrentMissionSubEvent = null;
            onAllFinished?.Invoke ();
        }
    }

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
        var charModel = GameUtils.FindOrSpawnChar ();
        charModel.SetAllowUserControl (false);
        Time.timeScale = 1;

        yield return new WaitForSeconds (subEvent.WaitTime);

        if (CurrentMissionEvent != null) {
            // Only allow user control for MissionEvent (but not SpecialSceneEvent)
            charModel.SetAllowUserControl (true);
        }

        Time.timeScale = 0;

        onFinished?.Invoke ();
    }

    private void StartChangeSubSpecialSceneSubEvent (ChangeSubSpecialSceneSubEvent subEvent, Action onFinished = null) {
        CurrentSubSpecialSceneIndex++;
        
        if (CurrentSpecialSceneEvent == null) {
            Log.PrintWarning ("Fail to StartChangeSubSpecialSceneSubEvent : CurrentSpecialSceneEvent is null.", LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        var sceneData = mapManager.GetSubSpecialSceneData (CurrentSpecialSceneEvent.SpecialSceneType, CurrentSubSpecialSceneIndex);
        if (sceneData == null) {
            Log.PrintWarning ("Fail to StartChangeSubSpecialSceneSubEvent : SceneData is null. SpecialSceneType = " + CurrentSpecialSceneEvent.SpecialSceneType + " ; SubSpecialSceneIndex = " + CurrentSubSpecialSceneIndex, LogTypes.MissionEvent);
            onFinished?.Invoke ();
            return;
        }

        var charModel = GameUtils.FindOrSpawnChar ();
        var bossModel = mapManager.BossModel;

        Action onFadeInFinished = () => {
            SpecialSceneEventSubSceneChanging?.Invoke ();

            charModel.DetachCameraAndSetCameraPos (sceneData.cameraPos);
            charModel.SetCaveCollapseEffect (sceneData.isNeedCaveCollapseEffect);
            if (sceneData.player == null) {
                charModel.SetActive (false);
            } else {
                charModel.SetActive (true);
                charModel.Reset (sceneData.player.pos, sceneData.player.direction);
            }

            if (bossModel != null) {
                if (sceneData.boss == null) {
                    bossModel.SetActive (false);
                } else {
                    bossModel.SetActive (true);
                    bossModel.Reset (sceneData.boss.pos, sceneData.boss.direction);
                }
            }

            GameUtils.ScreenFadeOut (false, onFinished);
        };

        GameUtils.ScreenFadeIn (false, onFadeInFinished);
    }

    private void StartForceCharWalkSubEvent (ForceCharWalkSubEvent subEvent, Action onFinished = null) {
        var charModel = GameUtils.FindOrSpawnChar ();
        charModel.Reset (charModel.GetPos (), subEvent.Direction);
        charModel.SetAllowMove (true);
        charModel.SetAllowUserControl (false);

        onFinished?.Invoke ();
    }

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