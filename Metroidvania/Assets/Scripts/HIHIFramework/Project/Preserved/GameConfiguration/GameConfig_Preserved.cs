using System.Collections.Generic;

public static partial class GameConfig {
	/// <summary>
	/// All the game config sets shown in GameConfig scene
	/// </summary>
	public static List<GameConfigSet> AllGameConfigSetList { get; } = new List<GameConfigSet> {
		SampleGameConfigSet1,
		SampleGameConfigSet2,
	};

	/// <summary>
	/// The game config set used for release build
	/// </summary>
	public static GameConfigSet ReleaseBuildGameConfigSet { get; } = SampleGameConfigSet1;

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
		BaseURL = gameConfigSet.BaseURL;
		AnalyticsType = gameConfigSet.AnalyticsType;
	}
}