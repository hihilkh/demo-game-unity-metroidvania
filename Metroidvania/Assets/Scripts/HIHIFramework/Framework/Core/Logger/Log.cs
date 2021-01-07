using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HIHIFramework.Core {
    public static class Log {

        public static void PrintDebug (object obj) {
            Print (obj, LogLevel.Debug);
        }

        public static void PrintDebug (string message) {
            Print (message, LogLevel.Debug);
        }

        public static void PrintWarning (object obj) {
            Print (obj, LogLevel.Warning);
        }

        public static void PrintWarning (string message) {
            Print (message, LogLevel.Warning);
        }

        public static void PrintError (object obj) {
            Print (obj, LogLevel.Error);
        }

        public static void PrintError (string message) {
            Print (message, LogLevel.Error);
        }

        public static void Print (object obj, LogLevel logLevel = LogLevel.Info) {
            if (obj == null) {
                Print ("<null>", logLevel);
            } else {
                Print (obj.ToString (), logLevel);
            }
        }

        public static void Print (string message, LogLevel logLevel = LogLevel.Info) {
            if (!CheckIsPrintLog (logLevel)) {
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

        private static bool CheckIsPrintLog (LogLevel logLevel) {
            if (logLevel < GameVariable.MinLogLevel) {
                return false;
            }

            if (!GameVariable.IsLogForReleaseBuild && FrameworkUtils.GetIsReleaseBuild ()) {
                return false;
            }

            return true;
        }

        private static string ConstructLog (string rawMessage) {
            if (UnityEngine.Debug.isDebugBuild) {
                return rawMessage;
            }

            var logTag = GameVariable.LogTag;
            var logTime = DateTime.Now.ToString ("HH:mm:ss.fff");
            var stackTraceDetails = GetStackTraceDetails ();
            return logTime + " " + logTag + " " + stackTraceDetails + "   " + rawMessage;
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