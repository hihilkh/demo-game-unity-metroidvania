using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {
    [SerializeField] private Tilemap groundTileMap;
    [SerializeField] private Tilemap wallTileMap;

    private void Start () {
        var json = "{\"tiles\":[{\"x\":0,\"y\":-1,\"tileType\":0,\"tileTag\":0},{\"x\":-1,\"y\":-1,\"tileType\":0,\"tileTag\":0},{\"x\":1,\"y\":-1,\"tileType\":0,\"tileTag\":0}]}";
        var mapData = JsonUtility.FromJson<MapData> (json);
        GenerateMap (mapData);
    }

    public void GenerateMap (MapData mapData) {
        if (mapData == null || mapData.tiles == null) {
            Log.PrintWarning ("mapData is null. Please check.");
            return;
        }

        foreach (var tileData in mapData.tiles) {
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