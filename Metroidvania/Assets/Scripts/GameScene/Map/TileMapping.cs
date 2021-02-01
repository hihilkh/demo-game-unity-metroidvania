using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using UnityEngine;

public static class TileMapping {
    private const string ResourcesFolderName = "Tiles/";

    // Remarks : Only File name
    private static readonly Dictionary<MapEnum.TileType, string> TileResourcesNameDict = new Dictionary<MapEnum.TileType, string> () {
        { MapEnum.TileType.Soil, "Soil" },

        { MapEnum.TileType.Ground_Top, "Ground_Top" },
        { MapEnum.TileType.Ground_Bottom, "Ground_Bottom" },
        { MapEnum.TileType.Ground_Left, "Ground_Left" },
        { MapEnum.TileType.Ground_Right, "Ground_Right" },
        { MapEnum.TileType.Ground_OuterTopLeft, "Ground_OuterTopLeft" },
        { MapEnum.TileType.Ground_OuterTopRight, "Ground_OuterTopRight" },
        { MapEnum.TileType.Ground_OuterBottomLeft, "Ground_OuterBottomLeft" },
        { MapEnum.TileType.Ground_OuterBottomRight, "Ground_OuterBottomRight" },
        { MapEnum.TileType.Ground_InnerTopLeft, "Ground_InnerTopLeft" },
        { MapEnum.TileType.Ground_InnerTopRight, "Ground_InnerTopRight" },
        { MapEnum.TileType.Ground_InnerBottomLeft, "Ground_InnerBottomLeft" },
        { MapEnum.TileType.Ground_InnerBottomRight, "Ground_InnerBottomRight" },
        { MapEnum.TileType.Ground_Monotone, "Ground_Monotone" },
        { MapEnum.TileType.Ground_Single, "Ground_Single" },

        { MapEnum.TileType.Slippy_Top, "Slippy_Top" },
        { MapEnum.TileType.Slippy_Bottom, "Slippy_Bottom" },
        { MapEnum.TileType.Slippy_Left, "Slippy_Left" },
        { MapEnum.TileType.Slippy_Right, "Slippy_Right" },
        { MapEnum.TileType.Slippy_OuterTopLeft, "Slippy_OuterTopLeft" },
        { MapEnum.TileType.Slippy_OuterTopRight, "Slippy_OuterTopRight" },
        { MapEnum.TileType.Slippy_OuterBottomLeft, "Slippy_OuterBottomLeft" },
        { MapEnum.TileType.Slippy_OuterBottomRight, "Slippy_OuterBottomRight" },
        { MapEnum.TileType.Slippy_InnerTopLeft, "Slippy_InnerTopLeft" },
        { MapEnum.TileType.Slippy_InnerTopRight, "Slippy_InnerTopRight" },
        { MapEnum.TileType.Slippy_InnerBottomLeft, "Slippy_InnerBottomLeft" },
        { MapEnum.TileType.Slippy_InnerBottomRight, "Slippy_InnerBottomRight" },
        { MapEnum.TileType.Slippy_FromTopGround, "Slippy_FromTopGround" },
        { MapEnum.TileType.Slippy_FromBottomGround, "Slippy_FromBottomGround" },
        { MapEnum.TileType.Slippy_FromLeftGround, "Slippy_FromLeftGround" },
        { MapEnum.TileType.Slippy_FromRightGround, "Slippy_FromRightGround" },

        { MapEnum.TileType.Stick_Vert_1, "Stick_Vert_1" },
        { MapEnum.TileType.Stick_Vert_2, "Stick_Vert_2" },
        { MapEnum.TileType.Stick_Hori_1, "Stick_Hori_1" },
        { MapEnum.TileType.Stick_Hori_2, "Stick_Hori_2" },
        { MapEnum.TileType.Billboard_Left_1, "Billboard_Left_1" },
        { MapEnum.TileType.Billboard_Left_2, "Billboard_Left_2" },
        { MapEnum.TileType.Billboard_Right_1, "Billboard_Right_1" },
        { MapEnum.TileType.Billboard_Right_2, "Billboard_Right_2" },
        { MapEnum.TileType.Billboard_TopLeft_1, "Billboard_TopLeft_1" },
        { MapEnum.TileType.Billboard_TopLeft_2, "Billboard_TopLeft_2" },
        { MapEnum.TileType.Billboard_Up_1, "Billboard_Up_1" },
        { MapEnum.TileType.Billboard_Up_2, "Billboard_Up_2" },

        { MapEnum.TileType.Sting_Top, "Sting_Top" },
        { MapEnum.TileType.Sting_Bottom, "Sting_Bottom" },
        { MapEnum.TileType.Sting_Left, "Sting_Left" },
        { MapEnum.TileType.Sting_Right, "Sting_Right" },

        { MapEnum.TileType.Switch_Off_1, "Switch_Off_1" },
        { MapEnum.TileType.Switch_Off_2, "Switch_Off_2" },
        { MapEnum.TileType.Switch_Off_3, "Switch_Off_3" },
        { MapEnum.TileType.Switch_Off_4, "Switch_Off_4" },

        { MapEnum.TileType.Switch_On_1, "Switch_On_1" },
        { MapEnum.TileType.Switch_On_2, "Switch_On_2" },
        { MapEnum.TileType.Switch_On_3, "Switch_On_3" },
        { MapEnum.TileType.Switch_On_4, "Switch_On_4" },

        { MapEnum.TileType.Target, "Target" },

        { MapEnum.TileType.Grass, "Grass" },

        { MapEnum.TileType.Torch_1, "Torch_1" },
        { MapEnum.TileType.Torch_2, "Torch_2" },
        { MapEnum.TileType.Torch_3, "Torch_3" },
        { MapEnum.TileType.Torch_4, "Torch_4" },

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

    public static bool CheckIsMapDesignTileType (MapEnum.TileType tileType) {
        foreach (var pair in MapDesignTileTypeDict) {
            if (pair.Value == tileType) {
                return true;
            }
        }

        return false;
    }
}
