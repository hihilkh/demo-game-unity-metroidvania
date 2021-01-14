using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

[Serializable]
public class MapData {
    public CharData charData;
    public List<TileData> tiles;

    public MapData () { }

    public MapData (CharData charData, List<TileData> tiles) {
        this.charData = charData;
        this.tiles = tiles;
    }

    [Serializable]
    public class CharData {
        public float x;
        public float y;
        public bool isFacingRight;

        public CharData () { }

        public CharData (float x, float y, bool isFacingRight) {
            this.x = x;
            this.y = y;
            this.isFacingRight = isFacingRight;
        }

        public Vector3 GetPos () {
            return new Vector3 (x, y, GameVariable.CharPosZ);
        }

        public CharEnum.HorizontalDirection GetDirection () {
            return isFacingRight ? CharEnum.HorizontalDirection.Right : CharEnum.HorizontalDirection.Left;
        }
    }

    [Serializable]
    public class TileData {
        public int x;
        public int y;
        public int tileType;
        public int tileMapType;

        public TileData () { }

        public TileData (int x, int y, MapEnum.TileType tileType, MapEnum.TileMapType tileMapType) {
            this.x = x;
            this.y = y;
            this.tileType = (int)tileType;
            this.tileMapType = (int)tileMapType;
        }

        public Vector3Int GetPos () {
            return new Vector3Int (x, y, GameVariable.TilePosZ);
        }

        public MapEnum.TileType GetTileType () {
            if (Enum.IsDefined (typeof (MapEnum.TileType), tileType)) {
                return (MapEnum.TileType)tileType;
            } else {
                Log.PrintError ("Invalid tileType : " + tileType + " . Pos : (" + x + ", " + y + "). Use default.", LogType.MapData);
                return default;
            }
        }

        public MapEnum.TileMapType GetTileMapType () {
            if (Enum.IsDefined (typeof (MapEnum.TileMapType), tileMapType)) {
                return (MapEnum.TileMapType)tileMapType;
            } else {
                Log.PrintError ("Invalid tileMapType : " + tileMapType + " . Pos : (" + x + ", " + y + "). Use default.", LogType.MapData);
                return default;
            }
        }
    }
}