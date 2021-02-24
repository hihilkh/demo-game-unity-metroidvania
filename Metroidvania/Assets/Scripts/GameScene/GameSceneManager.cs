﻿using System;
using System.Collections;
using HihiFramework.Asset;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour {

    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private MissionEventManager missionEventManager;
    [SerializeField] private CommandPanel commandPanel;
    [SerializeField] private GamePausePanel pausePanel;
    [SerializeField] private ReadyGo readyGo;

    private CharModel charModel;
    private int selectedMissionId = -1;
    private int selectedEntryId = -1;
    private MapData mapData = null;
    private MapData.EntryData selectedEntryData = null;

    private bool isAddedEventHandlers = false;

    public static bool IsGameInitialized { get; private set; } = false;
    public static bool IsGameStarted { get; private set; } = false;

    private void Awake () {
        charModel = GameUtils.FindOrSpawnChar ();
        selectedMissionId = UserManager.SelectedMissionId;
        selectedEntryId = UserManager.SelectedEntryId;
        mapData = GetMapData (selectedMissionId);
        selectedEntryData = mapData.GetEntryData (selectedEntryId);
    }

    private void Start () {
        AddEventHandlers ();

        var isControllerByLandingScene = SceneManager.GetActiveScene ().name == GameVariable.LandingSceneName;
        ResetGame (isControllerByLandingScene);
    }

    private void OnDestroy () {
        RemoveEventHandlers ();
        charModel.LeaveGameScene ();

        IsGameInitialized = false;
        IsGameStarted = false;
    }

    #region getter

    private MapData GetMapData (int missionId) {
        string[] lines = null;
        var isSuccess = AssetHandler.Instance.TryReadPersistentDataFileByLines (AssetEnum.AssetType.MapData, AssetDetails.GetMapDataJSONFileName (missionId), out lines);

        if (!isSuccess) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId, LogTypes.GameFlow | LogTypes.Asset);
            return null;
        }

        if (lines == null || lines.Length <= 0) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId + " , Error : JSON file is empty", LogTypes.GameFlow | LogTypes.Asset);
            return null;
        }

        return JsonUtility.FromJson<MapData> (lines[0]);
    }

    public Vector2 GetSelectedEntryPos () {
        return selectedEntryData.pos;
    }

    #endregion

    #region Game Flow

    private void ResetGame (bool isControlledByLandingScene = false) {
        Action onFadeOutFinished = () => {
            if (UserManager.CheckIsFirstMissionCleared ()) {
                commandPanel.Show ();

                if (!UserManager.CheckIsDoneMissionEvent (MissionEventEnum.EventType.FirstTimeCommandPanel)) {
                    missionEventManager.StartEvent (MissionEventEnum.EventType.FirstTimeCommandPanel);
                }
            } else {
                StartGame (true);
            }
        };

        Action onFadeInFinished = () => {
            Time.timeScale = 1;

            // Remarks : To ensure everything is ready so that CharModel has no strange behaviour
            if (!isControlledByLandingScene) {
                StartCoroutine (DelayResetCharModel (IsGameInitialized));
            }

            if (IsGameInitialized) {
                Log.Print ("Reset Game", LogTypes.GameFlow);
                mapManager.ResetMap ();
            } else {
                Log.Print ("Init Game", LogTypes.GameFlow);
                IsGameInitialized = true;

                mapManager.GenerateMap (selectedMissionId, mapData);
            }

            IsGameStarted = false;

            if (!isControlledByLandingScene) {
                GameUtils.ScreenFadeOut (onFadeOutFinished);
            }
        };

        if (isControlledByLandingScene) {
            uiManager.HideUI ();
            onFadeInFinished ();
        } else {
            uiManager.ShowUI ();
            uiManager.SetUIInteractable (false);
            GameUtils.ScreenFadeIn (onFadeInFinished);
        }
    }

    private IEnumerator DelayResetCharModel (bool isGameInitialized) {
        yield return null;

        if (!isGameInitialized) {
            charModel.EnterGameScene (mapManager, mapData.boundary);
        }

        charModel.Reset (selectedEntryData);
    }

    public void StartGameFromLanding () {
        charModel.EnterGameScene (mapManager, mapData.boundary);
        uiManager.ShowUI ();
        StartGame (true, false);

        if (!UserManager.CheckIsDoneMissionEvent (MissionEventEnum.EventType.Opening)) {
            missionEventManager.StartEvent (MissionEventEnum.EventType.Opening);
        }
    }

    private void BackToCaveEntry () {
        Action onFadeOutFinished = () => {
            charModel.SetAllowMove (true);
            missionEventManager.StartEvent (MissionEventEnum.EventType.BackToCaveEntry);
        };

        Action onFadeInFinished = () => {
            charModel.Reset (selectedEntryData);
            GameUtils.ScreenFadeOut (onFadeOutFinished);
        };

        GameUtils.ScreenFadeIn (onFadeInFinished);
    }

    private void StartGame (bool isNeedToSetCachedCommandSettings = false, bool isNeedReadyGo = true) {
        Log.Print ("ReadyGo", LogTypes.GameFlow);

        Action onReadyGoFinished = () => {
            Log.Print ("Start Game", LogTypes.GameFlow);

            IsGameStarted = true;
            uiManager.SetUIInteractable (true);
            charModel.SetAllowMove (true);

            if (isNeedToSetCachedCommandSettings) {
                GameUtils.FindOrSpawnChar ().SetCommandSettings (UserManager.CommandSettingsCache);
            }
        };

        if (isNeedReadyGo) {
            readyGo.Play (onReadyGoFinished);
        } else {
            onReadyGoFinished ();
        }
    }

    private void LeaveGame () {
        Log.Print ("Leave Game", LogTypes.GameFlow);

        Action onFadeInFinished = () => {
            SceneManager.LoadScene (GameVariable.MainMenuSceneName);
        };

        GameUtils.ScreenFadeIn (onFadeInFinished);
    }

    #endregion

    #region Events

    private void AddEventHandlers () {
        if (!isAddedEventHandlers) {
            isAddedEventHandlers = true;

            MapCollectableObject.Collected += CollectedCollectableHandler;
            MapExit.ExitReached += ExitReachedHandler;
            MissionEventTrigger.MissionEventTriggered += MissionEventTriggeredHandler;

            commandPanel.PanelHid += CommandPanelHidHandler;
            charModel.Died += CharDiedHandler;

            UIEventManager.AddEventHandler (BtnOnClickType.Game_Pause, PauseBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_Restart, RestartBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_BackToMM, BackToMMBtnClickedHandler);
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            MapCollectableObject.Collected -= CollectedCollectableHandler;
            MapExit.ExitReached -= ExitReachedHandler;
            MissionEventTrigger.MissionEventTriggered -= MissionEventTriggeredHandler;

            commandPanel.PanelHid -= CommandPanelHidHandler;
            charModel.Died -= CharDiedHandler;

            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_Pause, PauseBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_Restart, RestartBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_BackToMM, BackToMMBtnClickedHandler);

            isAddedEventHandlers = false;
        }
    }

    private void CollectedCollectableHandler (MapCollectableObject collectableObject) {
        Log.Print ("Character collected collectable : " + collectableObject.ToString (), LogTypes.GameFlow | LogTypes.Char);

        var collectable = CollectableManager.GetCollectable (collectableObject.GetCollectableType ());

        if (collectable == null) {
            Log.PrintError ("Cannot find collectable. Do not do CollectCollectable action.", LogTypes.GameFlow);
            return;
        }

        // Command / BodyPart
        CharEnum.Command? enabledCommand = null;
        var obtainedBodyPart = CharEnum.BodyParts.None;
        switch (collectableObject.GetCollectableType ()) {
            case Collectable.Type.Command_Hit:
                enabledCommand = CharEnum.Command.Hit;
                obtainedBodyPart = CharEnum.BodyParts.Arms;
                break;
            case Collectable.Type.Command_Jump:
                enabledCommand = CharEnum.Command.Jump;
                obtainedBodyPart = CharEnum.BodyParts.Legs;
                break;
            case Collectable.Type.Command_Dash:
                enabledCommand = CharEnum.Command.Dash;
                obtainedBodyPart = CharEnum.BodyParts.Thrusters;
                break;
            case Collectable.Type.Command_Arrow:
                enabledCommand = CharEnum.Command.Arrow;
                obtainedBodyPart = CharEnum.BodyParts.Arrow;
                break;
            case Collectable.Type.Command_Turn:
                enabledCommand = CharEnum.Command.Turn;
                break;
        }

        // Include collect panel, note panel and coresponding event
        Action<bool> onAllActionFinished = (bool isUpdateCharCommandSettings) => {
            UserManager.CollectCollectable (UserManager.SelectedMissionId, collectableObject.GetCollectableType ());
            if (collectable.EventType != null) {
                MissionEventManager.CheckAndSetMissionEventDone ((MissionEventEnum.EventType)collectable.EventType);
            }

            if (isUpdateCharCommandSettings) {
                commandPanel.UpdateCharCommandSettings (true);
            }

            if (enabledCommand != null) {
                UserManager.EnableCommand ((CharEnum.Command)enabledCommand);
            }

            uiManager.SetUIInteractable (true);
            Time.timeScale = 1;
        };

        Action onMissionEventFinished = () => {
            onAllActionFinished (true);
        };

        // Include collect panel and note panel
        Action onAllCollectActionFinished = () => {
            if (obtainedBodyPart != CharEnum.BodyParts.None) {
                charModel.ObtainBodyPart (obtainedBodyPart);
            }

            if (collectable.EventType == null) {
                onAllActionFinished (false);
            } else {
                missionEventManager.StartEvent ((MissionEventEnum.EventType)collectable.EventType, onMissionEventFinished, true);
            }
        };

        Action onShowCollectedPanelFinished = () => {
            if (collectable is NoteCollectable) {
                uiManager.ShowNotePanel ((NoteCollectable)collectable, onAllCollectActionFinished);
            } else {
                onAllCollectActionFinished ();
            }
        };

        Action onCollectedAnimFinished = () => {
            uiManager.ShowCollectedPanel (collectable, onShowCollectedPanelFinished);
            charModel.SetAllowUserControl (true);
        };

        Time.timeScale = 0;
        charModel.SetAllowUserControl (false);
        uiManager.SetUIInteractable (false);
        collectableObject.StartCollectedAnim (charModel.GetCurrentCollectedCollectablePos (), onCollectedAnimFinished);
    }

    private void ExitReachedHandler (int toEntryId) {
        Log.Print ("Character reached exit : " + toEntryId, LogTypes.GameFlow | LogTypes.Char);

        if (toEntryId == MissionManager.CaveEntryId && selectedMissionId == MissionManager.FirstMissionId) {
            // Cannot leave the cave
            BackToCaveEntry ();
        } else {
            UserManager.ClearMission (selectedMissionId, toEntryId);
            LeaveGame ();
        }
    }

    private void MissionEventTriggeredHandler (MissionEventEnum.EventType eventType) {
        Log.Print ("Character triggered mission event : " + eventType.ToString (), LogTypes.GameFlow | LogTypes.Char);

        missionEventManager.StartEvent (eventType);
    }

    private void CommandPanelHidHandler () {
        if (IsGameStarted) {
            return;
        }

        StartGame ();
    }

    private void CharDiedHandler () {
        Log.Print ("Character died.", LogTypes.GameFlow | LogTypes.Char);
        ResetGame ();
    }

    private void PauseBtnClickedHandler (HIHIButton sender) {
        pausePanel.Show (UserManager.CommandSettingsCache);
    }

    private void RestartBtnClickedHandler (HIHIButton sender) {
        pausePanel.Hide (false);
        ResetGame ();
    }

    private void BackToMMBtnClickedHandler (HIHIButton sender) {
        LeaveGame ();
    }

    #endregion
}