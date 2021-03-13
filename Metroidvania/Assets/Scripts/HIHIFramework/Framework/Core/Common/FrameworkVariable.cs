namespace HihiFramework.Core {
    public static class FrameworkVariable {

        public const string FrameworkVersion = "1.0.0";

        #region Common

        public const string DefaultDelimiter = "|";

        #endregion

        #region GameConfig

        public const string GameConfigSetNameFieldName = "GameConfigSetName";
        public const string GameConfigCustomStringSuffix = "_CustomString";

        #endregion

        #region Lang

        public const string LocalizationFileExtension = "tsv";
        public const string LocalizationFileDelimiter = "\t";
        public const string FontResourcesFolder = "Fonts";

        #endregion

        #region Asset

        public const string IOTempFolderName = "Temp";
        public const string AssetVersionFileExtension = "version";
        public const string AssetChecksumFileExtension = "checksum";

        #endregion

        #region GameUtils

        public const string ProgressPercentFormat = "{0}%";

        #endregion

        #region Player Pref Key

        // GameConfig
        public const string GameConfigLastTimeKey = "HIHI_GAME_CONFIG_LAST_TIME";
        public const string BaseURLKey = "BASE_URL";

        // Lang
        public const string CurrentLangTypeKey = "HIHI_CURRENT_LANG_TYPE";

        // Asset
        public const string AssetVersionKey = "HIHI_Version_{0}_{1}";

        #endregion
    }
}