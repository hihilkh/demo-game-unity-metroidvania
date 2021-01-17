using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public static partial class GameVariable {

    #region Tag

    public const string PlayerTag = "Player";
    public const string EnemyTag = "Enemy";
    public const string GroundTag = "Ground";
    public const string SlippyWallTag = "SlippyWall";
    public const string DeathTag = "Death";
    public const string AttackTag = "Attack";

    #endregion

    #region Scene

    public const string LandingSceneName = "Landing";
    public const string MainMenuSceneName = "MainMenu";
    public const string GameSceneName = "Game";
    public const string MapEditorSceneName = "MapEditor";

    #endregion

    #region Map Related

    public const string CharPrefabResourcesName = "Char/Character";

    public const int CharPosZ = 0;
    public const int TilePosZ = 0;

    #endregion

    #region Player Pref Key

    // GameConfig
    public const string BaseURLKey = "BASE_URL";
    public const string AnalyticsTypeKey = "ANALYTICS_TYPE";

    // Tutorial
    public const string HasDoneOpeningTutorialKey = "HAS_DONE_OPENING_TUTORIAL";

    // Game Progress
    public const string AllMissionProgressKey = "ALL_MISSION_PROGRESS";
    public const string EnabledCommandListKey = "ENABLED_COMMAND_LIST";


    #endregion
}
