// Remarks:
// - Do not change the int value of the corresponding event because Unity Editor save the enum as int.
// - Keep ascend the int value for new enum. Even there is a decreprated enum, do not reuse its int value.
// - int value after 9999 is preserved by Framework usage.
public enum BtnOnClickType : int {
    Landing_Start = 1,
    MainMenu_SelectMissionItem = 100,

    Game_ClickOnScreen = 200,

    GameConfig_Confirm = 10000,
    GameConfig_ClearPlayerPrefs = 10001,
}