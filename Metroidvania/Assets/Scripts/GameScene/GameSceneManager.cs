using System;
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
            if (UserManager.IsJustEnteredEndingMission) {
                UserManager.IsJustEnteredEndingMission = false;
                StartGame (true);
            } else if (UserManager.CheckIsFirstMissionCleared ()) {
                commandPanel.Show ();

                if (!UserManager.CheckIsDoneMissionEvent (MissionEventEnum.EventType.FirstTimeCommandPanel)) {
                    missionEventManager.StartEvent (MissionEventEnum.EventType.FirstTimeCommandPanel);
                }
            } else {
                StartGame (true);
            }
        };

        Action onFadeInFinished = () => {
            IsGameStarted = false;
            Time.timeScale = 1;

            // Remarks : To ensure everything is ready so that CharModel has no strange behaviour
            if (!isControlledByLandingScene) {
                Action onResetCharModelFinished = () => {
                    GameUtils.ScreenFadeOut (onFadeOutFinished);
                };

                StartCoroutine (DelayResetCharModel (IsGameInitialized, onResetCharModelFinished));
            }

            if (IsGameInitialized) {
                Log.Print ("Reset Game", LogTypes.GameFlow);
                mapManager.ResetMap ();
            } else {
                Log.Print ("Init Game", LogTypes.GameFlow);
                IsGameInitialized = true;

                mapManager.GenerateMap (selectedMissionId, mapData);
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

    private IEnumerator DelayResetCharModel (bool isGameInitialized, Action onFinished = null) {
        yield return null;

        if (!isGameInitialized) {
            charModel.EnterGameScene (mapManager, mapData.boundary);
        }

        charModel.Reset (selectedEntryData);

        onFinished?.Invoke ();
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

    private void LeaveGame (bool isAfterEnding = false) {
        Log.Print ("Leave Game", LogTypes.GameFlow);

        Action onFadeInFinished = () => {
            if (isAfterEnding) {
                SceneManager.LoadScene (GameVariable.LandingSceneName);
            } else {
                SceneManager.LoadScene (GameVariable.MainMenuSceneName);
            }
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
            MapExit.SpecialSceneExitReached += SpecialSceneExitReachedHandler;
            MissionEventTrigger.MissionEventTriggered += MissionEventTriggeredHandler;
            MapSwitch.Switched += MapSwitchSwitchedHandler;

            commandPanel.PanelHid += CommandPanelHidHandler;
            CharModel.Died += CharDiedHandler;

            UIEventManager.AddEventHandler (BtnOnClickType.Game_Pause, PauseBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_Restart, RestartBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_BackToMM, BackToMMBtnClickedHandler);
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            MapCollectableObject.Collected -= CollectedCollectableHandler;
            MapExit.ExitReached -= ExitReachedHandler;
            MapExit.SpecialSceneExitReached -= SpecialSceneExitReachedHandler;
            MissionEventTrigger.MissionEventTriggered -= MissionEventTriggeredHandler;
            MapSwitch.Switched -= MapSwitchSwitchedHandler;

            commandPanel.PanelHid -= CommandPanelHidHandler;
            CharModel.Died -= CharDiedHandler;

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
        var obtainedBodyPart = CharEnum.BodyParts.None;
        switch (collectableObject.GetCollectableType ()) {
            case Collectable.Type.Command_Hit:
                obtainedBodyPart = CharEnum.BodyParts.Arms;
                break;
            case Collectable.Type.Command_Jump:
                obtainedBodyPart = CharEnum.BodyParts.Legs;
                break;
            case Collectable.Type.Command_Dash:
                obtainedBodyPart = CharEnum.BodyParts.Thrusters;
                break;
            case Collectable.Type.Command_Arrow:
                obtainedBodyPart = CharEnum.BodyParts.Arrow;
                break;
        }

        // Include collect panel, note panel and coresponding event
        Action<bool> onAllActionFinished = (bool isUpdateCharCommandSettings) => {
            UserManager.CollectCollectable (collectableObject.GetCollectableType ());
            if (collectable.EventType != null) {
                MissionEventManager.CheckAndSetMissionEventDone ((MissionEventEnum.EventType)collectable.EventType);
            }

            if (isUpdateCharCommandSettings) {
                commandPanel.UpdateCharCommandSettings (true);
            }

            if (Collectable.HPUpTypeList.Contains (collectableObject.GetCollectableType ())) {
                charModel.GotHPUp ();
            } else if (Collectable.PowerUpTypeList.Contains (collectableObject.GetCollectableType ())) {
                charModel.GotPowerUp ();
            } else if (collectableObject.GetCollectableType () == Collectable.Type.FireArrow) {
                commandPanel.ClearCommandPickers ();    // To force the command panel to renew the command pickers display
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
                // Remarks :
                // Obtain body part for char model to ensure the display for the mission event is correct.
                // But note that the body part has not yet been saved to UserManager,
                // so if reset game now (or quit app and restart), it would still have not yet obtained the body part.
                // The body part would be saved to UserManager with UserManager.CollectCollectable (), which is in onAllActionFinished.
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

    private void SpecialSceneExitReachedHandler (MissionEventEnum.SpecialSceneType specialSceneType) {
        Log.Print ("Character reached special scene exit : " + specialSceneType, LogTypes.GameFlow | LogTypes.Char);

        if (specialSceneType == MissionEventEnum.SpecialSceneType.Ending_3) {
            if (!UserManager.GetIsCollectedCollectable (Collectable.Type.Ending_1)) {
                // Ending_3 must be after Ending_1
                return;
            }
        }

        Action onFinished = () => {
            switch (specialSceneType) {
                case MissionEventEnum.SpecialSceneType.Ending_2:
                    UserManager.CollectCollectable (Collectable.Type.Ending_2);
                    break;
                case MissionEventEnum.SpecialSceneType.Ending_3:
                    UserManager.CollectCollectable (Collectable.Type.Ending_3);
                    break;
            }
            
            LeaveGame (true);
        };
        missionEventManager.StartSpecialSceneEvent (specialSceneType, onFinished);
    }

    private void MissionEventTriggeredHandler (MissionEventEnum.EventType eventType) {
        Log.Print ("Character triggered mission event : " + eventType.ToString (), LogTypes.GameFlow | LogTypes.Char | LogTypes.MissionEvent);

        Action onFinished = () => {
            if (eventType == MissionEventEnum.EventType.Boss) {
                if (mapManager.BossModel == null) {
                    Log.PrintError ("Cannot get boss model in map manager.", LogTypes.GameFlow | LogTypes.MissionEvent);
                    return;
                }

                mapManager.BossModel.SetAllowMove (true);
            }
        };
        missionEventManager.StartEvent (eventType, onFinished);
    }

    private void MapSwitchSwitchedHandler (MapSwitch mapSwitch, bool isSwitchOn) {
        if (mapSwitch.GetSwitchType () == MapEnum.SwitchType.Tree) {
            Action onFinished = () => {
                UserManager.IsJustEnteredEndingMission = true;
                GameUtils.LoadGameScene (MissionManager.EndingMissionId, MissionManager.EndingEntryId);
            };

            missionEventManager.StartSpecialSceneEvent (MissionEventEnum.SpecialSceneType.BurnTree, onFinished);
        }
    }

    private void CommandPanelHidHandler () {
        if (IsGameStarted) {
            return;
        }

        StartGame ();
    }

    private void CharDiedHandler (CharEnum.CharType charType) {
        Log.Print ("Character died. charType : " + charType, LogTypes.GameFlow | LogTypes.Char);

        switch (charType) {
            case CharEnum.CharType.Player:
                ResetGame ();
                break;
            case CharEnum.CharType.Boss:
                if (UserManager.GetIsCollectedCollectable (Collectable.Type.Ending_1)) {
                    UserManager.ClearMission (selectedMissionId);
                    LeaveGame ();
                } else {
                    Action onFinished = () => {
                        UserManager.CollectCollectable (Collectable.Type.Ending_1);
                        LeaveGame (true);
                    };
                    missionEventManager.StartSpecialSceneEvent (MissionEventEnum.SpecialSceneType.Ending_1, onFinished);
                }
                break;
        }

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