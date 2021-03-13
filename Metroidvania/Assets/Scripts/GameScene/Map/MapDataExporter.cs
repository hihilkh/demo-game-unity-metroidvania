using System;
using System.Collections.Generic;
using System.IO;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MapDataExporter : MonoBehaviour {

    [Header ("Export Info")]
    [SerializeField] private string exportRootFolderPath;

    [Header ("Tilemap")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap deathTileMap;
    [SerializeField] private Tilemap bgTileMap;
    [SerializeField] private Tilemap onTopEffectTileMap;

    [Header ("Image Background")]
    [SerializeField] private Transform imageBGBase;

    [Header ("Mission")]
    [SerializeField] private int missionId;
    [SerializeField] private RectTransform exportRange;
    [SerializeField] private RectTransform missionBoundary;
    [SerializeField] private Transform mapObjectsBaseTransform;

    private Dictionary<MapEnum.TileMapType, Tilemap> TileMapDict => new Dictionary<MapEnum.TileMapType, Tilemap> {
        { MapEnum.TileMapType.Ground, groundTileMap},
        { MapEnum.TileMapType.SlippyWall, slippyWallTileMap},
        { MapEnum.TileMapType.Death, deathTileMap },
        { MapEnum.TileMapType.Background, bgTileMap },
        { MapEnum.TileMapType.OnTopEffect, onTopEffectTileMap },
    };

    private const string ExportFileNameFormat = "{0}.{1}";  // fileName.timestamp

    private const string EntriesBaseTransformName = "Entries";
    private const string EnemiesBaseTransformName = "Enemies";
    private const string CollectablesBaseTransformName = "Collectables";
    private const string SwitchesBaseTransformName = "Switches";
    private const string HiddenPathsBaseTransformName = "HiddenPaths";
    private const string ExitsBaseTransformName = "Exits";
    private const string MissionEventsBaseTransformName = "MissionEvents";
    private const string SpecialScenesBaseTransformName = "SpecialScenes";
    private const string IgnoreTilesBaseTransformName = "IgnoreTiles";
    private const string EndingExitsBaseTransformName = "EndingExits";

    private void OnGUI () {
        if (GUI.Button (new Rect (10, 10, 150, 50), "Check Map Design")) {
            CheckMapDesign ();
        }

        if (GUI.Button (new Rect (180, 10, 150, 50), "Export MapData")) {
            ExportMapData (false);
        }

        if (GUI.Button (new Rect (350, 10, 150, 50), "Export Ending Mission")) {
            ExportMapData (true);
        }
    }

    private MapDataTileExportIterator GetMapDataTileExportIterator (MapData.Boundary boundary) {
        return new MapDataTileExportIterator (TileMapDict, boundary.lowerBound, boundary.upperBound);
    }

    private void CheckMapDesign () {
        Log.PrintWarning ("Start Check Map Design", LogTypes.MapData);

        var exportRange = ExportBoundary (true);
        var iterator = GetMapDataTileExportIterator (exportRange);

        foreach (var tileData in iterator) {
            if (tileData != null) {
                var tileMapType = tileData.tileMapType;
                if (tileMapType == MapEnum.TileMapType.Background) {
                    if (TileMapping.CheckIsMapDesignTileType (tileData.tileType)) {
                        Log.PrintError ("Check Map Design finished : Not match. Pos : " + tileData.pos + ", background tile using map design tile type", LogTypes.MapData);
                        return;
                    }
                } else {
                    var mapDesignTileType = TileMapping.GetMapDesignTileType (tileMapType);
                    if (mapDesignTileType == null || mapDesignTileType != tileData.tileType) {
                        Log.PrintError ("Check Map Design finished : Not match. Pos : " + tileData.pos + ", tileType : " + tileData.tileType + " , added to TileMapType : " + tileData.tileMapType, LogTypes.MapData);
                        return;
                    }
                }
            }
        }

        Log.PrintWarning ("Check Map Design finished : All match!", LogTypes.MapData);
    }

    #region Export

    private bool CheckEnemyExist (List<MapData.EnemyData> enemies, int enemyId) {
        foreach (var enemy in enemies) {
            if (enemy.id == enemyId) {
                return true;
            }
        }

        return false;
    }

    private void ExportMapData (bool isEndingMission) {
        Log.PrintWarning ("Start export MapData", LogTypes.MapData);

        var mapData = new MapData ();

        mapData.boundary = ExportBoundary (false);
        mapData.tiles = ExportTileDataList (ExportBoundary (true), isEndingMission);
        mapData.entries = ExportEntryDataList ();
        mapData.enemies = ExportEnemyDataList ();
        if (!isEndingMission) {
            mapData.collectables = ExportCollectableDataList (mapData.enemies);
            mapData.switches = ExportSwitchDataList (ref mapData);  // Must be after exporting tiles and enemies
            mapData.events = ExportMissionEventDataList ();
        }
        
        mapData.exits = ExportExitDataList (isEndingMission);
        mapData.backgrounds = ExportImageBGDataList ();
        mapData.specialScenes = ExportSpecialSceneDataList ();

        var json = JsonUtility.ToJson (mapData);
        var exportMissionId = isEndingMission ? MissionManager.EndingMissionId : missionId;
        var fileName = FrameworkUtils.StringReplace (ExportFileNameFormat, AssetDetails.GetMapDataJSONFileName (exportMissionId), FrameworkUtils.ConvertDateTimeToTimestampMS (System.DateTime.Now).ToString ());
        var filePath = Path.Combine (exportRootFolderPath, fileName);

        var isSuccess = FrameworkUtils.CreateFile (filePath, false, json);
        if (isSuccess) {
            Log.PrintWarning ("Export MapData success. Path : " + filePath, LogTypes.MapData);
        } else {
            Log.PrintError ("Export MapData failed. Path : " + filePath, LogTypes.MapData);
        }
    }

    private MapData.Boundary ExportBoundary (bool isExportRange) {
        Log.PrintWarning ("Start export Boundary", LogTypes.MapData);

        var targetRange = isExportRange ? exportRange : missionBoundary;
        var lowerBound = new Vector2Int (Mathf.FloorToInt (targetRange.position.x), Mathf.FloorToInt (targetRange.position.y));
        var upperBound = new Vector2Int (Mathf.CeilToInt (targetRange.position.x + targetRange.sizeDelta.x), Mathf.CeilToInt (targetRange.position.y + targetRange.sizeDelta.y));
        var boundary = new MapData.Boundary (lowerBound, upperBound);

        Log.PrintWarning ("Export Boundary success", LogTypes.MapData);
        return boundary;
    }

    private List<MapData.TileData> ExportTileDataList (MapData.Boundary boundary, bool isEndingMission) {
        Log.PrintWarning ("Start export TileData", LogTypes.MapData);

        var ignoreTilePosList = new List<Vector2Int> ();
        if (isEndingMission) {
            ignoreTilePosList = ExportIgnoreTilePosList ();
        }

        var iterator = GetMapDataTileExportIterator (boundary);
        var tileDataList = new List<MapData.TileData> ();

        foreach (var tileData in iterator) {
            if (tileData != null) {
                if (!ignoreTilePosList.Contains (tileData.pos)) {
                    tileDataList.Add (tileData);
                }
            }
        }

        Log.PrintWarning ("Export TileData success", LogTypes.MapData);
        return tileDataList;
    }

    private List<Vector2Int> ExportIgnoreTilePosList () {
        Log.PrintWarning ("Start export IgnoreTilePosList", LogTypes.MapData);

        var result = new List<Vector2Int> ();
        var baseTransform = mapObjectsBaseTransform.Find (IgnoreTilesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export IgnoreTilePosList failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var spriteRenderer = child.GetComponent<SpriteRenderer> ();
            if (spriteRenderer == null) {
                throw new Exception ("Export IgnoreTilePosList failed. Cannot find spriteRenderer for : " + child.name);
            }

            // Remarks :
            // - Assume the setting is correct and the calculated result is always an integer.
            // - Must use local position in order to get the correct tile position instead of world position
            var hiddenPathMinPosX = (int)(child.localPosition.x - ((spriteRenderer.size.x - 1) / 2));
            var hiddenPathMinPosY = (int)(child.localPosition.y - ((spriteRenderer.size.y - 1) / 2));

            for (int x = 0; x < spriteRenderer.size.x; x++) {
                for (int y = 0; y < spriteRenderer.size.y; y++) {
                    result.Add (new Vector2Int (hiddenPathMinPosX + x, hiddenPathMinPosY + y));
                }
            }
        }

        return result;
    }

    private List<MapData.EntryData> ExportEntryDataList () {
        Log.PrintWarning ("Start export EntryData", LogTypes.MapData);

        var result = new List<MapData.EntryData> ();
        var baseTransform = mapObjectsBaseTransform.Find (EntriesBaseTransformName);
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
            if (!FrameworkUtils.TryParseToEnum (array[1], out direction)) {
                throw new Exception ("Export EntryData failed. Invalid HorizontalDirection : " + array[1]);
            }

            result.Add (new MapData.EntryData (child.position, direction, entryId));
        }

        Log.PrintWarning ("Export EntryData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.EnemyData> ExportEnemyDataList () {
        Log.PrintWarning ("Start export EnemyData", LogTypes.MapData);

        var result = new List<MapData.EnemyData> ();
        var baseTransform = mapObjectsBaseTransform.Find (EnemiesBaseTransformName);
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
            if (!FrameworkUtils.TryParseToEnum (array[1], out enemyType)) {
                throw new Exception ("Export EnemyData failed. Invalid EnemyType : " + array[1]);
            }
            LifeEnum.HorizontalDirection direction;
            if (!FrameworkUtils.TryParseToEnum (array[2], out direction)) {
                throw new Exception ("Export EnemyData failed. Invalid HorizontalDirection : " + array[2]);
            }

            result.Add (new MapData.EnemyData (child.position, direction, enemyId, enemyType));
        }

        Log.PrintWarning ("Export EnemyData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.CollectableData> ExportCollectableDataList (List<MapData.EnemyData> enemies) {
        Log.PrintWarning ("Start export CollectableData", LogTypes.MapData);

        var result = new List<MapData.CollectableData> ();
        var baseTransform = mapObjectsBaseTransform.Find (CollectablesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export CollectableData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 2) {
                throw new Exception ("Export CollectableData failed. Invalid transform name : " + child.name);
            }

            Collectable.Type collectableType;
            if (!FrameworkUtils.TryParseToEnum (array[0], out collectableType)) {
                throw new Exception ("Export CollectableData failed. Invalid MissionCollectable.Type : " + array[0]);
            }

            if (string.IsNullOrEmpty (array[1])) {
                result.Add (new MapData.CollectableData (child.position, collectableType));
            } else {
                var fromEnemyId = int.Parse (array[1]);

                if (!CheckEnemyExist (enemies, fromEnemyId)) {
                    throw new Exception ("Export CollectableData failed. No matched enemy to fromEnemyId : " + fromEnemyId);
                }

                result.Add (new MapData.CollectableData (child.position, collectableType, fromEnemyId));
            }
        }

        Log.PrintWarning ("Export CollectableData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.SwitchData> ExportSwitchDataList (ref MapData mapData) {
        Log.PrintWarning ("Start export SwitchData", LogTypes.MapData);

        var result = new List<MapData.SwitchData> ();
        var baseTransform = mapObjectsBaseTransform.Find (SwitchesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export SwitchData failed. Cannot find the base transform.");
        }

        var hiddenPathsBaseTransform = baseTransform.Find (HiddenPathsBaseTransformName);
        if (hiddenPathsBaseTransform == null) {
            throw new Exception ("Export SwitchData failed. Cannot find the hidden paths base transform.");
        }

        var switchTransformList = new List<Transform> ();
        foreach (Transform child in baseTransform) {
            if (child != hiddenPathsBaseTransform) {
                switchTransformList.Add (child);
            }
        }

        foreach (var switchTransform in switchTransformList) {
            var array = switchTransform.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 3) {
                throw new Exception ("Export SwitchData failed. Invalid transform name : " + switchTransform.name);
            }

            var switchId = int.Parse (array[0]);
            MapEnum.SwitchType switchType;
            if (!FrameworkUtils.TryParseToEnum (array[1], out switchType)) {
                throw new Exception ("Export SwitchData failed. Invalid SwitchType : " + array[1]);
            }

            var collider = switchTransform.GetComponent<BoxCollider2D> ();
            if (collider == null && switchType != MapEnum.SwitchType.Enemy && switchType != MapEnum.SwitchType.MissionEvent) {
                throw new Exception ("Export SwitchData failed. Cannot find BoxCollider2D for switchId : " + switchId);
            }

            MapData.SwitchData switchData;
            switch (switchType) {
                case MapEnum.SwitchType.Enemy:
                    var fromEnemyId = int.Parse (array[2]);

                    if (!CheckEnemyExist (mapData.enemies, fromEnemyId)) {
                        throw new Exception ("Export SwitchData failed. No matched enemy to fromEnemyId : " + fromEnemyId);
                    }

                    switchData = new MapData.SwitchData (switchId, fromEnemyId);
                    break;
                case MapEnum.SwitchType.MissionEvent:
                    switchData = new MapData.SwitchData (switchId);
                    break;
                default:
                    var colliderPosX = switchTransform.position.x + collider.offset.x;
                    var colliderPosY = switchTransform.position.y + collider.offset.y;

                    // Remarks : Must use local position in order to get the correct tile position instead of world position
                    var switchBasePos = new Vector2Int ((int)switchTransform.localPosition.x, (int)switchTransform.localPosition.y);
                    switchData = new MapData.SwitchData (switchId, new Vector2 (colliderPosX, colliderPosY), collider.size, switchType, switchBasePos);
                    break;
            }

            foreach (Transform trans in hiddenPathsBaseTransform) {
                var hiddenPathNameArray = trans.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
                if (hiddenPathNameArray.Length < 4) {
                    throw new Exception ("Export SwitchData failed. Invalid transform name : " + switchTransform.name);
                }

                if (hiddenPathNameArray[0] != switchId.ToString ()) {
                    continue;
                }

                MapEnum.HiddenPathType hiddenPathType;
                if (!FrameworkUtils.TryParseToEnum (hiddenPathNameArray[1], out hiddenPathType)) {
                    throw new Exception ("Export SwitchData failed. Invalid HiddenPathType : " + hiddenPathNameArray[1]);
                }

                MapEnum.HiddenPathOrdering hiddenPathOrdering;
                if (!FrameworkUtils.TryParseToEnum (hiddenPathNameArray[2], out hiddenPathOrdering)) {
                    throw new Exception ("Export SwitchData failed. Invalid HiddenPathOrdering : " + hiddenPathNameArray[2]);
                }

                var spriteRenderer = trans.GetComponent<SpriteRenderer> ();
                if (spriteRenderer == null) {
                    throw new Exception ("Export SwitchData failed. Cannot find spriteRenderer for : " + trans.name);
                }

                // Remarks :
                // - Assume the setting is correct and the calculated result is always an integer.
                // - Must use local position in order to get the correct tile position instead of world position
                var hiddenPathMinPosX = (int)(trans.localPosition.x - ((spriteRenderer.size.x - 1) / 2));
                var hiddenPathMinPosY = (int)(trans.localPosition.y - ((spriteRenderer.size.y - 1) / 2));

                var hiddenPathTiles = new List<MapData.TileData> ();
                for (int x = 0; x < spriteRenderer.size.x; x++) {
                    for (int y = 0; y < spriteRenderer.size.y; y++) {
                        var pos = new Vector2Int (hiddenPathMinPosX + x, hiddenPathMinPosY + y);

                        MapData.TileData tileData;
                        if (string.IsNullOrEmpty (hiddenPathNameArray[3])) {
                            tileData = mapData.GetTileData (pos);
                        } else {
                            MapEnum.TileMapType tileMapType;
                            if (!FrameworkUtils.TryParseToEnum (hiddenPathNameArray[3], out tileMapType)) {
                                throw new Exception ("Export SwitchData failed. Invalid target tileMapType for HiddenPath : " + hiddenPathNameArray[3]);
                            }

                            tileData = mapData.GetTileData (pos, tileMapType);
                        }


                        if (tileData != null) {
                            hiddenPathTiles.Add (tileData);

                            if (hiddenPathType == MapEnum.HiddenPathType.ShowWhenSwitchOn) {
                                var isRemoveSuccess = mapData.RemoveTileData (tileData);

                                if (!isRemoveSuccess) {
                                    throw new Exception ("Export SwitchData failed. Cannot remove tile which should be shown after switched on. Pos : " + tileData.pos);
                                }
                            }
                        }
                    }
                }

                if (hiddenPathTiles.Count <= 0) {
                    throw new Exception ("Export SwitchData failed. hiddenPathTiles count <= 0 for : " + trans.name);
                }

                switchData.AddHiddenPath (hiddenPathType, hiddenPathOrdering, hiddenPathTiles);
            }

            result.Add (switchData);
        }


        Log.PrintWarning ("Export SwitchData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.ExitData> ExportExitDataList (bool isEndingMission) {
        Log.PrintWarning ("Start export ExitData", LogTypes.MapData);

        var result = new List<MapData.ExitData> ();
        var baseTransform = mapObjectsBaseTransform.Find (isEndingMission ? EndingExitsBaseTransformName : ExitsBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export ExitData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export ExitData failed. Invalid transform name : " + child.name);
            }

            var collider = child.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                throw new Exception ("Export ExitData failed. Cannot find BoxCollider2D : " + child.name);
            }

            if (isEndingMission) {
                MissionEventEnum.SpecialSceneType specialSceneExitType;
                if (!FrameworkUtils.TryParseToEnum (array[0], out specialSceneExitType)) {
                    throw new Exception ("Export ExitData failed. Invalid MissionEventEnum.SpecialSceneType : " + array[0]);
                }
                result.Add (new MapData.ExitData (child.position, collider.size, specialSceneExitType));
            } else {
                var toEntryId = int.Parse (array[0]);
                result.Add (new MapData.ExitData (child.position, collider.size, toEntryId));
            }
        }

        Log.PrintWarning ("Export ExitData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.MissionEventData> ExportMissionEventDataList () {
        Log.PrintWarning ("Start export MissionEventData", LogTypes.MapData);

        var result = new List<MapData.MissionEventData> ();
        var baseTransform = mapObjectsBaseTransform.Find (MissionEventsBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export MissionEventData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export MissionEventData failed. Invalid transform name : " + child.name);
            }

            MissionEventEnum.EventType eventType;
            if (!FrameworkUtils.TryParseToEnum (array[0], out eventType)) {
                throw new Exception ("Export MissionEventData failed. Invalid MissionEventEnum.EventType : " + array[0]);
            }

            var collider = child.GetComponent<BoxCollider2D> ();
            if (collider == null) {
                throw new Exception ("Export MissionEventData failed. Cannot find BoxCollider2D : " + child.name);
            }

            result.Add (new MapData.MissionEventData (child.position, collider.size, eventType));
        }

        Log.PrintWarning ("Export MissionEventData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.ImageBGData> ExportImageBGDataList () {
        Log.PrintWarning ("Start export ImageBGData", LogTypes.MapData);

        var result = new List<MapData.ImageBGData> ();
        if (imageBGBase == null) {
            throw new Exception ("Export ImageBGData failed. Do not have base transform.");
        }

        foreach (Transform child in imageBGBase) {
            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export ImageBGData failed. Invalid transform name : " + child.name);
            }

            MapEnum.ImageBGType imageBGType;
            if (!FrameworkUtils.TryParseToEnum (array[0], out imageBGType)) {
                throw new Exception ("Export ImageBGData failed. Invalid imageBGType : " + array[0]);
            }

            var rectTransform = child.GetComponent<RectTransform> ();
            if (rectTransform == null) {
                throw new Exception ("Export ImageBGData failed. Cannot find RectTransform : " + child.name);
            }

            var image = child.GetComponent<Image> ();
            if (image == null) {
                throw new Exception ("Export ImageBGData failed. Cannot find Image : " + child.name);
            }

            var pos = child.position;
            var size = rectTransform.sizeDelta;
            var color = image.color;

            result.Add (new MapData.ImageBGData (imageBGType, pos, size, color));
        }

        Log.PrintWarning ("Export ImageBGData success", LogTypes.MapData);
        return result;
    }

    private List<MapData.SpecialSceneData> ExportSpecialSceneDataList () {
        Log.PrintWarning ("Start export SpecialSceneData", LogTypes.MapData);

        var result = new List<MapData.SpecialSceneData> ();
        var baseTransform = mapObjectsBaseTransform.Find (SpecialScenesBaseTransformName);
        if (baseTransform == null) {
            throw new Exception ("Export SpecialSceneData failed. Cannot find the base transform.");
        }

        foreach (Transform child in baseTransform) {
            if (child.childCount <= 0) {
                throw new Exception ("Export SpecialSceneData failed. No sub special scene transform for special scene transform : " + child.name);
            }

            var array = child.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
            if (array.Length < 1) {
                throw new Exception ("Export SpecialSceneData failed. Invalid transform name : " + child.name);
            }

            MissionEventEnum.SpecialSceneType specialSceneType;
            if (!FrameworkUtils.TryParseToEnum (array[0], out specialSceneType)) {
                throw new Exception ("Export SpecialSceneData failed. Invalid MissionEventEnum.SpecialSceneType : " + array[0]);
            }

            var data = new MapData.SpecialSceneData (specialSceneType);

            foreach (Transform subSpecialScene in child) {
                var subSpecialSceneArray = subSpecialScene.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
                if (subSpecialSceneArray.Length < 1) {
                    throw new Exception ("Export SpecialSceneData failed. Invalid subSpecialScene name : " + subSpecialScene.name);
                }
                var isNeedCaveCollapseEffect = subSpecialSceneArray[0] == "1" ? true : false;

                var cameraPos = subSpecialScene.position;
                Vector2? playerPos = null;
                LifeEnum.HorizontalDirection? playerDirection = null;
                Vector2? bossPos = null;
                LifeEnum.HorizontalDirection? bossDirection = null;

                foreach (Transform details in subSpecialScene) {
                    var detailsNameArray = details.name.Split (new string[] { FrameworkVariable.DefaultDelimiter }, StringSplitOptions.None);
                    if (detailsNameArray.Length < 2) {
                        throw new Exception ("Export SpecialSceneData failed. Invalid sub special scene details name : " + details.name);
                    }

                    LifeEnum.HorizontalDirection direction;
                    if (!FrameworkUtils.TryParseToEnum (detailsNameArray[1], out direction)) {
                        throw new Exception ("Export SpecialSceneData failed. Invalid LifeEnum.HorizontalDirection : " + detailsNameArray[1]);
                    }

                    if (detailsNameArray[0] == "0") {   // Player
                        playerPos = details.position;
                        playerDirection = direction;
                    } else {                            // Boss
                        bossPos = details.position;
                        bossDirection = direction;
                    }
                }

                data.AddSubSpecialSceneData (cameraPos, isNeedCaveCollapseEffect, playerPos, playerDirection, bossPos, bossDirection);
            }

            result.Add (data);
        }

        Log.PrintWarning ("Export SpecialSceneData success", LogTypes.MapData);
        return result;
    }
    #endregion
}