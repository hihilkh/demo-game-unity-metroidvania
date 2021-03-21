using System.Collections.Generic;
using HihiFramework.Core;

public static partial class AssetDetails {

    private const string MapDataJSONFileFormat = "map_data_{0}.json";

    private static readonly List<string> PracticalStreamingAssetsFileNames_MapData = new List<string> () {
        "map_data.zip",
    };

    public static string GetMapDataJSONFileName (int missionId) {
        return FrameworkUtils.StringReplace (MapDataJSONFileFormat, missionId.ToString ());
    }
}
