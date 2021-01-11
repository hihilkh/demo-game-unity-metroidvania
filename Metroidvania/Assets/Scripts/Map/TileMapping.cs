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
        { MapEnum.TileType.Ground2Tag, "Ground2Tag" },
        { MapEnum.TileType.SlippyWallTag, "SlippyWallTag" },
        { MapEnum.TileType.DeathTag, "DeathTag" },
    };

    private static Dictionary<MapEnum.TileMapType, MapEnum.TileType> MapDesignTileTypeDict = new Dictionary<MapEnum.TileMapType, MapEnum.TileType> () {
        { MapEnum.TileMapType.Ground, MapEnum.TileType.GroundTag },
        { MapEnum.TileMapType.Ground2, MapEnum.TileType.Ground2Tag },
        { MapEnum.TileMapType.SlippyWall, MapEnum.TileType.SlippyWallTag },
        { MapEnum.TileMapType.Death, MapEnum.TileType.DeathTag },
    };

    public static string GetTileResourcesName (MapEnum.TileType tileType) {
        if (TileResourcesNameDict.ContainsKey (tileType)) {
            return ResourcesFolderName + TileResourcesNameDict[tileType];
        } else {
            Log.PrintError ("No matched tile resources name for tileType : " + tileType, LogType.MapData);
            return null;
        }
    }

    public static MapEnum.TileType? GetTileType (string resourcesName) {
        foreach (var pair in TileResourcesNameDict) {
            if (pair.Value == resourcesName) {
                return pair.Key;
            }
        }

        Log.PrintError ("Cannot get TileType. resourcesName : " + resourcesName, LogType.MapData);
        return null;
    }

    public static MapEnum.TileType? GetMapDesignTileType (MapEnum.TileMapType tileMapType) {
        if (MapDesignTileTypeDict.ContainsKey (tileMapType)) {
            return MapDesignTileTypeDict[tileMapType];
        }

        Log.PrintError ("MapDesignTileType mapping is not yet assigned. tileMapType : " + tileMapType, LogType.MapData);
        return null;
    }
}
