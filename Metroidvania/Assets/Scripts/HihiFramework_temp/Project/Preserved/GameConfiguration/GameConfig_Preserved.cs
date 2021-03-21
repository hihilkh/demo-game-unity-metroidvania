using System.Collections.Generic;
using HihiFramework.GameConfiguration;

public partial class GameConfig : GameConfigBase {
	/// <summary>
	/// All the game config sets shown in GameConfig scene
	/// </summary>
	public static List<GameConfigSet> AllGameConfigSetList => new List<GameConfigSet> {
		DummyConfigSet,
	};

	/// <summary>
	/// The game config set used for release build
	/// </summary>
	public static GameConfigSet ReleaseBuildGameConfigSet => DummyConfigSet;

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
		SaveFrameworkRuntimeGameConfig (gameConfigSet);
	}
}