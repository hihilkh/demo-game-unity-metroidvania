using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.Core;

public static partial class GameVariable {

    #region Tag

    public const string DefaultTag = "Untagged";
    public const string PlayerTag = "Player";
    public const string EnemyTag = "Enemy";
    public const string GroundTag = "Ground";
    public const string SlippyWallTag = "SlippyWall";
    public const string DeathTag = "Death";
    public const string ArrowSwitchTag = "ArrowSwitch";
    public const string DropHitSwitchTag = "DropHitSwitch";

    #endregion

    #region Layer

    public const int DefaultLayer = 0;
    public const int PlayerLayer = 8;
    public const int EnemyLayer = 9;
    public const int PlayerInvincibleLayer = 10;
    public const int EnemyInvincibleLayer = 11;
    public const int PlayerInteractableLayer = 12;
    public const int PlayerAttackLayer = 13;
    public const int EnemyAttackLayer = 14;

    #endregion

    #region Scene

    public const string LandingSceneName = "Landing";
    public const string MainMenuSceneName = "MainMenu";
    public const string GameSceneName = "Game";
    public const string MapEditorSceneName = "MapEditor";

    #endregion

    #region Map Related

    public const string CharPrefabResourcesName = "Char/Character";

    public const int CharPosZ = -1;
    public const int EnemyPosZ = 0;
    public const int TilePosZ = 0;
    public const int GeneralMapItemPosZ = 0;

    #endregion

    #region Player Pref Key

    // GameConfig
    public const string BaseURLKey = "BASE_URL";
    public const string AnalyticsTypeKey = "ANALYTICS_TYPE";

    // Tutorial
    public const string HasDoneGameTutorialKey = "HAS_DONE_GAME_TUTORIAL_{0}";

    // User Manager
    public const string AllMissionProgressKey = "ALL_MISSION_PROGRESS";
    public const string EnabledCommandListKey = "ENABLED_COMMAND_LIST";
    public const string SelectedMissionIdKey = "SELECTED_MISSION_ID";
    public const string SelectedEntryIdKey = "SELECTED_ENTRY_ID";

    #endregion

    #region UIAnimation

    public const string HidePanelAnimStateName = "Hide";

    #endregion
}
