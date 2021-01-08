using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class TileMapping {
    private const string ResourcesFolderName = "Tiles/";

    // Remarks : Only File name
    private static Dictionary<MapEnum.TileType, string> TileResourcesNameDict = new Dictionary<MapEnum.TileType, string> () {
        { MapEnum.TileType.Dirt, "Dirt" },
        { MapEnum.TileType.GroundTag, "GroundTag" },
        { MapEnum.TileType.WallTag, "WallTag" },
        { MapEnum.TileType.RoofTag, "RoofTag" },
        { MapEnum.TileType.SlippyWallTag, "SlippyWallTag" },
        { MapEnum.TileType.DeathTag, "DeathTag" }
    };

    public static string GetTileResourcesName (MapEnum.TileType tileType) {
        if (TileResourcesNameDict.ContainsKey(tileType)) {
            return ResourcesFolderName + TileResourcesNameDict[tileType];
        } else {
            Log.PrintError ("No matched tile resources name for tileType : " + tileType);
            return null;
        }
    }

    public static MapEnum.TileType? GetTileType (string resourcesName) {
        foreach (var pair in TileResourcesNameDict) {
            if (pair.Value == resourcesName) {
                return pair.Key;
            }
        }

        Log.PrintError ("Cannot get TileType by resourcesName : " + resourcesName);
        return null;
    }
}
