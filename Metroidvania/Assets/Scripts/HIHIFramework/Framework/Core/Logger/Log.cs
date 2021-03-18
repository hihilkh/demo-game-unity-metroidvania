using System;
using System.Diagnostics;
using UnityEngine;

namespace HihiFramework.Core {
    public static class Log {

        public static void PrintDebug (object obj, LogTypes logTypes = LogTypes.General) {
            Print (2, obj, logTypes, LogLevel.Debug);
        }

        public static void PrintDebug (string message, LogTypes logTypes = LogTypes.General) {
            Print (2, message, logTypes, LogLevel.Debug);
        }

        public static void Print (object obj, LogTypes logTypes = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            Print (2, obj, logTypes, logLevel);
        }
        public static void Print (string message, LogTypes logTypes = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            Print (2, message, logTypes, logLevel);
        }

        public static void PrintWarning (object obj, LogTypes logTypes = LogTypes.General) {
            Print (2, obj, logTypes, LogLevel.Warning);
        }

        public static void PrintWarning (string message, LogTypes logTypes = LogTypes.General) {
            Print (2, message, logTypes, LogLevel.Warning);
        }

        public static void PrintError (object obj, LogTypes logTypes = LogTypes.General) {
            Print (2, obj, logTypes, LogLevel.Error);
        }

        public static void PrintError (string message, LogTypes logTypes = LogTypes.General) {
            Print (2, message, logTypes, LogLevel.Error);
        }

        private static void Print (int stackTraceCount, object obj, LogTypes logTypes = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            if (obj == null) {
                Print (stackTraceCount + 1, "<null>", logTypes, logLevel);
            } else {
                Print (stackTraceCount + 1, obj.ToString (), logTypes, logLevel);
            }
        }

        private static void Print (int stackTraceCount, string message, LogTypes logTypes = LogTypes.General, LogLevel logLevel = LogLevel.Info) {
            if (!CheckIsPrintLog (logLevel, logTypes)) {
                return;
            }

            var log = ConstructLog (message, stackTraceCount + 1);

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

        private static string ConstructLog (string rawMessage, int stackTraceCount) {
            if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) {
                return rawMessage;
            } else {
                var logTag = GameVariable.LogTag;
                var logTime = DateTime.Now.ToString ("HH:mm:ss.fff");
                var stackTraceDetails = GetStackTraceDetails (stackTraceCount + 1);
                return logTime + " " + logTag + " " + stackTraceDetails + "   " + rawMessage;
            }
        }

        private static string GetStackTraceDetails (int stackTraceCount) {
            var stackTrace = new StackTrace (stackTraceCount, true);
            var frameMethod = stackTrace.GetFrame (0).GetMethod ();
            var className = frameMethod.DeclaringType.Name;
            var methodName = frameMethod.Name;
            var lineNo = stackTrace.GetFrame (0).GetFileLineNumber ();

            return className + "." + methodName + ":" + lineNo;
        }
    }
}