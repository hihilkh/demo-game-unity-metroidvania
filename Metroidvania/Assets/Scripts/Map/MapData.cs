using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

[Serializable]
public class MapData {
    public Boundary boundary;
    public List<TileData> tiles;                // TilePosData
    public List<SwitchData> switches;           // InvisibleTriggerData
    public List<EntryData> entries;             // WorldPosDirectionData
    public List<EnemyData> enemies;             // WorldPosDirectionData
    public List<CollectableData> collectables;  // WorldPosData
    public List<ExitData> exits;                // InvisibleTriggerData
    public List<TutorialData> tutorials;        // InvisibleTriggerData

    public MapData () { }

    [Serializable]
    public class Boundary {
        public int[] lowerBound;
        public int[] upperBound;

        public Boundary () { }

        public Boundary (Vector2Int lowerBound, Vector2Int upperBound) {
            this.lowerBound = new int[] { lowerBound.x, lowerBound.y };
            this.upperBound = new int[] { upperBound.x, upperBound.y };
        }

        public Vector2Int GetLowerBound () {
            return new Vector2Int (lowerBound[0], lowerBound[1]);
        }

        public Vector2Int GetUpperBound () {
            return new Vector2Int (upperBound[0], upperBound[1]);
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
            MapEnum.TileType result;
            if (!FrameworkUtils.TryParseToEnum (tileType, out result)) {
                Log.PrintError ("Invalid TileType : " + tileType + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
            }

            return result;
        }

        public MapEnum.TileMapType GetTileMapType () {
            MapEnum.TileMapType result;
            if (!FrameworkUtils.TryParseToEnum (tileMapType, out result)) {
                Log.PrintError ("Invalid TileMapType : " + tileMapType + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
            }

            return result;
        }
    }

    [Serializable]
    public class SwitchData : InvisibleTriggerData {
        public int switchType;
        public HiddenPathData hiddenPath;

        public SwitchData () { }

        public SwitchData (float x, float y, float sizeX, float sizeY, MapEnum.SwitchType switchType, MapEnum.HiddenPathOpenType hiddenPathOpenType, List<Vector2Int> hiddenPathTilesPos) : base (x, y, sizeX, sizeY) {
            this.switchType = (int)switchType;
            this.hiddenPath = new HiddenPathData (hiddenPathOpenType, hiddenPathTilesPos);
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
        public int id;
        public int type;
        protected override float posZ => GameVariable.EnemyPosZ;

        public EnemyData () { }

        public EnemyData (float x, float y, LifeEnum.HorizontalDirection direction, int id, EnemyEnum.EnemyType type) : base (x, y, direction) {
            this.id = id;
            this.type = (int)type;
        }

        public EnemyEnum.EnemyType GetEnemyType () {
            EnemyEnum.EnemyType result;
            if (!FrameworkUtils.TryParseToEnum (type, out result)) {
                Log.PrintError ("Invalid EnemyType : " + type + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
            }

            return result;
        }
    }

    [Serializable]
    public class CollectableData : WorldPosData {
        public int type;
        public int fromEnemyId = -1;
        public bool isFromEnemy => fromEnemyId > -1;

        public CollectableData () { }

        public CollectableData (float x, float y, MissionCollectable.Type type) : base (x, y) {
            this.type = (int)type;
        }

        public CollectableData (float x, float y, MissionCollectable.Type type, int fromEnemyId) : this (x, y, type) {
            this.fromEnemyId = fromEnemyId;
        }

        public MissionCollectable.Type GetCollectableType () {
            MissionCollectable.Type result;
            if (!FrameworkUtils.TryParseToEnum (type, out result)) {
                Log.PrintError ("Invalid MissionCollectable.Type : " + type + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
            }

            return result;
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
            TutorialEnum.GameTutorialType result;
            if (!FrameworkUtils.TryParseToEnum (type, out result)) {
                Log.PrintError ("Invalid GameTutorialType : " + type + " . Use default.", LogType.MapData);
            }

            return result;
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
            LifeEnum.HorizontalDirection result;
            if (!FrameworkUtils.TryParseToEnum (direction, out result)) {
                Log.PrintError ("Invalid HorizontalDirection : " + direction + " . Pos : " + GetPos () + ". Use default.", LogType.MapData);
            }

            return result;
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

        public HiddenPathData (MapEnum.HiddenPathOpenType openType, List<Vector2Int> tilesPos) {
            this.type = (int)openType;
            this.tilesPos = new List<int[]> ();

            foreach (var pos in tilesPos) {
                this.tilesPos.Add (new int[] { pos.x, pos.y });
            }
        }

        private MapEnum.HiddenPathOpenType GetOpenType () {
            MapEnum.HiddenPathOpenType result;
            if (!FrameworkUtils.TryParseToEnum (type, out result)) {
                Log.PrintError ("Invalid HiddenPathOpenType : " + type + " . Use default.", LogType.MapData);
            }

            return result;
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