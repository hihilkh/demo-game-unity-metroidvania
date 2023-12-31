﻿/// <summary>
/// Do not change the int value of the corresponding event because Unity Editor save the enum as int.<br />
/// Keep ascend the int value for new enum. Even there is a decreprated enum, do not reuse its int value.<br />
/// int value after 9999 is preserved by Framework usage.
/// </summary>
public enum BtnOnClickType {
    Landing_Start = 1,

    MainMenu_SelectMission = 100,
    MainMenu_SelectEntry = 101,
    MainMenu_OpenNotesPanel = 102,
    MainMenu_SelectNote = 103,

    Game_ClickOnScreen = 200,
    Game_RemoveCommand = 201,
    Game_ConfirmCommand = 202,
    Game_Pause = 203,
    Game_Restart = 204,
    Game_BackToMM = 205,
    Game_ViewEnv = 206,
    Game_ViewEnvBack = 207,

    Settings = 300,
    Settings_LangLeft = 301,
    Settings_LangRight = 302,
    Settings_Apply = 303,
    Settings_SfxLeft = 304,
    Settings_SfxRight = 305,
    Settings_BgmLeft = 306,
    Settings_BgmRight = 307,
    Settings_ViewCredits = 308,
    Settings_CloseCredits = 309,

    // Framework usage
    GameConfig_Confirm = 10000,
    GameConfig_ClearPlayerPrefs = 10001,

    Panel_CloseBtn = 10002,
}