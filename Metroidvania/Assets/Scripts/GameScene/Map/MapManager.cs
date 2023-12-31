﻿using System;
using System.Collections;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class MapManager : MonoBehaviour {
    [Header ("Tilemap")]
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap slippyWallTileMap;
    [SerializeField] private Tilemap deathTileMap;
    [SerializeField] private Tilemap bgTileMap;
    [SerializeField] private Tilemap onTopEffectTileMap;

    [Header ("Image Background")]
    [SerializeField] private Transform imageBGBase;
    [SerializeField] private Sprite rightToLeftImageBGSprite;
    [SerializeField] private Sprite downToUpImageBGSprite;

    [Space(10)]
    [SerializeField] private Transform mapObjectsBaseTransform;

    [SerializeField] private GameObject collectableTemplate;

    private readonly Dictionary<MapEnum.TileMapType, Tilemap> tileMapDict = new Dictionary<MapEnum.TileMapType, Tilemap> ();
    private readonly Dictionary<MapEnum.ImageBGType, Sprite> imageBGSpriteDict = new Dictionary<MapEnum.ImageBGType, Sprite> ();

    public static Action MapReseting;

    private int missionId;
    private MapData mapData;
    public List<IMapTarget> ArrowTargetList { get; } = new List<IMapTarget> ();

    /// <summary>
    /// bool : isAdded<br />
    ///  - true : initially not in map and added during game<br />
    /// - false : initially in map and removed during game
    /// </summary>
    private readonly Dictionary<MapData.TileData, bool> changedTileDict = new Dictionary<MapData.TileData, bool> ();
    private readonly List<Vector2Int> switchedOnOnOffSwitchBasePosList = new List<Vector2Int> ();
    private readonly List<MapSwitch> mapSwitchList = new List<MapSwitch> ();

    public CharModel BossModel { get; private set; } = null;

    private bool isAddedEventHandlers = false;
    private const float OpenOneHiddenPathLayerPeriod = 0.1f;

    private void Awake () {
        tileMapDict.Add (MapEnum.TileMapType.Ground, groundTileMap);
        tileMapDict.Add (MapEnum.TileMapType.SlippyWall, slippyWallTileMap);
        tileMapDict.Add (MapEnum.TileMapType.Death, deathTileMap);
        tileMapDict.Add (MapEnum.TileMapType.Background, bgTileMap);
        tileMapDict.Add (MapEnum.TileMapType.OnTopEffect, onTopEffectTileMap);

        imageBGSpriteDict.Add (MapEnum.ImageBGType.RightToLeft, rightToLeftImageBGSprite);
        imageBGSpriteDict.Add (MapEnum.ImageBGType.DownToUp, downToUpImageBGSprite);
    }

    private void OnDestroy () {
        RemoveEventHandlers ();
    }

    #region getter

    public MapData.SubSpecialSceneData GetSubSpecialSceneData (MissionEventEnum.SpecialSceneType specialSceneType, int subSpecialSceneIndex) {
        return mapData.GetSubSpecialSceneData (specialSceneType, subSpecialSceneIndex);
    }

    #endregion

    #region Map Generation

    public void GenerateMap (int missionId, MapData mapData) {
        this.missionId = missionId;
        this.mapData = mapData;

        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogTypes.MapData);
            return;
        }

        changedTileDict.Clear ();
        switchedOnOnOffSwitchBasePosList.Clear ();
        BossModel = null;
        InitializeTiles (mapData.tiles);
        GenerateExits (mapData.exits);
        GenerateMapDisposableObjects ();
        GenerateImageBGs (mapData.backgrounds);

        AddEventHandlers ();
    }

    public void ResetMap () {
        MapReseting?.Invoke ();

        ResumeAllChangedTiles ();
        GenerateMapDisposableObjects ();
    }

    private void GenerateMapDisposableObjects () {
        if (mapData == null) {
            Log.PrintError ("mapData is null. Please check.", LogTypes.MapData);
            return;
        }

        ArrowTargetList.Clear ();
        mapSwitchList.Clear ();

        GenerateEnemy (mapData.enemies);
        GenerateCollectables (missionId, mapData.collectables);
        GenerateSwitches (mapData.switches);
        GenerateMissionEvents (mapData.events);
    }

    private void InitializeTiles (List<MapData.TileData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.PrintError ("No tile data. Please check.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start InitializeTiles", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            GenerateTile (data, true, false);
        }

        Log.Print ("Finish InitializeTiles", LogTypes.MapData);
    }

    /// <param name="isChangedInGame">If true, the tile is changed during game and would do some logic with changedTileDict</param>
    private void GenerateTile (MapData.TileData data, bool isShow, bool isChangedInGame) {
        var tileMapType = data.tileMapType;
        if (!tileMapDict.ContainsKey (tileMapType)) {
            Log.PrintError ("Generate tile failed : tileMapDict do not have mapping for tileMapType : " + tileMapType, LogTypes.MapData);
            return;
        }

        var targetTilemap = tileMapDict[tileMapType];

        if (!isShow) {
            targetTilemap.SetTile (new Vector3Int (data.pos.x, data.pos.y, GameVariable.TilePosZ), null);
        } else {
            var tileType = data.tileType;
            var resourcesName = TileMapping.GetTileResourcesName (tileType);
            if (string.IsNullOrEmpty (resourcesName)) {
                Log.PrintError ("Generate tile failed : resourcesName is empty for tileType : " + tileType, LogTypes.MapData);
                return;
            }

            var tile = Resources.Load<Tile> (resourcesName);
            if (tile == null) {
                Log.PrintError ("Generate tile failed : Cannot load tile resources for resourcesName : " + resourcesName, LogTypes.MapData);
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
            Log.PrintError ("No exit data. Please check.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateExits", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapExit");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go, false);
            var script = go.AddComponent<MapExit> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateExits", LogTypes.MapData);
    }

    private void GenerateEnemy (List<MapData.EnemyData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateEnemy : No enemy data.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateEnemy", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            var enemyType = data.type;
            var resourcesName = EnemyMapping.GetEnemyResourcesName (enemyType);
            if (string.IsNullOrEmpty (resourcesName)) {
                Log.PrintError ("Skipped enemy : resourcesName is empty for enemyType : " + enemyType, LogTypes.MapData);
                continue;
            }

            if (enemyType != EnemyEnum.EnemyType.Boss) {
                var enemy = Resources.Load<EnemyModelBase> (resourcesName);
                if (enemy == null) {
                    Log.PrintError ("Skipped enemy : Cannot load enemy resources for resourcesName : " + resourcesName, LogTypes.MapData);
                    continue;
                }

                var instance = Instantiate (enemy, mapObjectsBaseTransform);
                instance.Reset (data);
                ArrowTargetList.Add (instance);
            } else {
                if (UserManager.SelectedMissionId == MissionManager.EndingMissionId) {
                    if (!UserManager.GetIsCollectedCollectable(Collectable.Type.Ending_1)) {
                        // Do not generate boss if not yet completed Ending_1
                        continue;
                    }
                }
                var boss = Resources.Load<CharModel> (resourcesName);
                if (boss == null) {
                    Log.PrintError ("Skipped enemy : Cannot load enemy resources for resourcesName : " + resourcesName, LogTypes.MapData);
                    continue;
                }

                var instance = Instantiate (boss, mapObjectsBaseTransform);
                instance.Reset (data.pos, data.direction);
                ArrowTargetList.Add (instance);

                BossModel = instance;
            }

        }

        Log.Print ("Finish GenerateEnemy", LogTypes.MapData);

        return;
    }

    #region Map Trigger
    // TODO : Think if it can be simplied by a generic method

    private void GenerateCollectables (int missionId, List<MapData.CollectableData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateCollectables : No collectable data.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateCollectables", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            if (UserManager.GetIsCollectedCollectable (data.type)) {
                // Already collected
                continue;
            }

            var go = Instantiate (collectableTemplate);
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go, false);
            var script = go.GetComponentInChildren<MapCollectableObject> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateCollectables", LogTypes.MapData);
    }

    private void GenerateSwitches (List<MapData.SwitchData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateSwitches : No switch data.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateSwitches", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("MapSwitch");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go, false);
            var script = go.AddComponent<MapSwitch> ();
            script.Init (data);
            mapSwitchList.Add (script);

            switch (data.switchType) {
                case MapEnum.SwitchType.Arrow:
                    ArrowTargetList.Add (script);
                    break;
            }
        }

        Log.Print ("Finish GenerateSwitches", LogTypes.MapData);
    }

    private void GenerateMissionEvents (List<MapData.MissionEventData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateMissionEvents : No mission event data.", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateMissionEvents", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            if (UserManager.CheckIsDoneMissionEvent (data.type)) {
                // Mission event is already done
                continue;
            }
            var go = new GameObject ("MissionEvent");
            FrameworkUtils.InsertChildrenToParent (mapObjectsBaseTransform, go, false);
            var script = go.AddComponent<MissionEventTrigger> ();
            script.Init (data);
        }

        Log.Print ("Finish GenerateMissionEvents", LogTypes.MapData);
    }

    #endregion

    private void GenerateImageBGs (List<MapData.ImageBGData> dataList) {
        if (dataList == null || dataList.Count <= 0) {
            Log.Print ("Skip GenerateImageBGs : No ImageBGData", LogTypes.MapData);
            return;
        } else {
            Log.Print ("Start GenerateImageBGs", LogTypes.MapData);
        }

        foreach (var data in dataList) {
            var go = new GameObject ("ImageBG");
            FrameworkUtils.InsertChildrenToParent (imageBGBase, go, false);

            var image = go.AddComponent<Image> ();
            if (imageBGSpriteDict.ContainsKey (data.type)) {
                image.sprite = imageBGSpriteDict[data.type];
                image.type = Image.Type.Sliced;
            }
            image.color = data.color;

            var rectTransform = go.GetComponent<RectTransform> ();
            rectTransform.position = data.pos;
            rectTransform.sizeDelta = data.size;
        }

        Log.Print ("Finish GenerateImageBGs", LogTypes.MapData);
    }

    #endregion

    #region Map interaction

    private void AddEventHandlers () {
        if (!isAddedEventHandlers) {
            isAddedEventHandlers = true;

            EnemyModelBase.Died += EnemyDiedHandler;
            MapSwitch.Switched += MapSwitchSwitchedHandler;
        }
    }

    private void RemoveEventHandlers () {
        if (isAddedEventHandlers) {
            EnemyModelBase.Died -= EnemyDiedHandler;
            MapSwitch.Switched -= MapSwitchSwitchedHandler;

            isAddedEventHandlers = false;
        }
    }

    private void EnemyDiedHandler (int enemyId) {
        foreach (var target in ArrowTargetList) {
            if (target is EnemyModelBase) {
                if (((EnemyModelBase)target).Id == enemyId) {
                    ArrowTargetList.Remove (target);
                    return;
                }
            }
        }
    }

    private void MapSwitchSwitchedHandler (MapSwitch mapSwitch, bool isSwitchOn) {
        // Tree SwitchType is contrlled by GameSceneManager
        if (mapSwitch.GetSwitchType () != MapEnum.SwitchType.Tree) {
            TriggerMapSwitch (mapSwitch, isSwitchOn);
        }
    }

    public void SwitchOnMapSwitch (MapSwitchSubEvent subEvent, Action onFinished = null) {
        foreach (var mapSwitch in mapSwitchList) {
            if (mapSwitch.GetSwitchId () == subEvent.MapSwitchId) {
                TriggerMapSwitch (mapSwitch, true, onFinished);
                return;
            }
        }

        Log.PrintWarning ("Cannot find mission event switch with id : " + subEvent.MapSwitchId + " . Do not switch on anything.", LogTypes.GameFlow | LogTypes.MapData | LogTypes.MissionEvent);
    }

    private void TriggerMapSwitch (MapSwitch mapSwitch, bool isSwitchOn, Action onFinished = null) {
        Log.Print ("TriggerMapSwitch : Pos : " + mapSwitch.GetTargetPos () + " ; isSwitchOn : " + isSwitchOn, LogTypes.GameFlow | LogTypes.MapData);

        switch (mapSwitch.GetSwitchType ()) {
            case MapEnum.SwitchType.Arrow:
                foreach (var target in ArrowTargetList) {
                    if (target is MapSwitch) {
                        if ((MapSwitch)target == mapSwitch) {
                            ArrowTargetList.Remove (target);
                            break;
                        }
                    }
                }
                break;
            case MapEnum.SwitchType.OnOff:
                SetOnOffSwitch (mapSwitch.GetSwitchBasePos (), isSwitchOn, true);
                break;
        }

        StartCoroutine (OpenHiddenPath (mapSwitch, isSwitchOn, onFinished));
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
            var tileData = new MapData.TileData (new Vector2Int (switchBasePos.x + pair.Key.x, switchBasePos.y + pair.Key.y), tileType, MapEnum.TileMapType.Background);
            GenerateTile (tileData, pair.Value != null, false);
        }

        if (isChangedInGame) {
            if (switchedOnOnOffSwitchBasePosList.Contains (switchBasePos)) {
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
    
    private IEnumerator OpenHiddenPath (MapSwitch mapSwitch, bool isSwitchOn, Action onFinished = null) {
        if (mapSwitch == null) {
            yield break;
        }

        var pathDataList = mapSwitch.GetHiddenPathDataList ();
        if (pathDataList == null || pathDataList.Count <= 0) {
            mapSwitch.SwitchingFinished ();
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

            yield return new WaitForSecondsRealtime (OpenOneHiddenPathLayerPeriod);
        }

        mapSwitch.SwitchingFinished ();
        onFinished?.Invoke ();
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