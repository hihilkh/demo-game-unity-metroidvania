using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HIHIFramework.Core {
    public class GameUtils : Singleton<GameUtils> {

        #region Game Initialization

        public static void InitGameSettings () {
            // frame rate
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = GameVariable.TargetFrameRate;
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
    }
}