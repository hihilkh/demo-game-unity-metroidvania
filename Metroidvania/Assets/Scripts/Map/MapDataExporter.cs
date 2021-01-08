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
    [SerializeField] private Tilemap wallTileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap roofTileMap;
    [SerializeField] private Tilemap deathTileMap;

    [SerializeField] private Vector2Int lowerBound;
    [SerializeField] private Vector2Int upperBound;
    [SerializeField] private string exportRootFolderPath;
    [SerializeField] private int missionId;

    private const string ExportFileNameFormat = "{0}.{1}";  // fileName.timestamp

    private void OnGUI () {
        if (GUI.Button (new Rect (10, 10, 150, 50), "Export MapData")) {
            ExportMapData ();
        }
    }

    private void ExportMapData () {
        Log.Print ("Start export MapData");

        var tileMapDict = new Dictionary<MapEnum.TileTag, Tilemap> {
            { MapEnum.TileTag.Ground, groundTileMap},
            { MapEnum.TileTag.Wall, wallTileMap},
            { MapEnum.TileTag.SlippyWall, slippyWallTileMap},
            { MapEnum.TileTag.Roof, roofTileMap},
            { MapEnum.TileTag.Death, deathTileMap }
        };

        var charData = new MapData.CharData (charModel.transform.position.x, charModel.transform.position.y, true);

        var tiles = new List<MapData.TileData> ();
        for (var x = lowerBound.x; x <= upperBound.x; x++) {
            for (var y = lowerBound.y; y <= upperBound.y; y++) {

                var pos = new Vector3Int (x, y, GameVariable.TilePosZ);

                foreach (var pair in tileMapDict) {
                    var tile = pair.Value.GetTile (pos);
                    if (tile != null) {
                        var tileType = TileMapping.GetTileType (tile.name);
                        if (tileType == null) {
                            Log.PrintError ("Export MapData failed. Cannot get tile type.");
                            return;
                        }

                        var tileData = new MapData.TileData (x, y, (MapEnum.TileType)tileType, pair.Key);
                        tiles.Add (tileData);
                        break;   // Skip other tilemap searching
                    }
                }
            }
        }

        var mapData = new MapData (charData, tiles);
        var json = JsonUtility.ToJson (mapData);
        var fileName = FrameworkUtils.StringReplace (ExportFileNameFormat, AssetDetails.GetMapDataJSONFileName (missionId), FrameworkUtils.ConvertDateTimeToTimestampMS (System.DateTime.Now).ToString ());
        var filePath = Path.Combine (exportRootFolderPath, fileName);

        var isSuccess = FrameworkUtils.CreateFile (filePath, false, json);
        if (isSuccess) {
            Log.Print ("Export MapData success. Path : " + filePath);
        } else {
            Log.PrintError ("Export MapData failed. Path : " + filePath);
        }
    }
}