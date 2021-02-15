using System;
using HihiFramework.Asset;
using HihiFramework.Core;

// Similar usage of FrameworkUtils, but project based
public partial class GameUtils : Singleton<GameUtils> {
    /// <summary>
    /// Initialize stuffs with respect to this project while the game start
    /// </summary>
    public static void InitGameSettings (Action<bool> onFinished = null) {
        AssetHandler.Instance.CheckAndUpdateStreamingAssets (AssetEnum.AssetType.MapData, onFinished);
    }

    #region Logger

    public static LogLevel GetMinLogLevel (LogTypes logType) {
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