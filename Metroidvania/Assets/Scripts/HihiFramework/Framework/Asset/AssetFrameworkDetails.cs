﻿using System;
using System.IO;
using HihiFramework.Core;
using UnityEngine;

namespace HihiFramework.Asset {
    public static class AssetFrameworkDetails {

        public static string GetIOTempZipFilePath () {
            var ioTempFolderPath = Path.Combine (Application.persistentDataPath, FrameworkVariable.IOTempFolderName);
            var timestampMS = FrameworkUtils.ConvertDateTimeToTimestampMS (DateTime.Now);

            return Path.Combine (ioTempFolderPath, timestampMS + ".zip");
        }

        #region AssetFolderFullPath

        public static string GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory category, AssetEnum.AssetType type) {
            var folderName = AssetDetails.GetAssetFolderName (type);

            string fullPath = null;
            switch (category) {
                case AssetFrameworkEnum.AssetCategory.StreamingAssets:
                    fullPath = Application.streamingAssetsPath;
                    break;
                case AssetFrameworkEnum.AssetCategory.PersistentData:
                    fullPath = Application.persistentDataPath;
                    break;
            }

            if (string.IsNullOrEmpty (fullPath) || string.IsNullOrEmpty (folderName)) {
                Log.PrintError ("GetAssetFolderFullPath Failed. AssetCategory : " + category + " , AssetType : " + type, LogTypes.Asset);
                return null;
            }

            return Path.Combine (fullPath, folderName);
        }

        #endregion

        #region Asset Version File Related

        public static string GetStreamingAssetsVersionFileFullPath (AssetEnum.AssetType type, string fileName) {
            var assetFolderFullPath = GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory.StreamingAssets, type);

            if (string.IsNullOrEmpty (assetFolderFullPath)) {
                Log.PrintError ("GetStreamingAssetsVersionFileFullPath Failed. AssetType : " + type, LogTypes.Asset);
                return null;
            }

            return Path.Combine (assetFolderFullPath, fileName + "." + FrameworkVariable.AssetVersionFileExtension);
        }

        public static string GetAssetVersionPlayerPrefsKey (AssetEnum.AssetType type, string fileName) {
            var folderName = AssetDetails.GetAssetFolderName (type);

            if (string.IsNullOrEmpty (folderName)) {
                Log.PrintError ("GetAssetVersionPlayerPrefsKey Failed. AssetType : " + type, LogTypes.Asset);
                return null;
            }

            return FrameworkUtils.StringReplace (FrameworkVariable.AssetVersionKey, folderName, fileName);
        }

        #endregion

        #region Asset Checksum File Related

        public static string GetStreamingAssetsChecksumFileFullPath (AssetEnum.AssetType type, string fileName) {
            var assetFolderFullPath = GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory.StreamingAssets, type);

            if (string.IsNullOrEmpty (assetFolderFullPath)) {
                Log.PrintError ("GetStreamingAssetsChecksumFileFullPath Failed. AssetType : " + type, LogTypes.Asset);
                return null;
            }

            return Path.Combine (assetFolderFullPath, fileName + "." + FrameworkVariable.AssetChecksumFileExtension);
        }

        #endregion
    }
}