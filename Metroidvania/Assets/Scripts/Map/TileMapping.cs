using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class TileMapping {
    private static Dictionary<MapEnum.TileType, string> TileResourcesNameDict = new Dictionary<MapEnum.TileType, string> () {
        { MapEnum.TileType.Grass, "Tiles/Grass" }
    };

    public static string GetTileResourcesName (MapEnum.TileType tileType) {
        if (TileResourcesNameDict.ContainsKey(tileType)) {
            return TileResourcesNameDict[tileType];
        } else {
            Log.PrintError ("No matched tile resources name for tileType : " + tileType);
            return null;
        }
    }
}
