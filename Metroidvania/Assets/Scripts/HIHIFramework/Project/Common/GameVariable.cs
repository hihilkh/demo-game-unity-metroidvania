using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public partial class GameVariable {

    #region Tag

    public const string GroundTag = "Ground";
    public const string WallTag = "Wall";

    #endregion

    #region Scene

    public const string LandingSceneName = "Landing";
    public const string MainMenuSceneName = "MainMenu";
    public const string GameSceneName = "Game";

    #endregion

    #region Player Pref Key

    // GameConfig
    public const string BaseURLKey = "BASE_URL";
    public const string AnalyticsTypeKey = "ANALYTICS_TYPE";

    // Tutorial
    public const string HasDoneOpeningTutorialKey = "HAS_DONE_OPENING_TUTORIAL";

    #endregion
}
