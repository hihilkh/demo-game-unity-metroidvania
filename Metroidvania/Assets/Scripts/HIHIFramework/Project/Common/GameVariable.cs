using UnityEngine;

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
    public const int DetectionLayer = 15;
    public const int EnemyDefendLayer = 16;

    #endregion

    #region Scene

    public const string LandingSceneName = "Landing";
    public const string MainMenuSceneName = "MainMenu";
    public const string GameSceneName = "Game";
    public const string MapEditorSceneName = "MapEditor";
    public const string SandboxSceneName = "Sandbox";

    #endregion

    #region Map Related

    public const int CharPosZ = -2;
    public const int EnemyPosZ = -1;
    public const int TilePosZ = 0;
    public const int GeneralMapItemPosZ = 0;

    #endregion

    #region Player Pref Key

    // User Manager
    public const string AllMissionProgressKey = "ALL_MISSION_PROGRESS";
    public const string CollectedCollectableTypeListKey = "COLLECTED_COLLECTABLE_TYPE_LIST";
    public const string CommandSettingsCacheKey = "COMMAND_SETTINGS_CACHE";
    public const string DoneMissionEventTypeListKey = "DONE_MISSION_EVENT_TYPE_LIST";
    public const string SelectedMissionIdKey = "SELECTED_MISSION_ID";
    public const string SelectedEntryIdKey = "SELECTED_ENTRY_ID";

    #endregion

    #region Common UI values

    public const string TransitionCanvasPrefabResourcesName = "UI/TransitionCanvas";

    public const string HidePanelAnimStateName = "Hide";

    public const string UnknownTextKey = "UnknownText";

    public static Color DisabledUIMaskColor = new Color32 (200, 200, 200, 128);
    public static Color DisabledUIMaskColor_NoAlpha = new Color32 (128, 128, 128, 255);

    #endregion

    #region FireAttack

    public static float FireAttackTriggerPeriod = 0.5f;
    public static int FireAttackNoOfTrigger = 10;
    public static int FireDamagePerTrigger = 1;

    #endregion
}
