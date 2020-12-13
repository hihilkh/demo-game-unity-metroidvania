// Remarks:
// - Do not change the int value of the corresponding event because Unity Editor save the enum as int.
// - Keep ascend the int value for new enum. Even there is a decreprated enum, do not reuse its int value.
// - int value after 999 is preserved by Framework usage.
public enum BtnOnClickType {
    Landing_Start = 1,

    GameConfig_Confirm = 1000,
    GameConfig_ClearPlayerPrefs = 1001,
}