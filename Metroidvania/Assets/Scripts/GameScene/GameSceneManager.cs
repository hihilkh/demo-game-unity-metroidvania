using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Asset;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour {

    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MapManager mapManager;

    private CharModel _charModel = null;
    private CharModel charModel {
        get {
            if (_charModel == null) {
                _charModel = GameUtils.FindOrSpawnChar ();
            }

            return _charModel;
        }
    }

    private int selectedMissionId = -1;
    private int selectedMapEntryId = -1;
    private MapData mapData = null;
    private MapData.EntryData selectedMapEntryData = null;

    protected bool isGameInitialized = false;
    private bool isAddedEventListeners = false;

    private void Awake () {
        selectedMissionId = UserManager.SelectedMissionId;
        selectedMapEntryId = UserManager.SelectedMapEntryId;
        mapData = GetMapData (selectedMissionId);
        selectedMapEntryData = mapData.GetEntryData (selectedMapEntryId);
    }

    private void Start () {
        ResetGame ();
    }

    private void OnDestroy () {
        RemoveEventListeners ();
        charModel.LeaveGameScene ();
    }

    #region Game Flow

    private MapData GetMapData (int missionId) {
        string[] lines = null;
        var isSuccess = AssetHandler.Instance.TryReadPersistentDataFileByLines (AssetEnum.AssetType.MapData, AssetDetails.GetMapDataJSONFileName (missionId), out lines);

        if (!isSuccess) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId, LogType.GameFlow | LogType.Asset);
            return null;
        }

        if (lines == null || lines.Length <= 0) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId + " , Error : JSON file is empty", LogType.GameFlow | LogType.Asset);
            return null;
        }

        return JsonUtility.FromJson<MapData> (lines[0]);
    }

    private void ResetGame () {
        if (isGameInitialized) {
            Log.Print ("Reset Game", LogType.GameFlow);
            mapManager.ResetMap ();
        } else {
            Log.Print ("Init Game", LogType.GameFlow);
            isGameInitialized = true;

            mapManager.GenerateMap (selectedMissionId, mapData);
            charModel.EnterGameScene (mapManager, mapData.boundary);

            AddEventListeners ();
        }

        charModel.Reset (selectedMapEntryData);

        // TODO : Set command
        // TODO : opening animation
        StartCoroutine (StartGame (3));
    }

    private IEnumerator StartGame (float waitTime) {
        yield return new WaitForSeconds (waitTime);

        Log.Print ("Start Game", LogType.GameFlow);
        charModel.SetAllowMove (true);
    }

    private void LeaveGame () {
        // TODO : Transition
        Log.Print ("Leave Game", LogType.GameFlow);
        SceneManager.LoadScene (GameVariable.MainMenuSceneName);
    }

    #endregion

    #region Events

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            MapCollectableObject.CollectedEvent += CollectCollectable;
            MapExit.ExitedEvent += Exit;
            MapTutorialTrigger.TriggeredTutorialEvent += StartTutorial;

            charModel.diedEvent += CharDied;
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            MapCollectableObject.CollectedEvent -= CollectCollectable;
            MapExit.ExitedEvent -= Exit;
            MapTutorialTrigger.TriggeredTutorialEvent -= StartTutorial;

            charModel.diedEvent -= CharDied;

            isAddedEventListeners = false;
        }
    }

    private void CollectCollectable (MapCollectableObject collectableObject) {
        Log.Print ("Character collected collectable : " + collectableObject.ToString (), LogType.GameFlow | LogType.Char);

        var collectable = CollectableManager.GetCollectable (collectableObject.GetCollectableType ());

        if (collectable == null) {
            Log.PrintError ("Cannot find collectable. Do not do CollectCollectable action.", LogType.GameFlow);
            return;
        }

        Time.timeScale = 0;

        // Include collect panel, note panel and coresponding event
        Action onAllActionFinished = () => {
            UserManager.CollectedCollectable (UserManager.SelectedMissionId, collectableObject.GetCollectableType ());

            // Command / BodyPart
            CharEnum.Command? enabledCommand = null;
            var obtainedBodyPart = CharEnum.BodyPart.None;
            switch (collectableObject.GetCollectableType ()) {
                case Collectable.Type.Command_Hit:
                    enabledCommand = CharEnum.Command.Hit;
                    obtainedBodyPart = CharEnum.BodyPart.Arms;
                    break;
                case Collectable.Type.Command_Jump:
                    enabledCommand = CharEnum.Command.Jump;
                    obtainedBodyPart = CharEnum.BodyPart.Legs;
                    break;
                case Collectable.Type.Command_Dash:
                    enabledCommand = CharEnum.Command.Dash;
                    obtainedBodyPart = CharEnum.BodyPart.Thrusters;
                    break;
                case Collectable.Type.Command_Arrow:
                    enabledCommand = CharEnum.Command.Arrow;
                    obtainedBodyPart = CharEnum.BodyPart.Arrow;
                    break;
                case Collectable.Type.Command_Turn:
                    enabledCommand = CharEnum.Command.Turn;
                    break;
            }

            if (enabledCommand != null) {
                UserManager.EnableCommand ((CharEnum.Command)enabledCommand);
            }

            if (obtainedBodyPart != CharEnum.BodyPart.None) {
                charModel.ObtainBodyPart (obtainedBodyPart);
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

        collectableObject.StartCollectedAnim (charModel.GetCurrentCollectedCollectablePos (), onCollectedAnimFinished);
    }

    private void Exit (int toEntryId) {
        Log.Print ("Character reached exit : " + toEntryId, LogType.GameFlow | LogType.Char);
        UserManager.ClearMission (selectedMissionId, toEntryId);

        LeaveGame ();
    }

    private void StartTutorial (TutorialEnum.GameTutorialType tutorialType) {
        Log.Print ("Character triggered tutorial : " + tutorialType.ToString (), LogType.GameFlow | LogType.Char);
        // TODO
    }

    private void CharDied () {
        Log.Print ("Character died.", LogType.GameFlow | LogType.Char);
        // TODO : Transition
        ResetGame ();
    }

    #endregion
}