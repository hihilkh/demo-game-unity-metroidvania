public static partial class AudioEnum {
    public enum BgmType {
        None,           // Used by framework. Do not delete. Means no BGM need to be played.
        General,
    }

    /// <summary>
    /// Do not change the int value of the corresponding event because Unity Editor save the enum as int.<br />
    /// HihiButton would use this enum.
    /// </summary>
    public enum DynamicSfxType {
        None = 0,           // Used by framework. Do not delete. Means no SFX need to be played.
        ConfirmBtn = 1,
        CancelBtn = 2,
        StartGame = 3,
    }
}