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

    [SerializeField] private GameObject collectableTemplate;

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

    /// <summary>
    /// bool : isAdded<br />
    ///  - true : initially not in map and added during game<br />
    /// - false : initially in map and removed during game
    /// </summary>
    private Dictionary<MapData.TileData, bool> changedTileDict = new Dictionary<MapData.TileData, bool> ();
    private List<Vector2Int> switchedOnOnOffSwitchBasePosList = new List<Vector2Int> ();

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

        changedTileDict.Clear ();
        switchedOnOnOffSwitchBasePosList.Clear ();
        InitializeTiles (mapData.tiles);
        GenerateExits (mapData.exits);
        GenerateMapDisposableObjects ();

        AddEventListeners ();
    }

    public void ResetMap () {
        MapResetingEvent?.Invoke ();

        ResumeAllChangedTiles ();
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

    private void InitializeTiles (List<MapData.TileData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.PrintError ("No tile data. Please check.", LogType.MapData);
            return;
        } else {
            Log.Print ("Start InitializeTiles", LogType.MapData);
        }

        foreach (var data in dataList) {
            GenerateTile (data, true, false);
        }

        Log.Print ("Finish InitializeTiles", LogType.MapData);
    }

    /// <param name="isChangedInGame">If true, the tile is changed during game and would do some logic with changedTileDict</param>
    private void GenerateTile (MapData.TileData data, bool isShow, bool isChangedInGame) {
        var tileMapType = data.tileMapType;
        if (!tileMapDict.ContainsKey (tileMapType)) {
            Log.PrintError ("generate tile failed : tileMapDict do not have mapping for tileMapType : " + tileMapType, LogType.MapData);
            return;
        }

        var targetTilemap = tileMapDict[tileMapType];

        if (!isShow) {
            targetTilemap.SetTile (new Vector3Int (data.pos.x, data.pos.y, GameVariable.TilePosZ), null);
        } else {
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


        if (isChangedInGame) {
            if (changedTileDict.ContainsKey (data)) {
                if (changedTileDict[data] != isShow) {
                    changedTileDict.Remove (data);
                }
            } else {
                changedTileDict.Add (data, isShow);
            }
        }

    }

    private void ResumeAllChangedTiles () {
        foreach (var pair in changedTileDict) {
            GenerateTile (pair.Key, !pair.Value, false);
        }
        changedTileDict.Clear ();

        foreach (var switchBasePos in switchedOnOnOffSwitchBasePosList) {
            SetOnOffSwitch (switchBasePos, false, false);
        }
        switchedOnOnOffSwitchBasePosList.Clear ();
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
            instance.Reset (data);
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

            var go = Instantiate (collectableTemplate);
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go);
            var script = go.GetComponentInChildren<MapCollectableObject> ();
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

    #region Map interaction

    private void AddEventListeners () {
        if (!isAddedEventListeners) {
            isAddedEventListeners = true;

            EnemyModelBase.DiedEvent += EnemyDied;
            MapSwitch.SwitchedEvent += MapSwitchSwitched;
        }
    }

    private void RemoveEventListeners () {
        if (isAddedEventListeners) {
            EnemyModelBase.DiedEvent -= EnemyDied;
            MapSwitch.SwitchedEvent -= MapSwitchSwitched;

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

    private void MapSwitchSwitched (MapSwitch mapSwitch, bool isSwitchOn) {
        Log.Print ("Switch switched on : Pos : " + mapSwitch.GetTargetPos (), LogType.GameFlow | LogType.MapData);

        switch (mapSwitch.GetSwitchType ()) {
            case MapEnum.SwitchType.Arrow:
                foreach (var target in arrowTargetList) {
                    if (target is MapSwitch) {
                        if ((MapSwitch)target == mapSwitch) {
                            arrowTargetList.Remove (target);
                            break;
                        }
                    }
                }
                break;
            case MapEnum.SwitchType.OnOff:
                SetOnOffSwitch (mapSwitch.GetSwitchBasePos (), isSwitchOn, true);
                break;

        }

        StartCoroutine (OpenHiddenPath (mapSwitch, isSwitchOn));
    }

    /// <param name="isChangedInGame">If true, the tile is changed during game and would do some logic with switchedOnOnOffSwitchBasePosList</param>
    private void SetOnOffSwitch (Vector2Int switchBasePos, bool isOn, bool isChangedInGame) {
        var dict = isOn ? TileMapping.OnOffSwitchOnTileTypeDict : TileMapping.OnOffSwitchOffTileTypeDict;

        foreach (var pair in dict) {
            var tileType = default (MapEnum.TileType);
            if (pair.Value != null) {
                tileType = (MapEnum.TileType)pair.Value;
            }

            // Assume the switch is in background Tilemap
            var tileData = new MapData.TileData (switchBasePos.x + pair.Key.x, switchBasePos.y + pair.Key.y, tileType, MapEnum.TileMapType.Background);
            GenerateTile (tileData, pair.Value != null, false);
        }

        if (isChangedInGame) {
            if (switchedOnOnOffSwitchBasePosList.Contains (switchBasePos)) {
                // TODO : Check if Vector2Int can check equal by this way
                Log.PrintError ("switchedOnOnOffSwitchBasePosList contain");
                if (!isOn) {
                    switchedOnOnOffSwitchBasePosList.Remove (switchBasePos);
                }
            } else {
                if (isOn) {
                    switchedOnOnOffSwitchBasePosList.Add (switchBasePos);
                }
            }
        }
    }
    
    private IEnumerator OpenHiddenPath (MapSwitch mapSwitch, bool isSwitchOn) {
        if (mapSwitch == null) {
            yield break;
        }

        var pathDataList = mapSwitch.GetHiddenPathDataList ();
        if (pathDataList == null || pathDataList.Count <= 0) {
            mapSwitch.FinishSwitched ();
            yield break;
        }

        var tilesDataToHideByOrdering = new List<List<MapData.TileData>> ();
        var tilesDataToShowByOrdering = new List<List<MapData.TileData>> ();
        foreach (var pathData in pathDataList) {
            var tiles = pathData.GetTilesPosByOrdering (isSwitchOn);
            var targetList = tilesDataToHideByOrdering;

            if (!isSwitchOn && pathData.type == MapEnum.HiddenPathType.HideWhenSwitchOn) {
                targetList = tilesDataToShowByOrdering;
            } else if (isSwitchOn && pathData.type == MapEnum.HiddenPathType.ShowWhenSwitchOn) {
                targetList = tilesDataToShowByOrdering;
            }

            while (targetList.Count < tiles.Count) {
                targetList.Add (new List<MapData.TileData> ());
            }

            for (var i = 0; i < tiles.Count; i++) {
                targetList[i].AddRange (tiles[i]);
            }
        }

        var maxCount = Mathf.Max (tilesDataToHideByOrdering.Count, tilesDataToShowByOrdering.Count);

        for (var i = 0; i < maxCount; i++) {
            if (i < tilesDataToHideByOrdering.Count) {
                UpdateOneHiddenPathLayer (tilesDataToHideByOrdering[i], false);
            }

            if (i < tilesDataToShowByOrdering.Count) {
                UpdateOneHiddenPathLayer (tilesDataToShowByOrdering[i], true);
            }

            yield return new WaitForSeconds (OpenOneHiddenPathLayerPeriod);
        }

        mapSwitch.FinishSwitched ();
    }

    private void UpdateOneHiddenPathLayer (List<MapData.TileData> tileDataList, bool isShow) {
        if (tileDataList == null || tileDataList.Count <= 0) {
            return;
        }

        foreach (var tileData in tileDataList) {
            GenerateTile (tileData, isShow, true);
        }
    }

    #endregion
}