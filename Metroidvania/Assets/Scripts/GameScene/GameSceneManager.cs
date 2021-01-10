using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Asset;
using HIHIFramework.Core;
using UnityEngine;

public class GameSceneManager : MonoBehaviour {

    [SerializeField] private GameSceneUIManager uiManager;
    [SerializeField] private MapGenerator mapGenerator;

    private CharModel charModel;

    private void Start () {
        var missionId = 1;
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
        mapGenerator.GenerateMap (mapData.tiles);

        charModel = GameUtils.FindOrSpawnChar ();
        charModel.SetPosAndDirection (mapData.charData.GetPos (), mapData.charData.GetDirection ());
        charModel.SetAllowMove (true);
    }
}