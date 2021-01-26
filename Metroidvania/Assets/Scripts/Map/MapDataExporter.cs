using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteInEditMode]
public class MapDataExporter : MonoBehaviour {

    [Header ("Export Info")]
    [SerializeField] private string exportRootFolderPath;

    [Header ("Tilemap")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap ground2TileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap deathTileMap;
    [SerializeField] private Tilemap bgTileMap;

    [Header ("Mission Related")]
    [SerializeField] private int missionId;
    [SerializeField] private RectTransform exportRange;
    [SerializeField] private Transform otherStuffsBaseTransform;

    private const string ExportFileNameFormat = "{0}.{1}";  // fileName.timestamp

    private const string SwitchesBaseTransformName = "Switches";
    private const string EntriesBaseTransformName = "Entries";
    private const string EnemiesBaseTransformName = "Enemies";
    private const string CollectablesBaseTransformName = "Collectables";
    private const string ExitsBaseTransformName = "Exits";
    private const string TutorialsBaseTransformName = "Tutorials";

    private const string HiddenPathSuffix = "_HiddenPath";

    private void OnGUI () {
        if (GUI.Button (new Rect (10, 10, 150, 50), "Check Map Design")) {
            CheckMapDesign ();
        }

        if (GUI.Button (new Rect (180, 10, 150, 50), "Export MapData")) {
            ExportMapData ();
        }
    }

    private MapDataTileExportIterator GetMapDataTileExportIterator (MapData.Boundary boundary) {
        var tileMapDict = new Dictionary<MapEnum.TileMapType, Tilemap> {
            { MapEnum.TileMapType.Ground, groundTileMap},
            { MapEnum.TileMapType.Ground2, ground2TileMap},
            { MapEnum.TileMapType.SlippyWall, slippyWallTileMap},
            { MapEnum.TileMapType.Death, deathTileMap },
            { MapEnum.TileMapType.Background, bgTileMap },
        };

        return new MapDataTileExportIterator (tileMapDict, boundary.GetLowerBound (), boundary.GetUpperBound ());
    }

    private void CheckMapDesign () {
        Log.PrintWarning ("Start Check Map Design", LogType.MapData);

        var boundary = ExportBoundary ();
        var iterator = GetMapDataTileExportIterator (boundary);

        foreach (var tileData in iterator) {
            if (tileData != null) {
                var tileMapType = tileData.GetTileMapType ();
                if (tileMapType == MapEnum.TileMapType.Background) {
                    if (TileMapping.CheckIsMapDesignTileType (tileData.GetTileType ())) {
                        Log.PrintError ("Check Map Design finished : Not match. Pos : " + tileData.GetPos () + ", background tile using map design tile type", LogType.MapData);
                        return;
                    }
                } else {
                    var mapDesignTileType = TileMapping.GetMapDesignTileType (tileMapType);
                    if (mapDesignTileType == null || mapDesignTileType != tileData.GetTileType ()) {
                        Log.PrintError ("Check Map Design finished : Not match. Pos : " + tileData.GetPos () + ", tileType : " + tileData.GetTileType () + " , added to TileMapType : " + tileData.GetTileMapType (), LogType.MapData);
                        return;
                    }
                }
            }
        }

        Log.PrintWarning ("Check Map Design finished : All match!", LogType.MapData);
    }

    #region Export

    private void ExportMapData () {
        Log.PrintWarning ("Start export MapData", LogType.MapData);

        var mapData = new MapData ();
        var boundary = ExportBoundary ();
        mapData.boundary = boundary;
        mapData.tiles = ExportTileDataList (boundary);
        mapData.switches = ExportSwitchDataList ();
        mapData.entries = ExportEntryDataList ();
        mapData.enemies = ExportEnemyDataList ();
        mapData.collectables = ExportCollectableDataList (mapData.enemies);
        mapData.exits = ExportExitDataList ();
        mapData.tutorials = ExportTutorialDataList ();

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

    private MapData.Boundary ExportBoundary () {
        Log.PrintWarning ("Start export Boundary", LogType.MapData);

        var lowerBound = new Vector2Int (Mathf.FloorToInt (exportRange.position.x), Mathf.FloorToInt (exportRange.position.y));
        var upperBound = new Vector2Int (Mathf.CeilToInt (exportRange.position.x + exportRange.sizeDelta.x), Mathf.CeilToInt (exportRange.position.y + exportRange.sizeDelta.y));
        var boundary = new MapData.Boundary (lowerBound, upperBound);

        Log.PrintWarning ("Export Boundary success", LogType.MapData);
        return boundary;
    }

    private List<MapData.TileData> ExportTileDataList (MapData.Boundary boundary) {
        Log.PrintWarning ("Start export TileData", LogType.MapData);

        var iterator = GetMapDataTileExportIterator (boundary);
        var tileDataList = new List<MapData.TileData> ();

        foreach (var tileData in iterator) {
            if (tileData != null) {
                tileDataList.Add (tileData);
            }
        }

        Log.PrintWarning ("Export TileData success", LogType.MapData);
        return tileDataList;
    }

    private List<MapData.SwitchData> ExportSwitchDataList () {
        Log.PrintWarning ("Start export SwitchData", LogType.MapData);

        var result = new List<MapData.SwitchData> ();
        var baseTransform = otherStuffsBaseTransform.Find (SwitchesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export SwitchData failed. Cannot find the base transform.");
        }

        var switchTransformList = new List<Transform> ();
        foreach (Transform child in baseTransform) {
            if (child.name.Contains (FrameworkVariable.DefaultDelimiter)) {
                switchTransformList.Add (child);
            }
        }

        foreach (var switchTransform in switchTransformList) {
            var array = switchTransform.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 3) {
                throw new Exception ("Export SwitchData failed. Invalid transform name : " + switchTransform.name);
            }

            var switchName = array[0];
            MapEnum.SwitchType switchType;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[1]), out switchType)) {
                throw new Exception ("Export SwitchData failed. Invalid SwitchType : " + array[1]);
            }
            MapEnum.HiddenPathOpenType hiddenPathOpenType;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[2]), out hiddenPathOpenType)) {
                throw new Exception ("Export SwitchData failed. Invalid HiddenPathOpenType : " + array[2]);
            }

            var hiddenPathTransformName = switchName + HiddenPathSuffix;
            var hiddenPathTransform = baseTransform.Find (hiddenPathTransformName);
            if (baseTransform == null) {
                throw new Exception ("Export SwitchData failed. Cannot find hiddenPathTransform : " + hiddenPathTransformName);
            }
            var spriteRenderer = hiddenPathTransform.GetComponent<SpriteRenderer> ();
            if (spriteRenderer == null) {
                throw new Exception ("Export SwitchData failed. Cannot find spriteRenderer : " + hiddenPathTransformName);
            }

            // Remarks :
            // - Assume the setting is correct and the calculated result is always an integer.
            // - Must use local position in order to get the correct tile position instead of world position
            var hiddenPathMinPosX = (int)(hiddenPathTransform.localPosition.x - ((spriteRenderer.size.x - 1) / 2));
            var hiddenPathMinPosY = (int)(hiddenPathTransform.localPosition.y - ((spriteRenderer.size.y - 1) / 2));
            var hiddenPathTilesPos = new List<Vector2Int> ();
            for (int x = 0; x < spriteRenderer.size.x; x++) {
                for (int y = 0; y < spriteRenderer.size.y; y++) {
                    hiddenPathTilesPos.Add (new Vector2Int (hiddenPathMinPosX + x, hiddenPathMinPosY + y));
                }
            }

            result.Add (new MapData.SwitchData ((int)switchTransform.localPosition.x, (int)switchTransform.localPosition.y, switchType, hiddenPathOpenType, hiddenPathTilesPos));
        }


        Log.PrintWarning ("Export SwitchData success", LogType.MapData);
        return result;
    }

    private List<MapData.EntryData> ExportEntryDataList () {
        Log.PrintWarning ("Start export EntryData", LogType.MapData);

        var result = new List<MapData.EntryData> ();
        var baseTransform = otherStuffsBaseTransform.Find (EntriesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export EntryData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 2) {
                throw new Exception ("Export EntryData failed. Invalid transform name : " + child.name);
            }

            var entryId = int.Parse (array[0]);
            LifeEnum.HorizontalDirection direction;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[1]), out direction)) {
                throw new Exception ("Export EntryData failed. Invalid HorizontalDirection : " + array[1]);
            }

            result.Add (new MapData.EntryData (child.position.x, child.position.y, direction, entryId));
        }

        Log.PrintWarning ("Export EntryData success", LogType.MapData);
        return result;
    }

    private List<MapData.EnemyData> ExportEnemyDataList () {
        Log.PrintWarning ("Start export EnemyData", LogType.MapData);

        var result = new List<MapData.EnemyData> ();
        var baseTransform = otherStuffsBaseTransform.Find (EnemiesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export EnemyData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 3) {
                throw new Exception ("Export EnemyData failed. Invalid transform name : " + child.name);
            }

            var enemyId = int.Parse (array[0]);
            EnemyEnum.EnemyType enemyType;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[1]), out enemyType)) {
                throw new Exception ("Export EnemyData failed. Invalid EnemyType : " + array[1]);
            }
            LifeEnum.HorizontalDirection direction;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[2]), out direction)) {
                throw new Exception ("Export EnemyData failed. Invalid HorizontalDirection : " + array[2]);
            }

            result.Add (new MapData.EnemyData (child.position.x, child.position.y, direction, enemyId, enemyType));
        }

        Log.PrintWarning ("Export EnemyData success", LogType.MapData);
        return result;
    }

    private List<MapData.CollectableData> ExportCollectableDataList (List<MapData.EnemyData> enemies) {
        Log.PrintWarning ("Start export CollectableData", LogType.MapData);

        var result = new List<MapData.CollectableData> ();
        var baseTransform = otherStuffsBaseTransform.Find (CollectablesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export CollectableData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 2) {
                throw new Exception ("Export CollectableData failed. Invalid transform name : " + child.name);
            }

            MissionCollectable.Type collectableType;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[0]), out collectableType)) {
                throw new Exception ("Export CollectableData failed. Invalid MissionCollectable.Type : " + array[0]);
            }

            if (string.IsNullOrEmpty (array[1])) {
                result.Add (new MapData.CollectableData (child.position.x, child.position.y, collectableType));
            } else {
                var fromEnemyId = int.Parse (array[1]);
                var isEnemyMatched = false;
                foreach (var enemy in enemies) {
                    if (enemy.id == fromEnemyId) {
                        isEnemyMatched = true;
                        break;
                    }
                }

                if (!isEnemyMatched) {
                    throw new Exception ("Export CollectableData failed. No matched enemy to fromEnemyId : " + fromEnemyId);
                }

                result.Add (new MapData.CollectableData (child.position.x, child.position.y, collectableType, fromEnemyId));
            }
        }

        Log.PrintWarning ("Export CollectableData success", LogType.MapData);
        return result;
    }

    private List<MapData.ExitData> ExportExitDataList () {
        Log.PrintWarning ("Start export ExitData", LogType.MapData);

        var result = new List<MapData.ExitData> ();
        var baseTransform = otherStuffsBaseTransform.Find (ExitsBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export ExitData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export ExitData failed. Invalid transform name : " + child.name);
            }

            var toEntryId = int.Parse (array[0]);

            var collider = child.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                throw new Exception ("Export ExitData failed. Cannot find BoxCollider2D : " + child.name);
            }

            result.Add (new MapData.ExitData (child.position.x, child.position.y, collider.size.x, collider.size.y, toEntryId));
        }

        Log.PrintWarning ("Export ExitData success", LogType.MapData);
        return result;
    }

    private List<MapData.TutorialData> ExportTutorialDataList () {
        Log.PrintWarning ("Start export TutorialData", LogType.MapData);

        var result = new List<MapData.TutorialData> ();
        var baseTransform = otherStuffsBaseTransform.Find (TutorialsBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export TutorialData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export TutorialData failed. Invalid transform name : " + child.name);
            }

            TutorialEnum.GameTutorialType tutorialType;
            if (!FrameworkUtils.TryParseToEnum (int.Parse (array[0]), out tutorialType)) {
                throw new Exception ("Export TutorialData failed. Invalid TutorialEnum.GameTutorialType : " + array[0]);
            }

            var collider = child.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                throw new Exception ("Export TutorialData failed. Cannot find BoxCollider2D : " + child.name);
            }

            result.Add (new MapData.TutorialData (child.position.x, child.position.y, collider.size.x, collider.size.y, tutorialType));
        }

        Log.PrintWarning ("Export TutorialData success", LogType.MapData);
        return result;
    }

    #endregion
}