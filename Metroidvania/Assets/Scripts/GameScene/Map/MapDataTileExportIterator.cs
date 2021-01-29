using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapDataTileExportIterator : IEnumerable<MapData.TileData> {
    private IEnumerator<MapData.TileData> enumerator;

    public MapDataTileExportIterator (Dictionary<MapEnum.TileMapType, Tilemap> tileMapDict, Vector2Int lowerBound, Vector2Int upperBound) {
        enumerator = new MapDataTileExportEnumerator (tileMapDict, lowerBound, upperBound);
    }

    public IEnumerator<MapData.TileData> GetEnumerator () { return enumerator; }
    IEnumerator IEnumerable.GetEnumerator () { return enumerator; }

    #region Enumerator

    private class MapDataTileExportEnumerator : IEnumerator<MapData.TileData> {
        private Dictionary<MapEnum.TileMapType, Tilemap> tileMapDict;
        private Vector2Int lowerBound;
        private Vector2Int upperBound;

        private int currentPosX;
        private int currentPosY;

        public MapDataTileExportEnumerator (Dictionary<MapEnum.TileMapType, Tilemap> tileMapDict, Vector2Int lowerBound, Vector2Int upperBound) {
            this.tileMapDict = tileMapDict;
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;

            Reset ();
        }

        public MapData.TileData Current { get { return GetTileData (currentPosX, currentPosY); } }
        object IEnumerator.Current { get { return Current; } }

        public void Dispose () {
            tileMapDict = null;
        }



        public bool MoveNext () {
            currentPosX++;
            if (currentPosX > upperBound.x) {
                currentPosX = lowerBound.x;
                currentPosY++;
            }

            if (currentPosY > upperBound.y) {
                return false;
            }

            return true;
        }

        public void Reset () {
            currentPosX = lowerBound.x - 1;
            currentPosY = lowerBound.y;
        }

        private MapData.TileData GetTileData (int x, int y) {
            var pos = new Vector3Int (x, y, GameVariable.TilePosZ);

            var result = new List<MapData.TileData> ();
            foreach (var pair in tileMapDict) {
                var tile = pair.Value.GetTile (pos);
                if (tile != null) {
                    var tileType = TileMapping.GetTileType (tile.name);
                    if (tileType == null) {
                        Log.PrintError ("GetTileData for export error. Cannot get tileType of " + tile.name + " . Pos : (" + x + ", " + y + ")", LogType.MapData);
                    } else {
                        result.Add (new MapData.TileData (x, y, (MapEnum.TileType)tileType, pair.Key));
                    }
                }
            }

            switch (result.Count) {
                case 0:
                    return null;
                case 1:
                    return result[0];
                default:
                    var tileMapTypesStr = "";
                    foreach (var tileData in result) {
                        tileMapTypesStr += tileData.tileMapType + "  ";
                    }

                    Log.PrintError ("GetTileData for export error. Multiple TiieData. Pos : (" + x + ", " + y + ") , TileMapTypes : " + tileMapTypesStr, LogType.MapData);
                    return null;
            }
        }
    }

    #endregion
}