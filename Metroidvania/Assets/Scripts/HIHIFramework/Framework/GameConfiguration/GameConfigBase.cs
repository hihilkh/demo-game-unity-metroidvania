using UnityEngine;
using HihiFramework.Core;

namespace HihiFramework.GameConfiguration {
    public abstract class GameConfigBase {

		#region static Game Config

		public static string BaseURL {
			get { return PlayerPrefs.GetString (FrameworkVariable.BaseURLKey, ""); }
			private set { PlayerPrefs.SetString (FrameworkVariable.BaseURLKey, value); }
		}

        #endregion

        #region Save Framework Runtime Game Config

        /// <summary>
        /// Save selected game config set for runtime usage
        /// </summary>
        protected static void SaveFrameworkRuntimeGameConfig (GameConfigSetBase gameConfigSet) {
			BaseURL = gameConfigSet.BaseURL;
		}

		#endregion
	}
}