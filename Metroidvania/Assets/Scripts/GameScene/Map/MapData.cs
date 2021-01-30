using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
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
    public List<TutorialData> tutorials;        // InvisibleTriggerData

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
        public MapEnum.SwitchType switchType;
        public HiddenPathData hiddenPath;

        public SwitchData () { }

        public SwitchData (float x, float y, float sizeX, float sizeY, MapEnum.SwitchType switchType, MapEnum.HiddenPathOpenType hiddenPathOpenType, List<Vector2Int> hiddenPathTilesPos) : base (x, y, sizeX, sizeY) {
            this.switchType = switchType;
            this.hiddenPath = new HiddenPathData (hiddenPathOpenType, hiddenPathTilesPos);
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
        public MapCollectable.Type type;
        public int fromEnemyId = -1;
        public bool isFromEnemy => fromEnemyId > -1;

        public CollectableData () { }

        public CollectableData (float x, float y, MapCollectable.Type type) : base (x, y) {
            this.type = type;
        }

        public CollectableData (float x, float y, MapCollectable.Type type, int fromEnemyId) : this (x, y, type) {
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
    public class TutorialData : InvisibleTriggerData {
        public TutorialEnum.GameTutorialType type;

        public TutorialData () { }

        public TutorialData (float x, float y, float sizeX, float sizeY, TutorialEnum.GameTutorialType type) : base (x, y, sizeX, sizeY) {
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
        public MapEnum.HiddenPathOpenType openType;
        public List<Vector2Int> tilesPos;

        public HiddenPathData () { }

        public HiddenPathData (MapEnum.HiddenPathOpenType openType, List<Vector2Int> tilesPos) {
            this.openType = openType;
            this.tilesPos = new List<Vector2Int> ();

            foreach (var pos in tilesPos) {
                this.tilesPos.Add (pos);
            }
        }

        public List<List<Vector3Int>> GetTilesPosByOpenOrder () {
            if (tilesPos.Count <= 0) {
                return new List<List<Vector3Int>> ();
            }

            var tilesInOrder = new List<Vector2Int> (tilesPos);

            Comparison<Vector2Int> comparison;

            switch (openType) {
                case MapEnum.HiddenPathOpenType.LeftToRight:
                    comparison = (pos1, pos2) => {
                        return pos1.x.CompareTo (pos2.x);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.RightToLeft:
                    comparison = (pos1, pos2) => {
                        return pos2.x.CompareTo (pos1.x);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.UpToDown:
                    comparison = (pos1, pos2) => {
                        return pos2.y.CompareTo (pos1.y);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.DownToUp:
                default:
                    comparison = (pos1, pos2) => {
                        return pos1.y.CompareTo (pos2.y);
                    };
                    break;
            }

            tilesInOrder.Sort (comparison);

            var result = new List<List<Vector3Int>> ();

            Vector2Int? compareItem = null;
            var counter = -1;

            foreach (var tilePos in tilesInOrder) {
                if (compareItem != null && comparison (tilePos, (Vector2Int)compareItem) == 0) {
                    result[counter].Add (new Vector3Int (tilePos.x, tilePos.y, GameVariable.TilePosZ));
                } else {
                    result.Add (new List<Vector3Int> ());
                    counter++;

                    result[counter].Add (new Vector3Int (tilePos.x, tilePos.y, GameVariable.TilePosZ));
                    compareItem = tilePos;
                }
            }

            return result;
        }

    }

    #endregion
}