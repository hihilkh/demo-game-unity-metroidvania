using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class TileMapping {
    private const string ResourcesFolderName = "Tiles/";

    // Remarks : Only File name
    private static readonly Dictionary<MapEnum.TileType, string> TileResourcesCustomNameDict = new Dictionary<MapEnum.TileType, string> () {
        // TODO : Change TileTag to upper tile
        { MapEnum.TileType.GroundTag, "TileTag/GroundTag" },
        { MapEnum.TileType.Ground2Tag, "TileTag/Ground2Tag" },
        { MapEnum.TileType.SlippyWallTag, "TileTag/SlippyWallTag" },
        { MapEnum.TileType.DeathTag, "TileTag/DeathTag" },
    };

    private static readonly Dictionary<MapEnum.TileMapType, MapEnum.TileType> MapDesignTileTypeDict = new Dictionary<MapEnum.TileMapType, MapEnum.TileType> () {
        { MapEnum.TileMapType.Ground, MapEnum.TileType.GroundTag },
        { MapEnum.TileMapType.Ground2, MapEnum.TileType.Ground2Tag },
        { MapEnum.TileMapType.SlippyWall, MapEnum.TileType.SlippyWallTag },
        { MapEnum.TileMapType.Death, MapEnum.TileType.DeathTag },
    };

    public static string GetTileResourcesName (MapEnum.TileType tileType) {
        if (TileResourcesCustomNameDict.ContainsKey (tileType)) {
            return ResourcesFolderName + TileResourcesCustomNameDict[tileType];
        } else {
            // Assume use Enum value as file name
            return ResourcesFolderName + tileType.ToString ();
        }
    }

    public static MapEnum.TileType? GetTileType (string resourcesName) {
        foreach (var pair in TileResourcesCustomNameDict) {
            if (pair.Value == resourcesName) {
                return pair.Key;
            }
        }

        MapEnum.TileType tileType;
        if (!FrameworkUtils.TryParseToEnumByName (resourcesName, out tileType)) {
            Log.PrintError ("Cannot get TileType. resourcesName : " + resourcesName, LogType.MapData);
            return null;
        }

        return tileType;
    }

    public static MapEnum.TileType? GetMapDesignTileType (MapEnum.TileMapType tileMapType) {
        if (MapDesignTileTypeDict.ContainsKey (tileMapType)) {
            return MapDesignTileTypeDict[tileMapType];
        }

        Log.PrintError ("MapDesignTileType mapping is not yet assigned. tileMapType : " + tileMapType, LogType.MapData);
        return null;
    }

    public static bool CheckIsMapDesignTileType (MapEnum.TileType tileType) {
        foreach (var pair in MapDesignTileTypeDict) {
            if (pair.Value == tileType) {
                return true;
            }
        }

        return false;
    }
}
