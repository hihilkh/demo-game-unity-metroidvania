using System;
using System.Collections.Generic;
using HIHIFramework.Core;

public static partial class AssetDetails {
    /// <summary>
    /// Get the folder name of corresponding AssetType
    /// </summary>
    public static string GetAssetFolderName (AssetEnum.AssetType type) {
        switch (type) {
            case AssetEnum.AssetType.Localization: return "Localization";
            case AssetEnum.AssetType.MapData: return "MapData";
        }

        Log.PrintError ("AssetType : " + type + " has not been assigned folder name. Return null.", LogType.Asset);
        return null;
    }

    /// <summary>
    /// Get all "practical" streaming assets file name (with extension) of corresponding AssetType.
    /// "Practical" means need to be copied to persistant data to use.
    /// Every "practical" asset should come together with its version file and checksum file.
    /// </summary>
    public static List<string> GetPracticalStreamingAssetsFileNames (AssetEnum.AssetType type) {
        switch (type) {
            case AssetEnum.AssetType.Localization:
                var nameList = new List<string> ();
                foreach (LangType langType in Enum.GetValues (typeof (LangType))) {
                    var fileName = LangConfig.GetLocalizationFileName (langType);
                    if (!string.IsNullOrEmpty (fileName)) {
                        nameList.Add (fileName);
                    }
                }
                return nameList;
            case AssetEnum.AssetType.MapData:
                return PracticalStreamingAssetsFileNames_MapData;
        }

        Log.PrintError ("AssetType : " + type + " has not been assigned practical streaming assets file names. Return null.", LogType.Asset);
        return null;
    }
}