﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HIHIFramework.GameConfiguration;

public partial class GameConfig : GameConfigBase<GameConfigSet> {

	#region All Game Config Set

	// sampleGameConfigSet1
	private GameConfigSet sampleGameConfigSet1 = new GameConfigSet (
		"SampleOnly",                               // gameConfigSetName
		"www.google.com",                           // baseURL
		GameConfigEnum.AnalyticsType.None           // analyticsType
	);

	// sampleGameConfigSet2
	private GameConfigSet sampleGameConfigSet2 = new GameConfigSet (
		"PleaseDeleteMe",                                   // gameConfigSetName
		"www.yahoo.com",                                    // baseURL
		GameConfigEnum.AnalyticsType.Firebase               // analyticsType
	);

	#endregion

	#region static Game Config

	public static string BaseURL {
		get { return PlayerPrefs.GetString (GameVariable.BaseURLKey, ""); }
		private set { PlayerPrefs.SetString (GameVariable.BaseURLKey, value); }
	}

	public static GameConfigEnum.AnalyticsType AnalyticsType {
		get {
			int analyticsTypeInt = PlayerPrefs.GetInt (GameVariable.AnalyticsTypeKey, 0);
			return (GameConfigEnum.AnalyticsType)analyticsTypeInt;
		}

		private set {
			int analyticsTypeInt = (int)value;
			PlayerPrefs.SetInt (GameVariable.AnalyticsTypeKey, analyticsTypeInt);
		}
	}

	#endregion
}