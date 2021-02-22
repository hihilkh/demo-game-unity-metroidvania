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

    public MapData () { }

    public EntryData GetEntryData (int entryId) {
        foreach (var entryData in entries) {
            if (entryData.id == entryId) {
                return entryData;
            }
        }

        return null;
    }

    public TileData GetTileData (Vector2Int pos) {
        foreach (var tile in tiles) {
            if (tile.pos == pos) {
                return tile;
            }
        }

        return null;
    }

    public bool RemoveTileData (TileData tileData) {
        return tiles.Remove (tileData);
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

        public TileData (int x, int y, MapEnum.TileType tileType, MapEnum.TileMapType tileMapType) : base (x, y) {
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

        public SwitchData (int id, float x, float y, float sizeX, float sizeY, MapEnum.SwitchType switchType, Vector2Int switchBasePos) : base (x, y, sizeX, sizeY) {
            this.id = id;
            this.switchType = switchType;
            this.switchBasePos = switchBasePos;
            this.hiddenPaths = new List<HiddenPathData> ();
        }

        // pos / collider sizes / switchBasePos of MissionEventSwitch are just dummy
        public SwitchData (int id) : this (id, 0, 0, 1, 1, MapEnum.SwitchType.MissionEvent, Vector2Int.zero) {
        }

        // pos / collider sizes / switchBasePos of EnemySwitch are just dummy
        public SwitchData (int id, int fromEnemyId) : this (id, 0, 0, 1, 1, MapEnum.SwitchType.Enemy, Vector2Int.zero) {
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

        public EntryData (float x, float y, LifeEnum.HorizontalDirection direction, int id) : base (x, y, direction) {
            this.id = id;
        }
    }

    [Serializable]
    public class EnemyData : WorldPosDirectionData {
        public int id;
        public EnemyEnum.EnemyType type;

        public EnemyData () { }

        public EnemyData (float x, float y, LifeEnum.HorizontalDirection direction, int id, EnemyEnum.EnemyType type) : base (x, y, direction) {
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

        public CollectableData (float x, float y, Collectable.Type type) : base (x, y) {
            this.type = type;
        }

        public CollectableData (float x, float y, Collectable.Type type, int fromEnemyId) : this (x, y, type) {
            this.fromEnemyId = fromEnemyId;
        }
    }

    [Serializable]
    public class ExitData : InvisibleTriggerData {
        public int toEntryId;

        public ExitData () { }

        public ExitData (float x, float y, float sizeX, float sizeY, int toEntryId) : base (x, y, sizeX, sizeY) {
            this.toEntryId = toEntryId;
        }
    }

    [Serializable]
    public class MissionEventData : InvisibleTriggerData {
        public MissionEventEnum.EventType type;

        public MissionEventData () { }

        public MissionEventData (float x, float y, float sizeX, float sizeY, MissionEventEnum.EventType type) : base (x, y, sizeX, sizeY) {
            this.type = type;
        }
    }

    #region Base Class

    [Serializable]
    public abstract class TilePosData {
        public Vector2Int pos;

        public TilePosData () { }

        public TilePosData (int x, int y) {
            this.pos = new Vector2Int (x, y);
        }
    }

    [Serializable]
    public abstract class WorldPosData {
        public Vector2 pos;

        public WorldPosData () { }

        public WorldPosData (float x, float y) {
            this.pos = new Vector2 (x, y);
        }
    }

    [Serializable]
    public abstract class WorldPosDirectionData : WorldPosData {
        public LifeEnum.HorizontalDirection direction;

        public WorldPosDirectionData () { }

        public WorldPosDirectionData (float x, float y, LifeEnum.HorizontalDirection direction) : base (x, y) {
            this.direction = direction;
        }
    }

    [Serializable]
    public abstract class InvisibleTriggerData {
        public ColliderData collider;

        public InvisibleTriggerData () { }

        public InvisibleTriggerData (float x, float y, float sizeX, float sizeY) {
            this.collider = new ColliderData (x, y, sizeX, sizeY);
        }
    }

    #endregion

    #region Common Class

    [Serializable]
    public class ColliderData {
        public Vector2 pos;
        public Vector2 size;

        public ColliderData () { }

        public ColliderData (float x, float y, float sizeX, float sizeY) {
            this.pos = new Vector2 (x, y);
            this.size = new Vector2 (sizeX, sizeY);
        }
    }

    [Serializable]
    public class HiddenPathData {
        public MapEnum.HiddenPathType type;
        public MapEnum.HiddenPathOrdering ordering;
        public List<TileData> tiles;
        public List<Vector2Int> tilesPos;

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