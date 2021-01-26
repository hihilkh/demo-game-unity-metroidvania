using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

[Serializable]
public class MapData {
    public Boundary boundary;
    public List<TileData> tiles;                // TilePosData
    public List<SwitchData> switches;           // TilePosData
    public List<EntryData> entries;             // WorldPosDirectionData
    public List<EnemyData> enemies;             // WorldPosDirectionData
    public List<CollectableData> collectables;  // WorldPosData
    public List<ExitData> exits;                // InvisibleTriggerData
    public List<TutorialData> tutorials;        // InvisibleTriggerData

    private const float TilePosToWorldPosOffsetX = 0.5f;
    private const float TilePosToWorldPosOffsetY = 0.5f;

    private const float GeneralSwitchColliderSize = 0.5f;

    public MapData () { }

    public MapData (List<TileData> tiles) {
        this.tiles = tiles;
    }

    [Serializable]
    public class Boundary {
        public float[] lowerBound;
        public float[] upperBound;

        public Boundary () { }

        public Boundary (float minX, float minY, float maxX, float maxY) {
            this.lowerBound = new float[] { minX, minY };
            this.upperBound = new float[] { maxX, maxY };
        }

        public Vector2 GetLowerBound () {
            return new Vector2 (lowerBound[0], lowerBound[1]);
        }

        public Vector2 GetUpperBound () {
            return new Vector2 (upperBound[0], upperBound[1]);
        }
    }

    [Serializable]
    public class TileData : TilePosData {
        public int tileType;
        public int tileMapType;

        public TileData () { }

        public TileData (int x, int y, MapEnum.TileType tileType, MapEnum.TileMapType tileMapType) : base (x, y) {
            this.tileType = (int)tileType;
            this.tileMapType = (int)tileMapType;
        }

        public MapEnum.TileType GetTileType () {
            if (Enum.IsDefined (typeof (MapEnum.TileType), tileType)) {
                return (MapEnum.TileType)tileType;
            } else {
                Log.PrintError ("Invalid TileType : " + tileType + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
                return default;
            }
        }

        public MapEnum.TileMapType GetTileMapType () {
            if (Enum.IsDefined (typeof (MapEnum.TileMapType), tileMapType)) {
                return (MapEnum.TileMapType)tileMapType;
            } else {
                Log.PrintError ("Invalid TileMapType : " + tileMapType + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
                return default;
            }
        }
    }

    [Serializable]
    public class SwitchData : TilePosData {
        public int switchType;
        public HiddenPathData hiddenPath;

        public SwitchData () { }

        public SwitchData (int x, int y, MapEnum.SwitchType switchType, MapEnum.HiddenPathOpenType openType, List<Vector2Int> hiddenPathTilesPos) : base (x, y) {
            this.switchType = (int)switchType;
            this.hiddenPath = new HiddenPathData (openType, hiddenPathTilesPos);
        }

        public ColliderData GetColliderData () {
            return new ColliderData (pos[0] + TilePosToWorldPosOffsetX, pos[1] + TilePosToWorldPosOffsetY, GeneralSwitchColliderSize, GeneralSwitchColliderSize);
        }
    }

    [Serializable]
    public class EntryData : WorldPosDirectionData {
        public int id;
        protected override float posZ => GameVariable.CharPosZ;

        public EntryData () { }

        public EntryData (float x, float y, LifeEnum.HorizontalDirection direction, int id) : base (x, y, direction) {
            this.id = id;
        }
    }

    [Serializable]
    public class EnemyData : WorldPosDirectionData {
        public int type;
        protected override float posZ => GameVariable.EnemyPosZ;

        public EnemyData () { }

        public EnemyData (float x, float y, LifeEnum.HorizontalDirection direction, EnemyEnum.EnemyType type) : base (x, y, direction) {
            this.type = (int)type;
        }

        public EnemyEnum.EnemyType GetEnemyType () {
            if (Enum.IsDefined (typeof (EnemyEnum.EnemyType), type)) {
                return (EnemyEnum.EnemyType)type;
            } else {
                Log.PrintError ("Invalid EnemyType : " + type + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
                return default;
            }
        }
    }

    [Serializable]
    public class CollectableData : WorldPosData {
        public int type;

        public CollectableData () { }

        public CollectableData (float x, float y, MissionCollectable.Type type) : base (x, y) {
            this.type = (int)type;
        }

        public MissionCollectable.Type GetCollectableType () {
            if (Enum.IsDefined (typeof (MissionCollectable.Type), type)) {
                return (MissionCollectable.Type)type;
            } else {
                Log.PrintError ("Invalid MissionCollectable.Type : " + type + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
                return default;
            }
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
        public int type;

        public TutorialData () { }

        public TutorialData (float x, float y, float sizeX, float sizeY, TutorialEnum.GameTutorialType type) : base (x, y, sizeX, sizeY) {
            this.type = (int)type;
        }

        public TutorialEnum.GameTutorialType GetTutorialType () {
            if (Enum.IsDefined (typeof (TutorialEnum.GameTutorialType), type)) {
                return (TutorialEnum.GameTutorialType)type;
            } else {
                Log.PrintError ("Invalid GameTutorialType : " + type + " . Use default.", LogType.MapData);
                return default;
            }
        }
    }

    #region Base Class

    [Serializable]
    public abstract class TilePosData {
        public int[] pos;

        public TilePosData () { }

        public TilePosData (int x, int y) {
            this.pos = new int[] { x, y };
        }

        public Vector3Int GetPos () {
            return new Vector3Int (pos[0], pos[1], GameVariable.TilePosZ);
        }
    }

    [Serializable]
    public abstract class WorldPosData {
        public float[] pos;
        protected virtual float posZ => GameVariable.GeneralMapItemPosZ;

        public WorldPosData () { }

        public WorldPosData (float x, float y) {
            this.pos = new float[] { x, y };
        }

        public Vector3 GetPos () {
            return new Vector3 (pos[0], pos[1], posZ);
        }
    }

    [Serializable]
    public abstract class WorldPosDirectionData : WorldPosData {
        public int direction;

        public WorldPosDirectionData () { }

        public WorldPosDirectionData (float x, float y, LifeEnum.HorizontalDirection direction) : base (x, y) {
            this.direction = (int)direction;
        }

        public LifeEnum.HorizontalDirection GetDirection () {
            if (Enum.IsDefined (typeof (LifeEnum.HorizontalDirection), direction)) {
                return (LifeEnum.HorizontalDirection)direction;
            } else {
                Log.PrintError ("Invalid HorizontalDirection : " + direction + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
                return default;
            }
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
        public float[] pos;
        public float[] size;

        public ColliderData () { }

        public ColliderData (float x, float y, float sizeX, float sizeY) {
            this.pos = new float[] { x, y };
            this.size = new float[] { sizeX, sizeY };
        }

        public Vector3 GetPos () {
            return new Vector3 (pos[0], pos[1], GameVariable.GeneralMapItemPosZ);
        }

        public Vector2 GetSize () {
            return new Vector2 (size[0], size[1]);
        }
    }

    [Serializable]
    public class HiddenPathData {
        public int type;
        public List<int[]> tilesPos;

        public HiddenPathData () { }

        public HiddenPathData (MapEnum.HiddenPathOpenType type, List<Vector2Int> tilesPos) {
            this.type = (int)type;
            this.tilesPos = new List<int[]> ();

            foreach (var pos in tilesPos) {
                this.tilesPos.Add (new int[] { pos.x, pos.y });
            }
        }

        private MapEnum.HiddenPathOpenType GetOpenType () {
            if (Enum.IsDefined (typeof (MapEnum.HiddenPathOpenType), type)) {
                return (MapEnum.HiddenPathOpenType)type;
            } else {
                Log.PrintError ("Invalid HiddenPathOpenType : " + type + " . Use default.", LogType.MapData);
                return default;
            }
        }

        public List<List<Vector3Int>> GetTilesPosByOpenOrder () {
            if (tilesPos.Count <= 0) {
                return new List<List<Vector3Int>> ();
            }

            var tilesInOrder = new List<int[]> (tilesPos);

            var openType = GetOpenType ();
            Comparison<int[]> comparison;

            switch (openType) {
                case MapEnum.HiddenPathOpenType.LeftToRight:
                    comparison = (pos1, pos2) => {
                        return pos1[0].CompareTo (pos2[0]);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.RightToLeft:
                    comparison = (pos1, pos2) => {
                        return pos2[0].CompareTo (pos1[0]);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.UpToDown:
                    comparison = (pos1, pos2) => {
                        return pos2[1].CompareTo (pos1[1]);
                    };
                    break;
                case MapEnum.HiddenPathOpenType.DownToUp:
                default:
                    comparison = (pos1, pos2) => {
                        return pos1[1].CompareTo (pos2[1]);
                    };
                    break;
            }

            tilesInOrder.Sort (comparison);

            var result = new List<List<Vector3Int>> ();

            int[] compareItem = null;
            var counter = -1;

            foreach (var tilePos in tilesInOrder) {
                if (comparison (tilePos, compareItem) == 0) {
                    result[counter].Add (new Vector3Int (tilePos[0], tilePos[1], GameVariable.TilePosZ));
                } else {
                    result.Add (new List<Vector3Int> ());
                    counter++;

                    result[counter].Add (new Vector3Int (tilePos[0], tilePos[1], GameVariable.TilePosZ));
                    compareItem = tilePos;
                }
            }

            return result;
        }
    }

    #endregion
}