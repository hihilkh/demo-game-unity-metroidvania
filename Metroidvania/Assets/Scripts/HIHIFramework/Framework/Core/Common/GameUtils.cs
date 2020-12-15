using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace HIHIFramework.Core {
    public class GameUtils : Singleton<GameUtils> {

        #region Game Initialization

        public static void InitGameSettings (Action<bool> onFinished) {
            // frame rate
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GameVariable.TargetFrameRate;
            LangManager.Init (onFinished);
        }

        #endregion

        #region Common

        public static bool GetIsReleaseBuild () {
            return !(Debug.isDebugBuild || GameVariable.IsBuildForDevelopment);
        }

        public static long ConvertDateTimeToTimestamp (DateTime dateTime) {
            return ConvertDateTimeToTimestampMS (dateTime) / 1000;
        }

        public static long ConvertDateTimeToTimestampMS (DateTime dateTime) {
            return (dateTime.ToUniversalTime ().Ticks - 621355968000000000) / 10000;
        }

        public static string StringReplace (string stringBase, params string[] replaceStrings) {
            try {
                return String.Format (stringBase, replaceStrings);
            } catch (Exception ex) {
                Log.PrintError (ex.Message);
            }

            return stringBase;
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

        #region Cryptography

        public static string CalculateMD5 (string fileName) {
            using (var md5 = MD5.Create ()) {
                using (var stream = File.OpenRead (fileName)) {
                    var hash = md5.ComputeHash (stream);
                    return BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
                }
            }
        }

        #endregion

        #region I/O

        /// <summary>
        /// This method does <b>not</b> work with getting files under folders of <b>streaming assets on android or WebGL platform</b>
        /// </summary>
        public static List<string> GetAllFilePaths (string folderPath, string extension = "*", FrameworkEnum.FilePathType filePathType = FrameworkEnum.FilePathType.FullPath, bool isRecursive = false) {
            var filePaths = new string[0];
            try {
                filePaths = Directory.GetFiles (folderPath, "*." + extension, isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            } catch (Exception ex) {
                Log.PrintWarning (ex.Message);
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
            Log.Print ("Start unzip file. zipFilePath : " + zipFilePath + " , exportPath : " + destFolder);
            ZipUtils.Unzip (zipFilePath, destFolder);
            Log.Print ("Finish unzip file. zipFilePath : " + zipFilePath + " , exportPath : " + destFolder);

            if (isDeleteOriginalZipFile) {
                DeleteFile (zipFilePath);
            }
        }

        public static void DeleteFile (string filePath) {
            Log.Print ("Start delete file. filePath : " + filePath);
            if (!File.Exists (filePath)) {
                Log.Print ("File do not exist. No need to delete. filePath : " + filePath);
                return;
            }
            File.Delete (filePath);
            Log.Print ("Finish delete file. filePath : " + filePath);
        }

        #endregion
    }
}