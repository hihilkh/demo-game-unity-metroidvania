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

    private bool isAddedEventListeners = false;

    private void Awake () {
        selectedMissionId = UserManager.SelectedMissionId;
        selectedMapEntryId = UserManager.SelectedMapEntryId;
        mapData = GetMapData (selectedMissionId);
    }

    private void Start () {
        ResetGame (true);
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

    private void ResetGame (bool isInit) {
        if (isInit) {
            mapManager.GenerateMap (selectedMissionId, mapData);
            charModel.EnterGameScene (mapManager, mapData.GetEntryData (selectedMapEntryId), mapData.boundary);

            AddEventListeners ();
        } else {
            mapManager.ResetMap ();
        }

        charModel.SetReadyForGame ();

        // TODO : Set command
        // TODO : opening animation
        StartCoroutine (StartGame (3));
    }

    private IEnumerator StartGame (float waitTime) {
        yield return new WaitForSeconds (waitTime);
        charModel.SetAllowMove (true);
    }

    private void LeaveGame () {
        // TODO : Transition
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
        UserManager.CollectedCollectable (UserManager.SelectedMissionId, collectableObject.GetCollectableType ());

        //TODO
    }

    private void Exit (int toEntryId) {
        UserManager.ClearMission (selectedMissionId, toEntryId);

        LeaveGame ();
    }

    private void StartTutorial (TutorialEnum.GameTutorialType tutorialType) {
        // TODO
    }

    private void CharDied () {
        // TODO : Transition
        ResetGame (false);
    }

    #endregion
}