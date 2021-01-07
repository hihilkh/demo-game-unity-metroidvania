using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HIHIFramework.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace HIHIFramework.Asset {
    public class AssetHandler : Singleton<AssetHandler> {

        #region Check Assets Update

        /// <param name="onFinished">bool : isSuccess</param>
        public void CheckAndUpdateStreamingAssets (AssetEnum.AssetType type, Action<bool> onFinished = null) {
            var fileNameList = AssetDetails.GetPracticalStreamingAssetsFileNames (type);
            if (fileNameList == null) {
                onFinished?.Invoke (false);
                return;
            }

            CheckAndUpdateStreamingAssetsRecursive (type, fileNameList, onFinished);
        }

        private void CheckAndUpdateStreamingAssets (AssetEnum.AssetType type, string fileName, bool isNeedUnzip, Action<bool> onFinished = null) {
            Action<bool, int> onCheckFinished = (isNeedUpdate, versionToUpdate) => {
                if (isNeedUpdate) {
                    UpdateStreamingAssets (type, fileName, isNeedUnzip, versionToUpdate, onFinished);
                } else {
                    onFinished?.Invoke (true);
                }
            };

            CheckIsNeedUpdateStreamingAssets (type, fileName, onCheckFinished);
        }

        private void CheckAndUpdateStreamingAssetsRecursive (AssetEnum.AssetType type, List<string> fileNameList, Action<bool> onFinished = null) {
            if (fileNameList.Count <= 0) {
                onFinished?.Invoke (true);
                return;
            }

            Action<bool> onOneFinished = (isSuccess) => {
                if (isSuccess) {
                    fileNameList.RemoveAt (0);
                    CheckAndUpdateStreamingAssetsRecursive (type, fileNameList, onFinished);
                } else {
                    Log.PrintError ("Check and update streaming assets failed. AssetType : " + type + " , fileName : " + fileNameList[0]);
                    onFinished?.Invoke (false);
                }
            };

            var fileName = fileNameList[0];
            var isNeedUnzip = Path.GetExtension (fileName) == ".zip";
            CheckAndUpdateStreamingAssets (type, fileName, isNeedUnzip, onOneFinished);
        }
        #endregion

        #region Asset Files Update

        /// <param name="onFinished">bool : isSuccess</param>
        private void UpdateStreamingAssets (AssetEnum.AssetType type, string fileName, bool isNeedUnzip, int version, Action<bool> onFinished = null) {
            var streamingAssetsFolder = AssetFrameworkDetails.GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory.StreamingAssets, type);
            var persistentDataFolder = AssetFrameworkDetails.GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory.PersistentData, type);

            var sourcePath = Path.Combine (streamingAssetsFolder, fileName);
            var destPath = Path.Combine (persistentDataFolder, fileName);

            var copyDestPath = isNeedUnzip ? AssetFrameworkDetails.GetIOTempZipFilePath () : destPath;

            Action<bool> onCopyFinished = (isSuccess) => {
                if (isSuccess) {
                    Action<bool> onCheckChecksumFinished = (isCorrect) => {
                        if (isCorrect) {
                            if (isNeedUnzip) {
                                var destFolder = Path.GetDirectoryName (destPath);
                                FrameworkUtils.Unzip (copyDestPath, destFolder);
                            }

                            SetCurrentAssetVersion (type, fileName, version);
                        } else {
                            FrameworkUtils.DeleteFile (copyDestPath);

                            // If isNeedUnzip = false, that means the file with wrong checksum has already overridden original file. The asset version should also be overridden.
                            if (!isNeedUnzip) {
                                ClearCurrentAssetVersion (type, fileName);
                            }
                        }

                        onFinished?.Invoke (isCorrect);
                    };

                    CheckIsChecksumCorrect (type, fileName, copyDestPath, onCheckChecksumFinished);
                } else {
                    FrameworkUtils.DeleteFile (copyDestPath);
                    ClearCurrentAssetVersion (type, fileName);
                    onFinished?.Invoke (false);
                }
            };

            CopyStreamingAssets (sourcePath, copyDestPath, onCopyFinished);
        }
        #endregion

        #region Streaming Assets I/O

        /// <summary>
        /// It <b>will override</b> original file in destPath if existed
        /// </summary>
        /// <param name="onFinished">bool : isSuccess</param>
        private void CopyStreamingAssets (string sourcePath, string destPath, Action<bool> onFinished = null) {
            StartCoroutine (CopyStreamingAssetsCoroutine (sourcePath, destPath, onFinished));
        }

        /// <summary>
        /// It <b>will override</b> original file in destPath if existed
        /// </summary>
        /// <param name="onFinished">bool : isSuccess</param>
        private IEnumerator CopyStreamingAssetsCoroutine (string sourcePath, string destPath, Action<bool> onFinished = null) {
            Log.Print ("Start copying streaming assets. sourcePath : " + sourcePath + " , destPath : " + destPath);

            var folderPath = Path.GetDirectoryName (destPath);
            if (!Directory.Exists (folderPath)) {
                Directory.CreateDirectory (folderPath);
            }

            var isSuccess = false;

            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                // Notes :  No file access is available on WebGL
                // Refer to https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
                // TODO : Do WebGL logic
                throw new NotImplementedException ();
            } else if (Application.platform == RuntimePlatform.Android) {
                // Notes : For android, you cannot access streaming assets files directly.
                // Refer to https://docs.unity3d.com/Manual/StreamingAssets.html

                var unityWebRequest = UnityWebRequest.Get (sourcePath);

                yield return unityWebRequest.SendWebRequest ();
                if (string.IsNullOrEmpty (unityWebRequest.error)) {
                    var resultText = unityWebRequest.downloadHandler.text;
                    File.WriteAllText (destPath, resultText);
                    isSuccess = true;
                } else {
                    Log.PrintError ("Copy streaming assets failed. sourcePath : " + sourcePath + " , destPath : " + destPath + " , Error : " + unityWebRequest.error);
                    isSuccess = false;
                }
            } else {
                if (File.Exists (sourcePath)) {
                    File.Copy (sourcePath, destPath, true);
                    isSuccess = true;
                } else {
                    Log.PrintError ("Copy streaming assets failed. sourcePath : " + sourcePath + " , destPath : " + destPath + " , Error : File does not exist");
                    isSuccess = false;
                }
            }

            if (isSuccess) {
                Log.Print ("Finished copying streaming assets file. sourcePath : " + sourcePath + " , destPath : " + destPath);
            }

            onFinished?.Invoke (isSuccess);

        }

        /// <param name="onFinished">string[] : content by lines, string : errorMsg (<b>null</b> means no error)</param>
        private void ReadStreamingAssetsFile (string filePath, Action<string[], string> onFinished) {
            StartCoroutine (ReadStreamingAssetsFileCoroutine (filePath, onFinished));
        }

        /// <param name="onFinished">string[] : content by lines, string : errorMsg (<b>null</b> means no error)</param>
        private IEnumerator ReadStreamingAssetsFileCoroutine (string filePath, Action<string[], string> onFinished) {
            Log.Print ("Start reading streaming assets file. filePath : " + filePath);
            string[] lines = null;
            string errorMsg = null;

            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                // Notes :  No file access is available on WebGL
                // Refer to https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
                // TODO : Do WebGL logic
                throw new NotImplementedException ();
            } else if (Application.platform == RuntimePlatform.Android) {
                // Notes : For android, you cannot access streaming assets files directly.
                // Refer to https://docs.unity3d.com/Manual/StreamingAssets.html

                var unityWebRequest = UnityWebRequest.Get (filePath);
                yield return unityWebRequest.SendWebRequest ();

                if (string.IsNullOrEmpty (unityWebRequest.error)) {
                    lines = unityWebRequest.downloadHandler.text.Split (new string[] { Environment.NewLine }, StringSplitOptions.None);
                } else {
                    errorMsg = unityWebRequest.error;
                }
            } else {
                if (File.Exists (filePath)) {
                    try {
                        lines = File.ReadAllLines (filePath);
                    } catch (Exception ex) {
                        errorMsg = ex.Message;
                    }
                } else {
                    errorMsg = "File does not exist";
                }
            }

            if (errorMsg == null) {
                Log.Print ("Finished reading streaming assets file. filePath : " + filePath);
            } else {
                Log.PrintError ("Failed to read streaming assets file. filePath : " + filePath + " , error : " + errorMsg);
            }

            onFinished?.Invoke (lines, errorMsg);
        }

        #endregion

        #region Persistent Data I/O

        /// <returns>string[] : content by lines, string : errorMsg (<b>null</b> means no error)</returns>
        public (string[] lines, string errorMsg) ReadPersistentDataFileByLines (AssetEnum.AssetType type, string fileName) {
            var filePath = Path.Combine (AssetFrameworkDetails.GetAssetFolderFullPath (AssetFrameworkEnum.AssetCategory.PersistentData, type), fileName);
            Log.Print ("Start reading persistent data file. filePath : " + filePath);

            string[] lines = null;
            string errorMsg = null;

            if (File.Exists (filePath)) {
                try {
                    lines = File.ReadAllLines (filePath);
                } catch (Exception ex) {
                    errorMsg = ex.Message;
                }
            } else {
                errorMsg = "File does not exist";
            }

            if (errorMsg == null) {
                Log.Print ("Finished reading persistent data file. filePath : " + filePath);
            } else {
                Log.PrintError ("Failed to read persistent data file. filePath : " + filePath + " , error : " + errorMsg);
            }

            return (lines, errorMsg);
        }

        #endregion


        #region Version related

        /// <param name="onFinished">bool : isNeedToUpdate , int : latest version</param>
        private void CheckIsNeedUpdateStreamingAssets (AssetEnum.AssetType type, string fileName, Action<bool, int> onFinished) {
            Log.Print ("Start checking need of update streaming assets. AssetType : " + type + " , fileName : " + fileName);
            var currentVersion = GetCurrentAssetVersion (type, fileName);

            Action<int> onGetStreamingAssetsVersionFinished = (streamingAssetsVersion) => {
                Log.PrintDebug ("AssetType : " + type + " , fileName : " + fileName + " -> streamingAssetsVersion : " + streamingAssetsVersion + " , currentVersion : " + currentVersion);
                if (currentVersion < streamingAssetsVersion) {
                    Log.Print ("Finished checking need of update streaming assets : isNeed = true. AssetType : " + type + " , fileName : " + fileName);
                    onFinished?.Invoke (true, streamingAssetsVersion);
                } else {
                    Log.Print ("Finished checking need of update streaming assets : isNeed = false. AssetType : " + type + " , fileName : " + fileName);
                    onFinished?.Invoke (false, currentVersion);
                }
            };

            GetStreamingAssetsVersion (type, fileName, onGetStreamingAssetsVersionFinished);
        }

        /// <param name="onFinished">int : version no (<b>-1</b> means there is error while getting the version no)</param>
        private void GetStreamingAssetsVersion (AssetEnum.AssetType type, string fileName, Action<int> onFinished) {

            var versionFilePath = AssetFrameworkDetails.GetStreamingAssetsVersionFileFullPath (type, fileName);

            Action<string[], string> onReadFinished = (lines, errorMsg) => {
                if (!string.IsNullOrEmpty (errorMsg)) {
                    Log.PrintError ("Get streaming assets version failed. versionFilePath : " + versionFilePath + " , Error : " + errorMsg);
                    onFinished?.Invoke (-1);
                    return;
                }

                if (lines == null || lines.Length <= 0) {
                    Log.PrintError ("Get streaming assets version failed. Error : version no raw text is empty");
                    onFinished?.Invoke (-1);
                }

                var rawText = lines[0];
                int version;
                if (Int32.TryParse (rawText, out version)) {
                    onFinished?.Invoke (version);
                } else {
                    Log.PrintError ("Get streaming assets version failed. version no raw text : " + rawText + " , Error : Cannot parse raw text to int");
                    onFinished?.Invoke (-1);
                }
            };

            ReadStreamingAssetsFile (versionFilePath, onReadFinished);
        }

        /// <returns>Current asset version (<b>-1</b> means cannot find any version of this asset)</returns>
        private int GetCurrentAssetVersion (AssetEnum.AssetType type, string fileName) {
            var playerPrefsKey = AssetFrameworkDetails.GetAssetVersionPlayerPrefsKey (type, fileName);
            return PlayerPrefs.GetInt (playerPrefsKey, -1);
        }

        private void SetCurrentAssetVersion (AssetEnum.AssetType type, string fileName, int version) {
            var playerPrefsKey = AssetFrameworkDetails.GetAssetVersionPlayerPrefsKey (type, fileName);
            PlayerPrefs.SetInt (playerPrefsKey, version);
        }

        private void ClearCurrentAssetVersion (AssetEnum.AssetType type, string fileName) {
            var playerPrefsKey = AssetFrameworkDetails.GetAssetVersionPlayerPrefsKey (type, fileName);
            PlayerPrefs.DeleteKey (playerPrefsKey);
        }

        #endregion

        #region Checksum related

        /// <param name="onFinished">bool : isCorrect</param>
        private void CheckIsChecksumCorrect (AssetEnum.AssetType type, string fileName, string copiedFilePath, Action<bool> onFinished) {
            Log.Print ("Start checking copied file checksum. AssetType : " + type + " , fileName : " + fileName);
            Action<string> onReadChecksumFinished = (streamingAssetsChecksum) => {
                var copiedFileChecksum = FrameworkUtils.CalculateMD5 (copiedFilePath);

                Log.PrintDebug ("streamingAssetsChecksum : " + streamingAssetsChecksum + " , copiedFileChecksum : " + copiedFileChecksum + " . AssetType : " + type + " , fileName : " + fileName);

                var isCorrect = copiedFileChecksum == streamingAssetsChecksum;

                if (isCorrect) {
                    Log.Print ("Copied file checksum is correct. AssetType : " + type + " , fileName : " + fileName);
                } else {
                    Log.PrintError ("Copied file checksum is wrong. AssetType : " + type + " , fileName : " + fileName);
                }

                onFinished?.Invoke (isCorrect);
            };

            GetStreamingAssetsChecksum (type, fileName, onReadChecksumFinished);
        }

        /// <param name="onFinished">string : checksum value (<b>null</b> means cannot get the checksum)</param>
        private void GetStreamingAssetsChecksum (AssetEnum.AssetType type, string fileName, Action<string> onFinished) {

            var checksumFilePath = AssetFrameworkDetails.GetStreamingAssetsChecksumFileFullPath (type, fileName);

            Action<string[], string> onReadFinished = (lines, errorMsg) => {
                if (errorMsg != null) {
                    Log.PrintError ("Get streaming assets checksum failed. checksumFilePath : " + checksumFilePath + " , Error : " + errorMsg);
                    onFinished?.Invoke (null);
                    return;
                }

                if (lines == null || lines.Length <= 0) {
                    Log.PrintError ("Get streaming assets checksum failed. Error : checksum is empty");
                    onFinished?.Invoke (null);
                }

                onFinished?.Invoke (lines[0]);
            };

            ReadStreamingAssetsFile (checksumFilePath, onReadFinished);
        }

        #endregion
    }
}