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
    [SerializeField] private CommandPanel commandPanel;
    [SerializeField] private GamePausePanel pausePanel;
    [SerializeField] private ReadyGo readyGo;

    private CharModel _character = null;
    private CharModel Character {
        get {
            if (_character == null) {
                _character = GameUtils.FindOrSpawnChar ();
            }

            return _character;
        }
    }

    private int selectedMissionId = -1;
    private int selectedEntryId = -1;
    private MapData mapData = null;
    private MapData.EntryData selectedEntryData = null;

    private bool isGameInitialized = false;
    private bool isAddedEventListeners = false;

    private void Awake () {
        selectedMissionId = UserManager.SelectedMissionId;
        selectedEntryId = UserManager.SelectedEntryId;
        mapData = GetMapData (selectedMissionId);
        selectedEntryData = mapData.GetEntryData (selectedEntryId);
    }

    private void Start () {
        AddEventListeners ();
        ResetGame ();
    }

    private void OnDestroy () {
        RemoveEventListeners ();
        Character.LeaveGameScene ();
    }

    #region Game Flow

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

    private void ResetGame () {

        Action onFadeOutFinished = () => {
            commandPanel.Show (UserManager.EnabledCommandList, UserManager.CommandSettingsCache);
        };

        Action onFadeInFinished = () => {
            Time.timeScale = 1;

            // Remarks : To ensure everything is ready so that CharModel has no strange behaviour 
            StartCoroutine (DelayResetCharModel (isGameInitialized));

            if (isGameInitialized) {
                Log.Print ("Reset Game", LogTypes.GameFlow);
                mapManager.ResetMap ();
            } else {
                Log.Print ("Init Game", LogTypes.GameFlow);
                isGameInitialized = true;

                mapManager.GenerateMap (selectedMissionId, mapData);
            }

            GameUtils.ScreenFadeOut (onFadeOutFinished);
        };

        uiManager.ResetGame ();
        GameUtils.ScreenFadeIn (onFadeInFinished);
    }

    private IEnumerator DelayResetCharModel (bool isGameInitialized) {
        yield return null;

        if (!isGameInitialized) {
            Character.EnterGameScene (mapManager, mapData.boundary);
        }

        Character.Reset (selectedEntryData);
    }

    private void StartGame () {
        Log.Print ("ReadyGo", LogTypes.GameFlow);

        Action onReadyGoFinished = () => {
            Log.Print ("Start Game", LogTypes.GameFlow);

            uiManager.StartGame ();
            Character.SetAllowMove (true);
        };

        readyGo.Play (onReadyGoFinished);
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

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            MapCollectableObject.Collected += CollectedCollectableHandler;
            MapExit.ExitReached += ExitReachedHandler;
            MapTutorialTrigger.TutorialTriggered += TutorialTriggeredHandler;

            commandPanel.PanelHid += CommandPanelHidHandler;
            Character.Died += CharDiedHandler;

            UIEventManager.AddEventHandler (BtnOnClickType.Game_Pause, PauseBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_Restart, RestartBtnClickedHandler);
            UIEventManager.AddEventHandler (BtnOnClickType.Game_BackToMM, BackToMMBtnClickedHandler);
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            MapCollectableObject.Collected -= CollectedCollectableHandler;
            MapExit.ExitReached -= ExitReachedHandler;
            MapTutorialTrigger.TutorialTriggered -= TutorialTriggeredHandler;

            commandPanel.PanelHid -= CommandPanelHidHandler;
            Character.Died -= CharDiedHandler;

            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_Pause, PauseBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_Restart, RestartBtnClickedHandler);
            UIEventManager.RemoveEventHandler (BtnOnClickType.Game_BackToMM, BackToMMBtnClickedHandler);

            isAddedEventListeners = false;
        }
    }

    private void CollectedCollectableHandler (MapCollectableObject collectableObject) {
        Log.Print ("Character collected collectable : " + collectableObject.ToString (), LogTypes.GameFlow | LogTypes.Char);

        var collectable = CollectableManager.GetCollectable (collectableObject.GetCollectableType ());

        if (collectable == null) {
            Log.PrintError ("Cannot find collectable. Do not do CollectCollectable action.", LogTypes.GameFlow);
            return;
        }

        Time.timeScale = 0;

        // Include collect panel, note panel and coresponding event
        Action onAllActionFinished = () => {
            UserManager.CollectCollectable (UserManager.SelectedMissionId, collectableObject.GetCollectableType ());

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

            if (enabledCommand != null) {
                UserManager.EnableCommand ((CharEnum.Command)enabledCommand);
            }

            if (obtainedBodyPart != CharEnum.BodyParts.None) {
                Character.ObtainBodyPart (obtainedBodyPart);
            }
            
            Time.timeScale = 1;
        };

        // Include collect panel and note panel
        Action onAllCollectActionFinished = () => {
            // TODO : events
            onAllActionFinished ();
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
        };

        collectableObject.StartCollectedAnim (Character.GetCurrentCollectedCollectablePos (), onCollectedAnimFinished);
    }

    private void ExitReachedHandler (int toEntryId) {
        Log.Print ("Character reached exit : " + toEntryId, LogTypes.GameFlow | LogTypes.Char);
        UserManager.ClearMission (selectedMissionId, toEntryId);

        LeaveGame ();
    }

    private void TutorialTriggeredHandler (TutorialEnum.GameTutorialType tutorialType) {
        Log.Print ("Character triggered tutorial : " + tutorialType.ToString (), LogTypes.GameFlow | LogTypes.Char);
        // TODO
    }

    private void CommandPanelHidHandler () {
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