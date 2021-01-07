using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HIHIFramework.Asset;
using HIHIFramework.Core;
using UnityEngine;

// Similar usage of FrameworkUtils, but project based
public partial class GameUtils : Singleton<GameUtils> {
    /// <summary>
    /// Initialize stuffs with respect to this project while the game start
    /// </summary>
    public static void InitGameSettings (Action<bool> onFinished = null) {
        AssetHandler.Instance.CheckAndUpdateStreamingAssets (AssetEnum.AssetType.MapData, onFinished);
    }
}