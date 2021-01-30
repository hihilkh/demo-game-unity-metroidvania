using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour {
    [Header ("Tilemap")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap ground2TileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap deathTileMap;
    [SerializeField] private Tilemap bgTileMap;

    [SerializeField] private Transform mapObjectsBaseTransform;

    private Dictionary<MapEnum.TileMapType, Tilemap> tileMapDict => new Dictionary<MapEnum.TileMapType, Tilemap> {
        { MapEnum.TileMapType.Ground, groundTileMap},
        { MapEnum.TileMapType.Ground2, ground2TileMap},
        { MapEnum.TileMapType.SlippyWall, slippyWallTileMap},
        { MapEnum.TileMapType.Death, deathTileMap },
        { MapEnum.TileMapType.Background, bgTileMap },
    };

    public static Action MapResetingEvent;

    private int missionId;
    private MapData mapData;
    public List<IMapTarget> arrowTargetList { get; private set; } = new List<IMapTarget> ();
    private List<Vector2Int> removedTilePosList = new List<Vector2Int> ();

    private bool isAddedEventListeners = false;
    private const float OpenOneHiddenPathLayerPeriod = 0.1f;

    private void OnDestroy () {
        RemoveEventListeners ();
    }

    #region Map Generation

    public void GenerateMap (int missionId, MapData mapData) {
        this.missionId = missionId;
        this.mapData = mapData;

        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogType.MapData);
            return;
        }

        removedTilePosList.Clear ();
        GenerateTiles (mapData.tiles);
        GenerateExits (mapData.exits);
        GenerateMapDisposableObjects ();

        AddEventListeners ();
    }

    public void ResetMap () {
        MapResetingEvent?.Invoke ();

        RegenerateRemovedTiles ();
        GenerateMapDisposableObjects ();
    }

    private void GenerateMapDisposableObjects () {
        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogType.MapData);
            return;
        }

        arrowTargetList.Clear ();

        GenerateEnemy (mapData.enemies);
        GenerateCollectables (missionId, mapData.collectables);
        GenerateSwitches (mapData.switches);
        GenerateTutorials (mapData.tutorials);
    }

    private void GenerateTiles (List<MapData.TileData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.PrintError ("No tile data. Please check.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateTiles", LogType.MapData);
        }

        foreach (var data in dataList) {
            GenerateTile (data);
        }

        Log.Print ("Finish GenerateTiles", LogType.MapData);
    }

    private void GenerateTile (MapData.TileData data) {
        var tileMapType = data.tileMapType;
        if (!tileMapDict.ContainsKey (tileMapType)) {
            Log.PrintError ("generate tile failed : tileMapDict do not have mapping for tileMapType : " + tileMapType, LogType.MapData);
            return;
        }

        var targetTilemap = tileMapDict[tileMapType];
        var tileType = data.tileType;
        var resourcesName = TileMapping.GetTileResourcesName (tileType);
        if (string.IsNullOrEmpty (resourcesName)) {
            Log.PrintError ("generate tile failed : resourcesName is empty for tileType : " + tileType, LogType.MapData);
            return;
        }

        var tile = Resources.Load<Tile> (resourcesName);
        if (tile == null) {
            Log.PrintError ("generate tile failed : Cannot load tile resources for resourcesName : " + resourcesName, LogType.MapData);
            return;
        }

        targetTilemap.SetTile (new Vector3Int (data.pos.x, data.pos.y, GameVariable.TilePosZ), tile);
    }

    private void RegenerateRemovedTiles () {
        foreach (var pos in removedTilePosList) {
            var tileData = mapData.GetTileData (pos);
            if (tileData != null) {
                GenerateTile (tileData);
            }
        }

        removedTilePosList.Clear ();
    }

    private void GenerateExits (List<MapData.ExitData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.PrintError ("No exit data. Please check.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateExits", LogType.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapExit");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapExit> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateExits", LogType.MapData);
    }

    private void GenerateEnemy (List<MapData.EnemyData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateEnemy : No enemy data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateEnemy", LogType.MapData);
        }

        foreach (var data in dataList) {
            var enemyType = data.type;
            var resourcesName = EnemyMapping.GetEnemyResourcesName (enemyType);
            if (string.IsNullOrEmpty (resourcesName)) {
                Log.PrintError ("Skipped enemy : resourcesName is empty for enemyType : " + enemyType, LogType.MapData);
                continue;
            }

            var enemy = Resources.Load<EnemyModelBase> (resourcesName);
            if (enemy == null) {
                Log.PrintError ("Skipped enemy : Cannot load enemy resources for resourcesName : " + resourcesName, LogType.MapData);
                continue;
            }

            var instance = Instantiate (enemy, mapObjectsBaseTransform);
            instance.Init (data);
            arrowTargetList.Add (instance);
        }

        Log.Print ("Finish GenerateEnemy", LogType.MapData);

        return;
    }

    #region Map Trigger
    // TODO : Think if it can be simplied by a generic method

    private void GenerateCollectables (int missionId, List<MapData.CollectableData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateCollectables : No collectable data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateCollectables", LogType.MapData);
        }

        var collectedCollectableList = UserManager.GetMissionProgress (missionId).collectedCollectables;
        foreach (var data in dataList) {
            if (collectedCollectableList.Contains (data.type)) {
                // Already collected
                continue;
            }

            var go = new GameObject ("MapCollectable");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapCollectableObject> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateCollectables", LogType.MapData);
    }

    private void GenerateSwitches (List<MapData.SwitchData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateSwitches : No switch data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateSwitches", LogType.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapSwitch");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapSwitch> ();
            script.Init (data);

            if (data.switchType == MapEnum.SwitchType.Arrow) {
                arrowTargetList.Add (script);
            }
        }

        Log.Print ("Finish GenerateSwitches", LogType.MapData);
    }

    private void GenerateTutorials (List<MapData.TutorialData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateTutorials : No tutorial data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateTutorials", LogType.MapData);
        }

        foreach (var data in dataList) {
            if (TutorialManager.GetHasDoneGameTutorial (data.type)) {
                // Tutorial already done
                continue;
            }
            var go = new GameObject ("MapTutorial");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapTutorialTrigger> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateTutorials", LogType.MapData);
    }

    #endregion

    #endregion

    #region Getter

    private (Tilemap, TileBase) GetTile (Vector3Int pos) {
        foreach (var pair in tileMapDict) {
            var tile = pair.Value.GetTile (pos);
            if (tile != null) {
                return (pair.Value, tile);
            }
        }

        return (null, null);
    }

    #endregion

    #region Map interaction

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            EnemyModelBase.DiedEvent += EnemyDied;
            MapSwitch.SwitchedOnEvent += MapSwitchSwitchedOn;
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            EnemyModelBase.DiedEvent -= EnemyDied;
            MapSwitch.SwitchedOnEvent -= MapSwitchSwitchedOn;

            isAddedEventListeners = false;
        }
    }

    private void EnemyDied (int enemyId) {
        foreach (var target in arrowTargetList) {
            if (target is EnemyModelBase) {
                if (((EnemyModelBase)target).id == enemyId) {
                    arrowTargetList.Remove (target);
                    return;
                }
            }
        }
    }

    private void MapSwitchSwitchedOn (MapSwitch mapSwitch) {
        foreach (var target in arrowTargetList) {
            if (target is MapSwitch) {
                if ((MapSwitch)target == mapSwitch) {
                    arrowTargetList.Remove (target);
                    break;
                }
            }
        }

        StartCoroutine (OpenHiddenPath (mapSwitch.GetHiddenPathData ()));
    }

    private IEnumerator OpenHiddenPath (MapData.HiddenPathData pathData) {
        var tiles = pathData.GetTilesPosByOpenOrder ();

        if (tiles == null || tiles.Count <= 0) {
            yield break;
        }

        for (var i = 0; i < tiles.Count; i++) {
            OpenOneHiddenPathLayer (tiles[i]);
            yield return new WaitForSeconds (OpenOneHiddenPathLayerPeriod);
        }
    }

    private void OpenOneHiddenPathLayer (List<Vector3Int> tilePosList) {
        if (tilePosList == null || tilePosList.Count <= 0) {
            return;
        }

        foreach (var pos in tilePosList) {
            (var tilemap, var tileBase) = GetTile (pos);

            if (tilemap == null) {
                Log.PrintWarning ("No coresponding tilemap of hidden path tile is found. Pos : " + pos, LogType.MapData);
            } else {
                removedTilePosList.Add (new Vector2Int (pos.x, pos.y));
                tilemap.SetTile (pos, null);
            }
        }
    }

    #endregion
}