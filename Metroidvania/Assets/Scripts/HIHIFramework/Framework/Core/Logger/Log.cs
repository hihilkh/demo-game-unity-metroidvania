using System;
using System.Diagnostics;
using UnityEngine;

namespace HihiFramework.Core {
    public static class Log {

        public static void PrintDebug (object obj, LogTypes logTypes = LogTypes.General) {
            Print (obj, logTypes, LogLevel.Debug);
        }

        public static void PrintDebug (string message, LogTypes logTypes = LogTypes.General) {
            Print (message, logTypes, LogLevel.Debug);
        }

        public static void PrintWarning (object obj, LogTypes logTypes = LogTypes.General) {
            Print (obj, logTypes, LogLevel.Warning);
        }

        public static void PrintWarning (string message, LogTypes logTypes = LogTypes.General) {
            Print (message, logTypes, LogLevel.Warning);
        }

        public static void PrintError (object obj, LogTypes logTypes = LogTypes.General) {
            Print (obj, logTypes, LogLevel.Error);
        }

        public static void PrintError (string message, LogTypes logTypes = LogTypes.General) {
            Print (message, logTypes, LogLevel.Error);
        }

        public static void Print (object obj, LogTypes logTypes = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            if (obj == null) {
                Print ("<null>", logTypes, logLevel);
            } else {
                Print (obj.ToString (), logTypes, logLevel);
            }
        }

        public static void Print (string message, LogTypes logType = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            if (!CheckIsPrintLog (logLevel, logType)) {
                return;
            }

            var log = ConstructLog (message);

            switch (logLevel) {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log (log);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log (log);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning (log);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError (log);
                    break;
            }
        }

        private static bool CheckIsPrintLog (LogLevel logLevel, LogTypes logType) {
            if (!GameVariable.IsLogForReleaseBuild && FrameworkUtils.GetIsReleaseBuild ()) {
                return false;
            }

            if (logLevel < GameUtils.GetMinLogLevel (logType)) {
                return false;
            }

            return true;
        }

        private static string ConstructLog (string rawMessage) {
            if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {
                return rawMessage;
            } else {
                var logTag = GameVariable.LogTag;
                var logTime = DateTime.Now.ToString ("HH:mm:ss.fff");
                var stackTraceDetails = GetStackTraceDetails ();
                return logTime + " " + logTag + " " + stackTraceDetails + "   " + rawMessage;
            }
        }

        private static string GetStackTraceDetails () {
            var stackTrace = new StackTrace (3, true);
            var frameMethod = stackTrace.GetFrame (0).GetMethod ();
            var className = frameMethod.DeclaringType.Name;
            var methodName = frameMethod.Name;
            var lineNo = stackTrace.GetFrame (0).GetFileLineNumber ();

            return className + "." + methodName + ":" + lineNo;
        }
    }
}