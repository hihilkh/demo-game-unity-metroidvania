using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Asset;
using HIHIFramework.Core;
using UnityEngine;

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

    private int currentMissionId = -1;

    private bool isAddedEventListeners = false;

    private void Start () {
        //TODO
        currentMissionId = 1;
        var entryId = 1;

        string[] lines = null;
        var isSuccess = AssetHandler.Instance.TryReadPersistentDataFileByLines (AssetEnum.AssetType.MapData, AssetDetails.GetMapDataJSONFileName (currentMissionId), out lines);

        if (!isSuccess) {
            Log.PrintError ("Read map data Failed. missionId : " + currentMissionId, LogType.GameFlow | LogType.Asset);
            return;
        }

        if (lines == null || lines.Length <= 0) {
            Log.PrintError ("Read map data Failed. missionId : " + currentMissionId + " , Error : JSON file is empty", LogType.GameFlow | LogType.Asset);
            return;
        }

        var mapData = JsonUtility.FromJson<MapData> (lines[0]);
        mapManager.GenerateMap (currentMissionId, mapData);

        charModel.EnterGameScene (mapManager, mapData.GetEntryData(entryId), mapData.boundary);
        charModel.SetAllowMove (true);

        AddEventListeners ();
    }

    private void OnDestroy () {
        RemoveEventListeners ();
        charModel.LeaveGameScene ();
    }

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
        GameProgress.CollectedCollectable (currentMissionId, collectableObject.GetCollectableType ());

        //TODO
    }

    private void Exit (int toEntryId) {
        // TODO
    }

    private void MapSwitchSwitchedOn (MapSwitch mapSwitch) {
        // TODO
    }

    private void StartTutorial (TutorialEnum.GameTutorialType tutorialType) {
        // TODO
    }

    private void CharDied () {

    }
    #endregion
}