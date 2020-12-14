using System.Collections.Generic;
using HIHIFramework.Core;

public partial class AssetMapping {
    /// <summary>
    /// Get the folder name of corresponding AssetType
    /// </summary>
    public static string GetAssetFolderName (AssetEnum.AssetType type) {
        switch (type) {
            case AssetEnum.AssetType.Localization: return "Localization";
        }

        Log.PrintError ("AssetType : " + type + " has not been assigned folder name. Return null.");
        return null;
    }
}