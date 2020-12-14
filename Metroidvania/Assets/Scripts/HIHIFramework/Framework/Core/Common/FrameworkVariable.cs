using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HIHIFramework.Core {
    public class FrameworkVariable {

        public const string FrameworkVersion = "1.0.0";

        #region Common

        public const string DefaultDelimiter = "||";

        #endregion

        #region GameConfig

        public const string GameConfigSetNameFieldName = "gameConfigSetName";
        public const string GameConfigCustomStringSuffix = "_CustomString";

        #endregion

        #region Asset

        public const string IOTempFolderName = "Temp";
        public const string AssetVersionFileExtension = "version";
        public const string AssetChecksumFileExtension = "checksum";

        #endregion

        #region Player Pref Key

        // GameConfig
        public const string GameConfigLastTimeKey = "HIHI_GAME_CONFIG_LAST_TIME";

        // Asset
        public const string AssetVersionKey = "HIHI_Version_{0}_{1}";

        #endregion
    }
}