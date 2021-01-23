using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public static partial class GameVariable {
    #region Common

    public const bool IsBuildForDevelopment = true;

    #endregion

    #region GameConfig

    public const bool IsShowGameConfigSceneInReleaseBuild = false;
    public const int TargetFrameRate = 60;
    public const string FirstSceneName = LandingSceneName;

    #endregion

    #region Logger

    public const string LogTag = "Metroidvania";
    public const bool IsLogForReleaseBuild = false;
    public static LogLevel GetMinLogLevel (LogType logType) {
        //if ((logType & LogType.Enemy) == LogType.Enemy) {
        //    return LogLevel.Error;
        //}
        //if ((logType & LogType.Animation) == LogType.Animation) {
        //    return LogLevel.Debug;
        //}

        return LogLevel.Info;
    }

    #endregion
}