using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public partial class GameVariable {
    #region Common

    public const bool IsBuildForDevelopment = true;

    #endregion

    #region GameConfig

    public const bool IsShowGameConfigSceneInReleaseBuild = false;
    public const int TargetFrameRate = 60;

    #endregion

    #region Logger

    public const string LogTag = "Metroidvania";
    public const LogLevel MinLogLevel = LogLevel.Info;
    public const bool IsLogForReleaseBuild = false;

    #endregion
}
