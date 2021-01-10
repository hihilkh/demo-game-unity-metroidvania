using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap wallTileMap;

    public void GenerateMap (List<MapData.TileData> tileDataList) {
        if (tileDataList == null) {
            Log.PrintWarning ("mapData is null. Please check.", LogType.MapData);
            return;
        }

        foreach (var tileData in tileDataList) {
            var tileType = tileData.GetTileType ();
            var resourcesName = TileMapping.GetTileResourcesName (tileType);
            if (resourcesName != null) {
                var tile = Resources.Load<Tile> (resourcesName);
                var targetTilemap = groundTileMap;
                switch (tileData.GetTileTag ()) {
                    case MapEnum.TileTag.Ground:
                        targetTilemap = groundTileMap;
                        break;
                    case MapEnum.TileTag.Wall:
                        targetTilemap = wallTileMap;
                        break;
                }
                targetTilemap.SetTile (tileData.GetPos (), tile);
            }
        }
    }
}