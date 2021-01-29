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

    public List<IMapTarget> arrowTargetList { get; private set; } = new List<IMapTarget> ();

    private bool isAddedEventListeners = false;

    private void OnDestroy () {
        RemoveEventListeners ();
    }

    #region Map Generation

    public void GenerateMap (int missionId, MapData mapData) {

        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogType.MapData);
            return;
        }

        arrowTargetList.Clear ();

        GenerateTiles (mapData.tiles);
        var enemyModelList = GenerateEnemy (mapData.enemies);
        GenerateCollectables (missionId, mapData.collectables, enemyModelList);
        GenerateSwitches (mapData.switches);
        GenerateExits (mapData.exits);
        GenerateTutorials (mapData.tutorials);

        AddEventListeners ();
    }

    private void GenerateTiles (List<MapData.TileData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.PrintError ("No tile data. Please check.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateTiles", LogType.MapData);
        }

        foreach (var data in dataList) {
            var tileMapType = data.GetTileMapType ();
            if (!tileMapDict.ContainsKey (tileMapType)) {
                Log.PrintError ("Skipped tile : tileMapDict do not have mapping for tileMapType : " + tileMapType, LogType.MapData);
                continue;
            }

            var targetTilemap = tileMapDict[tileMapType];
            var tileType = data.GetTileType ();
            var resourcesName = TileMapping.GetTileResourcesName (tileType);
            if (string.IsNullOrEmpty (resourcesName)) {
                Log.PrintError ("Skipped tile : resourcesName is empty for tileType : " + tileType, LogType.MapData);
                continue;
            }

            var tile = Resources.Load<Tile> (resourcesName);
            if (tile == null) {
                Log.PrintError ("Skipped tile : Cannot load tile resources for resourcesName : " + resourcesName, LogType.MapData);
                continue;
            }
            targetTilemap.SetTile (data.GetPos (), tile);
        }

        Log.Print ("Finish GenerateTiles", LogType.MapData);
    }

    private List<EnemyModelBase> GenerateEnemy (List<MapData.EnemyData> dataList) {
        var enemyModelList = new List<EnemyModelBase> ();
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateEnemy : No enemy data.", LogType.MapData);
            return enemyModelList;
        } else {
            Log.Print ("Start GenerateEnemy", LogType.MapData);
        }

        foreach (var data in dataList) {
            var enemyType = data.GetEnemyType ();
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

            enemyModelList.Add (instance);
            arrowTargetList.Add (instance);
        }

        Log.Print ("Finish GenerateEnemy", LogType.MapData);

        return enemyModelList;
    }

    #region Map Trigger
    // TODO : Think if it can be simplied by a generic method

    private void GenerateCollectables (int missionId, List<MapData.CollectableData> dataList, List<EnemyModelBase> enemyModelList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateCollectables : No collectable data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateCollectables", LogType.MapData);
        }

        var collectedCollectableList = GameProgress.GetMissionProgress (missionId).collectedCollectables;
        foreach (var data in dataList) {
            if (collectedCollectableList.Contains (data.GetCollectableType ())) {
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

            if (data.GetSwitchType() == MapEnum.SwitchType.Arrow) {
                arrowTargetList.Add (script);
            }
        }

        Log.Print ("Finish GenerateSwitches", LogType.MapData);
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

    private void GenerateTutorials (List<MapData.TutorialData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateTutorials : No tutorial data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateTutorials", LogType.MapData);
        }

        foreach (var data in dataList) {
            if (TutorialManager.GetHasDoneGameTutorial (data.GetTutorialType ())) {
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
                    return;
                }
            }
        }
    }

    #endregion
}