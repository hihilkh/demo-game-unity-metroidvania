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

            foreach (var pair in tileMapDict) {
                var tile = pair.Value.GetTile (pos);
                if (tile != null) {
                    var tileType = TileMapping.GetTileType (tile.name);
                    if (tileType == null) {
                        Log.PrintError ("GetTileData for export failed. Pos : (" + x + ", " + y + ")", LogType.MapData);
                        return null;
                    } else {
                        return new MapData.TileData (x, y, (MapEnum.TileType)tileType, pair.Key);
                    }
                }
            }

            return null;
        }
    }

    #endregion
}