// Remarks:
// - Do not change the int value of the corresponding event because Unity Editor save the enum as int.
// - Keep ascend the int value for new enum. Even there is a decreprated enum, do not reuse its int value.
// - int value after 9999 is preserved by Framework usage.
public enum BtnOnClickType : int {
    Landing_Start = 1,

    MainMenu_SelectMission = 100,
    MainMenu_CloseMissionDetailsPanel = 101,
    MainMenu_SelectEntry = 102,
    MainMenu_OpenNotesPanel = 103,
    MainMenu_CloseNotesPanel = 104,
    MainMenu_SelectNote = 105,

    Game_ClickOnScreen = 200,

    GameConfig_Confirm = 10000,
    GameConfig_ClearPlayerPrefs = 10001,
}