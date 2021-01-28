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

    private bool isAddedEventListeners = false;

    private void Start () {
        var missionId = 1;
        var entryId = 1;

        (var lines, var errorMsg) = AssetHandler.Instance.ReadPersistentDataFileByLines (AssetEnum.AssetType.MapData, AssetDetails.GetMapDataJSONFileName (missionId));

        if (errorMsg != null) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId + " , Error : " + errorMsg, LogType.GameFlow | LogType.Asset);
            return;
        }

        if (lines == null || lines.Length <= 0) {
            Log.PrintError ("Read map data Failed. missionId : " + missionId + " , Error : JSON file is empty", LogType.GameFlow | LogType.Asset);
            return;
        }

        var mapData = JsonUtility.FromJson<MapData> (lines[0]);
        mapManager.GenerateMap (mapData);

        charModel.SetPosAndDirection (mapData.GetEntryData(entryId));
        charModel.SetAllowMove (true);

        AddEventListeners ();
    }

    private void OnDestroy () {
        RemoveEventListeners ();
    }

    #region Events

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            MapCollectableObject.CollectedEvent += CollectCollectable;
            MapExit.ExitedEvent += Exit;
            MapSwitch.SwitchedOnEvent += OpenHiddenPath;
            MapTutorialTrigger.TriggeredTutorialEvent += StartTutorial;

            charModel.diedEvent += CharDied;
        }

    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            MapCollectableObject.CollectedEvent -= CollectCollectable;
            MapExit.ExitedEvent -= Exit;
            MapSwitch.SwitchedOnEvent -= OpenHiddenPath;
            MapTutorialTrigger.TriggeredTutorialEvent -= StartTutorial;

            charModel.diedEvent -= CharDied;

            isAddedEventListeners = false;
        }

    }

    private void CollectCollectable (MapCollectableObject collectable) {
        // TODO
    }

    private void Exit (int toEntryId) {
        // TODO
    }

    private void OpenHiddenPath (MapData.HiddenPathData hiddenPath) {
        // TODO
    }

    private void StartTutorial (TutorialEnum.GameTutorialType tutorialType) {
        // TODO
    }

    private void CharDied () {

    }
    #endregion
}