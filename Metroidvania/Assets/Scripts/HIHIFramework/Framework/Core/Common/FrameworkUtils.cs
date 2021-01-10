﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace HIHIFramework.Core {
    public class FrameworkUtils : Singleton<FrameworkUtils> {

        private static bool IsGameSettingsInitialized = false;
        #region Game Initialization

        public static void InitGameSettings (Action<bool> onFinished = null) {
            if (IsGameSettingsInitialized) {
                onFinished?.Invoke (true);
                return;
            }

            IsGameSettingsInitialized = true;

            // frame rate
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GameVariable.TargetFrameRate;

            Action<bool> onLangManagerInitFinished = (isSuccess) => {
                GameUtils.InitGameSettings (onFinished);
            };

            // LangManager
            LangManager.Init (onLangManagerInitFinished);
        }

        [RuntimeInitializeOnLoadMethod]
        private static void OnRuntimeInitializeLoad () {
            // For convenient of development, which start at a random scene
            InitGameSettings ();
        }

        #endregion

        #region Common

        public static bool GetIsReleaseBuild () {
            return !(Debug.isDebugBuild || GameVariable.IsBuildForDevelopment);
        }

        /// <returns>Timestamp in second</returns>
        public static long ConvertDateTimeToTimestamp (DateTime dateTime) {
            return ConvertDateTimeToTimestampMS (dateTime) / 1000;
        }

        /// <returns>Timestamp in millisecond</returns>
        public static long ConvertDateTimeToTimestampMS (DateTime dateTime) {
            return (dateTime.ToUniversalTime ().Ticks - 621355968000000000) / 10000;
        }

        public static string StringReplace (string stringBase, params string[] replaceStrings) {
            try {
                return String.Format (stringBase, replaceStrings);
            } catch (Exception ex) {
                Log.PrintError (ex.Message, LogType.General);
            }

            return stringBase;
        }

        public static string GetProgressPercentStr (float progress, bool isAllowOverOne = false, bool isAllowNegative = false) {
            var progressPercent = progress * 100;
            var progressPercentInt = Mathf.FloorToInt (progressPercent);

            if (progressPercentInt > 100) {
                if (!isAllowOverOne) {
                    progressPercentInt = 100;
                }
            } else if (progressPercentInt < 0) {
                if (!isAllowNegative) {
                    progressPercentInt = 0;
                }
            }

            return StringReplace (FrameworkVariable.ProgressPercentFormat, progressPercentInt.ToString ());
        }

        /// <summary>
        /// Used by non MonoBehaviour scripts. Call coroutine to wait until <paramref name="predicate"/> return <b>true</b> and trigger <paramref name="action"/>.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="action"></param>
        public void WaitUntil (Func<bool> predicate, Action action) {
            StartCoroutine (WaitUntilCoroutine (predicate, action));
        }

        private IEnumerator WaitUntilCoroutine (Func<bool> predicate, Action action) {
            yield return new WaitUntil (predicate);

            action?.Invoke ();
        }

        #endregion

        #region Parent/Child GameObject

        public static void InsertChildrenToParent (Transform parentTransform, List<Transform> childrenList, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) {
            // startSiblingIndex < 0 means insert at last
            var isInsertAtLast = startSiblingIndex < 0 || startSiblingIndex >= parentTransform.childCount;
            var currentSiblingIndex = startSiblingIndex;

            var localScale = Vector3.one;
            if (childrenList.Count > 0) {
                localScale = childrenList[0].localScale;
            }

            for (var i = 0; i < childrenList.Count; i++) {
                var child = childrenList[i];
                child.SetParent (parentTransform, isWorldPositionStay);
                child.localScale = localScale;
                if (isZPosZero) {
                    child.localPosition = new Vector3 (child.localPosition.x, child.localPosition.y, 0);
                }

                if (!isInsertAtLast) {
                    child.SetSiblingIndex (currentSiblingIndex);
                    currentSiblingIndex++;
                }
            }
        }

        public static void InsertChildrenToParent (Transform parentTransform, List<GameObject> childrenList, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) {
            var childrenTransformList = new List<Transform> ();

            foreach (var child in childrenList) {
                childrenTransformList.Add (child.transform);
            }

            InsertChildrenToParent (parentTransform, childrenTransformList, isZPosZero, startSiblingIndex, isWorldPositionStay);
        }

        public static void InsertChildrenToParent<T> (Transform parentTransform, List<T> childrenList, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) where T : MonoBehaviour {
            var childrenTransformList = new List<Transform> ();

            foreach (var child in childrenList) {
                childrenTransformList.Add (child.transform);
            }

            InsertChildrenToParent (parentTransform, childrenTransformList, isZPosZero, startSiblingIndex, isWorldPositionStay);
        }

        public static void InsertChildrenToParent (Transform parentTransform, Transform child, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) {
            var childrenTransformList = new List<Transform> ();

            childrenTransformList.Add (child);

            InsertChildrenToParent (parentTransform, childrenTransformList, isZPosZero, startSiblingIndex, isWorldPositionStay);
        }

        public static void InsertChildrenToParent (Transform parentTransform, GameObject child, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) {
            var childrenTransformList = new List<Transform> ();

            childrenTransformList.Add (child.transform);

            InsertChildrenToParent (parentTransform, childrenTransformList, isZPosZero, startSiblingIndex, isWorldPositionStay);
        }

        public static void InsertChildrenToParent<T> (Transform parentTransform, T child, bool isZPosZero = false, int startSiblingIndex = -1, bool isWorldPositionStay = true) where T : MonoBehaviour {
            var childrenTransformList = new List<Transform> ();

            childrenTransformList.Add (child.transform);

            InsertChildrenToParent (parentTransform, childrenTransformList, isZPosZero, startSiblingIndex, isWorldPositionStay);
        }

        public static void DestroyChildren (Transform parentTransform) {
            // Notes : Destroy method seems may have some delay, so first move the child out of the parent to ensure no error by the delay
            var childrenTransformToDestroy = new List<Transform> ();
            foreach (Transform childTransform in parentTransform.transform) {
                childrenTransformToDestroy.Add (childTransform);
            }

            foreach (var childTransform in childrenTransformToDestroy) {
                childTransform.SetParent (null);
                Destroy (childTransform.gameObject);
            }

            Resources.UnloadUnusedAssets ();
        }

        #endregion

        #region I/O

        #region Cryptography

        public static string CalculateMD5 (string fileName) {
            using (var md5 = MD5.Create ()) {
                using (var stream = File.OpenRead (fileName)) {
                    var hash = md5.ComputeHash (stream);
                    var value = BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
                    Log.PrintDebug ("MD5 = " + value + " . file : " + fileName, LogType.IO);
                    return value;
                }
            }
        }

        #endregion

        /// <summary>
        /// This method does <b>not</b> work with getting files under folders of <b>streaming assets on android or WebGL platform</b>
        /// </summary>
        public static List<string> GetAllFilePaths (string folderPath, string extension = "*", FrameworkEnum.FilePathType filePathType = FrameworkEnum.FilePathType.FullPath, bool isRecursive = false) {
            var filePaths = new string[0];
            try {
                filePaths = Directory.GetFiles (folderPath, "*." + extension, isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            } catch (Exception ex) {
                Log.PrintWarning (ex.Message, LogType.IO);
            }

            var resultList = new List<string> ();

            switch (filePathType) {
                case FrameworkEnum.FilePathType.FullPath:
                    resultList.AddRange (filePaths);
                    break;
                case FrameworkEnum.FilePathType.FileNameOnly:
                    foreach (var path in filePaths) {
                        resultList.Add (Path.GetFileName (path));
                    }
                    break;
                case FrameworkEnum.FilePathType.FileNameOnlyWithoutExtension:
                    foreach (var path in filePaths) {
                        resultList.Add (Path.GetFileNameWithoutExtension (path));
                    }
                    break;
            }

            return resultList;
        }

        /// <summary>
        /// It <b>will override</b> original file in destFolder if existed
        /// </summary>
        public static void Unzip (string zipFilePath, string destFolder, bool isDeleteOriginalZipFile = true) {
            Log.Print ("Start unzip file. zipFilePath : " + zipFilePath + " , exportPath : " + destFolder, LogType.IO);
            ZipUtils.Unzip (zipFilePath, destFolder);
            Log.Print ("Finish unzip file. zipFilePath : " + zipFilePath + " , exportPath : " + destFolder, LogType.IO);

            if (isDeleteOriginalZipFile) {
                DeleteFile (zipFilePath);
            }
        }

        public static void DeleteFile (string filePath) {
            Log.Print ("Start delete file. filePath : " + filePath, LogType.IO);
            if (!File.Exists (filePath)) {
                Log.Print ("File do not exist. No need to delete. filePath : " + filePath, LogType.IO);
                return;
            }
            File.Delete (filePath);
            Log.Print ("Finish delete file. filePath : " + filePath, LogType.IO);
        }

        /// <summary>
        /// Create a file in <paramref name="fullFilePath"/> with contents <paramref name="lines"/>.
        /// If <paramref name="isOverwrite"/> = false, it will return false if file already exists.
        /// <br></br><b>Currently only available for UnityEditor.</b>
        /// </summary>
        /// <returns>isSuccess</returns>
        public static bool CreateFile (string fullFilePath, bool isOverwrite, params string[] lines) {
            // TODO : Implement logic for other platform
#if UNITY_EDITOR
            var destFolder = Path.GetDirectoryName (fullFilePath);

            if (!string.IsNullOrEmpty (destFolder)) {
                if (!Directory.Exists (destFolder)) {
                    Directory.CreateDirectory (destFolder);
                }
            }

            if (File.Exists (fullFilePath) && !isOverwrite) {
                Log.PrintWarning ("File already exists. Do not overwrite. Path : " + fullFilePath, LogType.IO);
                return false;
            } else {
                try {
                    File.WriteAllLines (fullFilePath, lines);
                    return true;
                } catch (Exception ex) {
                    Log.PrintError ("Error occur : " + ex.Message, LogType.IO);
                    return false;
                }
            }
#else
            return false;
#endif
        }

        #endregion
    }
}