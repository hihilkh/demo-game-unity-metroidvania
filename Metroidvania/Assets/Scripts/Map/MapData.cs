using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

[Serializable]
public class MapData {
    public CharData charData;
    public List<TileData> tiles;

    [Serializable]
    public class CharData {
        public float x;
        public float y;
        public bool isFacingRight;

        public Vector3 GetPos () {
            return new Vector3 (x, y, 0);
        }

        public CharEnum.Direction GetDirection () {
            return isFacingRight ? CharEnum.Direction.Right : CharEnum.Direction.Left;
        }
    }

    [Serializable]
    public class TileData {
        public int x;
        public int y;
        public int tileType;
        public int tileTag;

        public Vector3Int GetPos () {
            return new Vector3Int (x, y, 0);
        }

        public MapEnum.TileType GetTileType () {
            if (Enum.IsDefined (typeof (MapEnum.TileType), tileType)) {
                return (MapEnum.TileType)tileType;
            } else {
                Log.PrintError ("Invalid tileType : " + tileType + " . Pos : (" + x + ", " + y + "). Use default.");
                return default;
            }
        }

        public MapEnum.TileTag GetTileTag () {
            if (Enum.IsDefined (typeof (MapEnum.TileTag), tileTag)) {
                return (MapEnum.TileTag)tileTag;
            } else {
                Log.PrintError ("Invalid tileTag : " + tileTag + " . Pos : (" + x + ", " + y + "). Use default.");
                return default;
            }
        }
    }
}