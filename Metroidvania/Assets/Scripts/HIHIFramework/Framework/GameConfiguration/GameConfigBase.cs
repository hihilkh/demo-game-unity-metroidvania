using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HIHIFramework.GameConfiguration {
    public abstract class GameConfigBase<T> where T : GameConfigSetBase {

        #region abstract part

        public abstract List<T> allGameConfigSetList { get; }        // All the game config sets shown in GameConfig scene
        public abstract T releaseBuildGameConfigSet { get; }        // The game config set used for release build

        public abstract T GetEmptyGameConfigSet ();
        public abstract void SetRuntimeGameConfig (T gameConfigSet);

        #endregion
    }
}