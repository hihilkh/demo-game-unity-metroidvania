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

    public static LogLevel GetMinLogLevel (LogTypes logTypes) {
        //if ((logTypes & LogTypes.Char) == LogTypes.Char) {
        //    return LogLevel.Debug;
        //} else {
        //    return LogLevel.Error;
        //}

        return LogLevel.Info;
    }

    #endregion
}