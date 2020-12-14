using UnityEngine;
using HIHIFramework.GameConfiguration;
using System.Collections.Generic;

public partial class GameConfig {
	/// <summary>
	/// All the game config sets shown in GameConfig scene
	/// </summary>
	public static readonly List<GameConfigSet> AllGameConfigSetList = new List<GameConfigSet> {
        SampleGameConfigSet1,
		SampleGameConfigSet2
	};

	/// <summary>
	/// The game config set used for release build
	/// </summary>
	public static readonly GameConfigSet ReleaseBuildGameConfigSet = SampleGameConfigSet1;

	/// <summary>
	/// Get empty game config set
	/// </summary>
	public static GameConfigSet GetEmptyGameConfigSet () {
		return new GameConfigSet ();
	}

	/// <summary>
	/// Save selected game config set for runtime usage
	/// </summary>
	public static void SaveRuntimeGameConfig (GameConfigSet gameConfigSet) {
		BaseURL = gameConfigSet.baseURL;
		AnalyticsType = gameConfigSet.analyticsType;
	}
}