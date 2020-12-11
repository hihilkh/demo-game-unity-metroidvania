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
    }
}