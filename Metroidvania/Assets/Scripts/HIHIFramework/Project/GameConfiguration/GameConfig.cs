using UnityEngine;
using HihiFramework.GameConfiguration;

public partial class GameConfig : GameConfigBase {

	#region All Game Config Set

	// sampleGameConfigSet1
	private static readonly GameConfigSet DummyConfigSet = new GameConfigSet (
		"Dummy",                               // gameConfigSetName
		"No Base URL"                           // baseURL
	);

	#endregion
}