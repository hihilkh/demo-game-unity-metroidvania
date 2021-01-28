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

    #region Map Generation

    public void GenerateMap (MapData mapData) {

        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogType.MapData);
            return;
        }

        GenerateTiles (mapData.tiles);
        var enemyModelList = GenerateEnemy (mapData.enemies);
        GenerateCollectables (mapData.collectables, enemyModelList);
        GenerateSwitches (mapData.switches);
        GenerateExits (mapData.exits);
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
        }

        Log.Print ("Finish GenerateEnemy", LogType.MapData);

        return enemyModelList;
    }

    #region Map Trigger
    // TODO : Think if it can be simplied by a generic method

    // TODO : Do not generate collected collectable
    private void GenerateCollectables (List<MapData.CollectableData> dataList, List<EnemyModelBase> enemyModelList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateCollectables : No collectable data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateCollectables", LogType.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapCollectable");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapCollectableObject> ();

            if (data.isFromEnemy) {
                var isInitialized = false;
                foreach (var enemy in enemyModelList) {
                    if (enemy.id == data.fromEnemyId) {
                        script.Init (data, enemy);
                        isInitialized = true;
                        break;
                    }
                }

                if (!isInitialized) {
                    Log.PrintError ("Cannot initialize collectable. Cannot find coresponding enemy with enemyId: " + data.fromEnemyId, LogType.MapData);
                }
            } else {
                script.Init (data);
            }

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

    // TODO : Do not generate finished tutorials
    private void GenerateTutorials (List<MapData.TutorialData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateTutorials : No tutorial data.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start GenerateTutorials", LogType.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapTutorial");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.AddComponent<MapTutorialTrigger> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateTutorials", LogType.MapData);
    }

    #endregion

    #endregion
}