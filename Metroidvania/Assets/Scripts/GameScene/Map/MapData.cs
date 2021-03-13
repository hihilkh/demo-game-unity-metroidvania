using System;
using System.Collections.Generic;
using HihiFramework.Core;
using UnityEngine;

[Serializable]
public class MapData {
    public Boundary boundary;
    public List<TileData> tiles;                // TilePosData
    public List<EntryData> entries;             // WorldPosDirectionData
    public List<EnemyData> enemies;             // WorldPosDirectionData
    public List<CollectableData> collectables;  // WorldPosData
    public List<SwitchData> switches;           // InvisibleTriggerData
    public List<ExitData> exits;                // InvisibleTriggerData
    public List<MissionEventData> events;        // InvisibleTriggerData
    public List<ImageBGData> backgrounds;
    public List<SpecialSceneData> specialScenes;

    public MapData () { }

    public EntryData GetEntryData (int entryId) {
        foreach (var entryData in entries) {
            if (entryData.id == entryId) {
                return entryData;
            }
        }

        return null;
    }

    /// <summary>
    /// Return the first TileData which meet the requirement
    /// </summary>
    public TileData GetTileData (Vector2Int pos) {
        foreach (var tile in tiles) {
            if (tile.pos == pos) {
                return tile;
            }
        }

        return null;
    }

    /// <summary>
    /// Return the first TileData which meet the requirement
    /// </summary>
    public TileData GetTileData (Vector2Int pos, MapEnum.TileMapType tileMapType) {
        foreach (var tile in tiles) {
            if (tile.pos == pos) {
                if (tile.tileMapType == tileMapType) {
                    return tile;
                }
            }
        }

        return null;
    }

    public bool RemoveTileData (TileData tileData) {
        return tiles.Remove (tileData);
    }

    public SubSpecialSceneData GetSubSpecialSceneData (MissionEventEnum.SpecialSceneType specialSceneType, int subSpecialSceneIndex) {
        foreach (var specialScene in specialScenes) {
            if (specialScene.type == specialSceneType) {
                if (specialScene.subSpecialScenes != null && specialScene.subSpecialScenes.Count > subSpecialSceneIndex) {
                    return specialScene.subSpecialScenes[subSpecialSceneIndex];
                }
            }
        }

        return null;
    }

    [Serializable]
    public class Boundary {
        public Vector2Int lowerBound;
        public Vector2Int upperBound;

        public Boundary () { }

        public Boundary (Vector2Int lowerBound, Vector2Int upperBound) {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }
    }

    [Serializable]
    public class TileData : TilePosData {
        public MapEnum.TileType tileType;
        public MapEnum.TileMapType tileMapType;

        public TileData () { }

        public TileData (Vector2Int pos, MapEnum.TileType tileType, MapEnum.TileMapType tileMapType) : base (pos) {
            this.tileType = tileType;
            this.tileMapType = tileMapType;
        }
    }

    [Serializable]
    public class SwitchData : InvisibleTriggerData {
        public int id;
        public MapEnum.SwitchType switchType;
        public Vector2Int switchBasePos;    // SwitchBasePos is the bottom left position of the switch
        public List<HiddenPathData> hiddenPaths;
        public int fromEnemyId = -1;

        public SwitchData () { }

        public SwitchData (int id, Vector2 pos, Vector2 size, MapEnum.SwitchType switchType, Vector2Int switchBasePos) : base (pos, size) {
            this.id = id;
            this.switchType = switchType;
            this.switchBasePos = switchBasePos;
            this.hiddenPaths = new List<HiddenPathData> ();
        }

        // pos / collider sizes / switchBasePos of MissionEventSwitch are just dummy
        public SwitchData (int id) : this (id, Vector2.zero, Vector2.one, MapEnum.SwitchType.MissionEvent, Vector2Int.zero) {
        }

        // pos / collider sizes / switchBasePos of EnemySwitch are just dummy
        public SwitchData (int id, int fromEnemyId) : this (id, Vector2.zero, Vector2.one, MapEnum.SwitchType.Enemy, Vector2Int.zero) {
            this.fromEnemyId = fromEnemyId;
        }

        public void AddHiddenPath (MapEnum.HiddenPathType hiddenPathType, MapEnum.HiddenPathOrdering hiddenPathOrdering, List<TileData> hiddenPathTiles) {
            hiddenPaths.Add (new HiddenPathData (hiddenPathType, hiddenPathOrdering, hiddenPathTiles));
        }
    }

    [Serializable]
    public class EntryData : WorldPosDirectionData {
        public int id;

        public EntryData () { }

        public EntryData (Vector2 pos, LifeEnum.HorizontalDirection direction, int id) : base (pos, direction) {
            this.id = id;
        }
    }

    [Serializable]
    public class EnemyData : WorldPosDirectionData {
        public int id;
        public EnemyEnum.EnemyType type;

        public EnemyData () { }

        public EnemyData (Vector2 pos, LifeEnum.HorizontalDirection direction, int id, EnemyEnum.EnemyType type) : base (pos, direction) {
            this.id = id;
            this.type = type;
        }
    }

    [Serializable]
    public class CollectableData : WorldPosData {
        public Collectable.Type type;
        public int fromEnemyId = -1;
        public bool IsFromEnemy => fromEnemyId > -1;

        public CollectableData () { }

        public CollectableData (Vector2 pos, Collectable.Type type) : base (pos) {
            this.type = type;
        }

        public CollectableData (Vector2 pos, Collectable.Type type, int fromEnemyId) : this (pos, type) {
            this.fromEnemyId = fromEnemyId;
        }
    }

    [Serializable]
    public class ExitData : InvisibleTriggerData {
        public int toEntryId;
        public bool isSpecialSceneExit;
        public MissionEventEnum.SpecialSceneType specialSceneExitType;

        public ExitData () { }

        public ExitData (Vector2 pos, Vector2 size, int toEntryId) : base (pos, size) {
            this.toEntryId = toEntryId;
            this.isSpecialSceneExit = false;
        }

        public ExitData (Vector2 pos, Vector2 size, MissionEventEnum.SpecialSceneType specialSceneExitType) : base (pos, size) {
            this.isSpecialSceneExit = true;
            this.specialSceneExitType = specialSceneExitType;
        }
    }

    [Serializable]
    public class MissionEventData : InvisibleTriggerData {
        public MissionEventEnum.EventType type;

        public MissionEventData () { }

        public MissionEventData (Vector2 pos, Vector2 size, MissionEventEnum.EventType type) : base (pos, size) {
            this.type = type;
        }
    }

    [Serializable]
    public class ImageBGData {
        public MapEnum.ImageBGType type;
        public Vector3 pos;
        public Vector2 size;
        public Color32 color;

        public ImageBGData () { }

        public ImageBGData (MapEnum.ImageBGType type, Vector3 pos, Vector2 size, Color32 color) {
            this.type = type;
            this.pos = pos;
            this.size = size;
            this.color = color;
        }
    }

    [Serializable]
    public class SpecialSceneData {
        public MissionEventEnum.SpecialSceneType type;
        public List<SubSpecialSceneData> subSpecialScenes;

        public SpecialSceneData () { }

        public SpecialSceneData (MissionEventEnum.SpecialSceneType type) {
            this.type = type;
            subSpecialScenes = new List<SubSpecialSceneData> ();
        }

        public void AddSubSpecialSceneData (Vector2 cameraPos, bool isNeedCaveCollapseEffect, Vector2? playerPos, LifeEnum.HorizontalDirection? playerDirection, Vector2? bossPos, LifeEnum.HorizontalDirection? bossDirection) {
            subSpecialScenes.Add (new SubSpecialSceneData (cameraPos, isNeedCaveCollapseEffect, playerPos, playerDirection, bossPos, bossDirection));
        }
    }

    [Serializable]
    public class SubSpecialSceneData {
        public Vector2 cameraPos;
        public bool isNeedCaveCollapseEffect;
        public WorldPosDirectionData player;
        public WorldPosDirectionData boss;

        public SubSpecialSceneData () { }

        public SubSpecialSceneData (Vector2 cameraPos, bool isNeedCaveCollapseEffect, Vector2? playerPos, LifeEnum.HorizontalDirection? playerDirection, Vector2? bossPos, LifeEnum.HorizontalDirection? bossDirection) {
            this.cameraPos = cameraPos;
            this.isNeedCaveCollapseEffect = isNeedCaveCollapseEffect;
            if (playerPos == null || playerDirection == null) {
                player = null;
            } else {
                player = new WorldPosDirectionData ((Vector2)playerPos, (LifeEnum.HorizontalDirection)playerDirection);
            }

            if (bossPos == null || bossDirection == null) {
                boss = null;
            } else {
                boss = new WorldPosDirectionData ((Vector2)bossPos, (LifeEnum.HorizontalDirection)bossDirection);
            }
        }
    }

    #region Base Class

    [Serializable]
    public class TilePosData {
        public Vector2Int pos;

        public TilePosData () { }

        public TilePosData (Vector2Int pos) {
            this.pos = pos;
        }
    }

    [Serializable]
    public class WorldPosData {
        public Vector2 pos;

        public WorldPosData () { }

        public WorldPosData (Vector2 pos) {
            this.pos = pos;
        }
    }

    [Serializable]
    public class WorldPosDirectionData : WorldPosData {
        public LifeEnum.HorizontalDirection direction;

        public WorldPosDirectionData () { }

        public WorldPosDirectionData (Vector2 pos, LifeEnum.HorizontalDirection direction) : base (pos) {
            this.direction = direction;
        }
    }

    [Serializable]
    public class InvisibleTriggerData {
        public ColliderData collider;

        public InvisibleTriggerData () { }

        public InvisibleTriggerData (Vector2 pos, Vector2 size) {
            this.collider = new ColliderData (pos, size);
        }
    }

    #endregion

    #region Common Class

    [Serializable]
    public class ColliderData {
        public Vector2 pos;
        public Vector2 size;

        public ColliderData () { }

        public ColliderData (Vector2 pos, Vector2 size) {
            this.pos = pos;
            this.size = size;
        }
    }

    [Serializable]
    public class HiddenPathData {
        public MapEnum.HiddenPathType type;
        public MapEnum.HiddenPathOrdering ordering;
        public List<TileData> tiles;

        public HiddenPathData () { }

        public HiddenPathData (MapEnum.HiddenPathType type, MapEnum.HiddenPathOrdering ordering, List<TileData> tiles) {
            this.type = type;
            this.ordering = ordering;
            this.tiles = tiles;
        }

        public List<List<TileData>> GetTilesPosByOrdering (bool isSwitchOn) {
            if (tiles.Count <= 0) {
                return new List<List<TileData>> ();
            }

            var tilesInOrder = new List<TileData> (tiles);

            Comparison<TileData> comparison;

            var runtimeOrdering = ordering;

            if (!isSwitchOn) {  // invert ordering
                switch (ordering) {
                    case MapEnum.HiddenPathOrdering.LeftToRight:
                        runtimeOrdering = MapEnum.HiddenPathOrdering.RightToLeft;
                        break;
                    case MapEnum.HiddenPathOrdering.RightToLeft:
                        runtimeOrdering = MapEnum.HiddenPathOrdering.LeftToRight;
                        break;
                    case MapEnum.HiddenPathOrdering.UpToDown:
                        runtimeOrdering = MapEnum.HiddenPathOrdering.DownToUp;
                        break;
                    case MapEnum.HiddenPathOrdering.DownToUp:
                        runtimeOrdering = MapEnum.HiddenPathOrdering.UpToDown;
                        break;
                }
            }

            switch (runtimeOrdering) {
                case MapEnum.HiddenPathOrdering.LeftToRight:
                    comparison = (tile1, tile2) => {
                        return tile1.pos.x.CompareTo (tile2.pos.x);
                    };
                    break;
                case MapEnum.HiddenPathOrdering.RightToLeft:
                    comparison = (tile1, tile2) => {
                        return tile2.pos.x.CompareTo (tile1.pos.x);
                    };
                    break;
                case MapEnum.HiddenPathOrdering.UpToDown:
                    comparison = (tile1, tile2) => {
                        return tile2.pos.y.CompareTo (tile1.pos.y);
                    };
                    break;
                case MapEnum.HiddenPathOrdering.DownToUp:
                default:
                    comparison = (tile1, tile2) => {
                        return tile1.pos.y.CompareTo (tile2.pos.y);
                    };
                    break;
            }

            tilesInOrder.Sort (comparison);

            var result = new List<List<TileData>> ();

            TileData compareItem = null;
            var counter = -1;

            foreach (var tile in tilesInOrder) {
                if (compareItem != null && comparison (tile, compareItem) == 0) {
                    result[counter].Add (tile);
                } else {
                    result.Add (new List<TileData> ());
                    counter++;

                    result[counter].Add (tile);
                    compareItem = tile;
                }
            }

            return result;
        }

    }

    #endregion
}