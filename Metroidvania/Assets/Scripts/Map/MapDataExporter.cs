using System.Collections;
using System.Collections.Generic;
using System.IO;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class MapDataExporter : MonoBehaviour {

    [SerializeField] private CharModel charModel;
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap ground2TileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap deathTileMap;

    [SerializeField] private Vector2Int lowerBound;
    [SerializeField] private Vector2Int upperBound;
    [SerializeField] private string exportRootFolderPath;
    [SerializeField] private int missionId;

    private const string ExportFileNameFormat = "{0}.{1}";  // fileName.timestamp

    private void OnGUI () {
        if (GUI.Button (new Rect (10, 10, 150, 50), "Check Map Design")) {
            CheckMapDesign ();
        }

        if (GUI.Button (new Rect (180, 10, 150, 50), "Export MapData")) {
            ExportMapData ();
        }
    }

    private MapDataTileExportIterator GetMapDataTileExportIterator () {
        var tileMapDict = new Dictionary<MapEnum.TileMapType, Tilemap> {
            { MapEnum.TileMapType.Ground, groundTileMap},
            { MapEnum.TileMapType.Ground2, ground2TileMap},
            { MapEnum.TileMapType.SlippyWall, slippyWallTileMap},
            { MapEnum.TileMapType.Death, deathTileMap },
        };

        return new MapDataTileExportIterator (tileMapDict, lowerBound, upperBound); ;
    }

    private void CheckMapDesign () {
        Log.PrintWarning ("Start Check Map Design", LogType.MapData);

        var iterator = GetMapDataTileExportIterator ();

        foreach (var tileData in iterator) {
            if (tileData != null) {
                var mapDesignTileType = TileMapping.GetMapDesignTileType (tileData.GetTileMapType ());
                if (mapDesignTileType == null || mapDesignTileType != tileData.GetTileType ()) {
                    Log.PrintError ("Check Map Design finished : Not match. Pos : " + tileData.GetPos () + ", tileType : " + tileData.GetTileType () + " , added to TileMapType : " + tileData.GetTileMapType (), LogType.MapData);
                    return;
                }
            }
        }

        Log.PrintWarning ("Check Map Design finished : All match!", LogType.MapData);
    }

    private void ExportMapData () {
        Log.PrintWarning ("Start export MapData", LogType.MapData);

        var charData = new MapData.CharData (charModel.transform.position.x, charModel.transform.position.y, true);

        var iterator = GetMapDataTileExportIterator ();
        var tiles = new List<MapData.TileData> ();

        foreach (var tileData in iterator) {
            if (tileData != null) {
                tiles.Add (tileData);
            }
        }

        var mapData = new MapData (charData, tiles);
        var json = JsonUtility.ToJson (mapData);
        var fileName = FrameworkUtils.StringReplace (ExportFileNameFormat, AssetDetails.GetMapDataJSONFileName (missionId), FrameworkUtils.ConvertDateTimeToTimestampMS (System.DateTime.Now).ToString ());
        var filePath = Path.Combine (exportRootFolderPath, fileName);

        var isSuccess = FrameworkUtils.CreateFile (filePath, false, json);
        if (isSuccess) {
            Log.PrintWarning ("Export MapData success. Path : " + filePath, LogType.MapData);
        } else {
            Log.PrintError ("Export MapData failed. Path : " + filePath, LogType.MapData);
        }
    }
}