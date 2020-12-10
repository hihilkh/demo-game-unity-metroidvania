using UnityEngine;
using HIHIFramework.GameConfiguration;
using System.Collections.Generic;

public partial class GameConfig : GameConfigBase<GameConfigSet> {
    // All the game config sets shown in GameConfig scene
    public override List<GameConfigSet> allGameConfigSetList {
		get {
			return new List<GameConfigSet> {
				sampleGameConfigSet1,
				sampleGameConfigSet2
			};
		}
	}

	// The game config set used for release build
	public override GameConfigSet releaseBuildGameConfigSet {
		get {
			return sampleGameConfigSet1;
		}
	}

	public override GameConfigSet GetEmptyGameConfigSet () {
		return new GameConfigSet ();
	}

	public override void SetRuntimeGameConfig (GameConfigSet gameConfigSet) {
		BaseURL = gameConfigSet.baseURL;
		AnalyticsType = gameConfigSet.analyticsType;
	}
}