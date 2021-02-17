﻿using HihiFramework.Core;
using UnityEngine;

// TODO : Replace by MissionEventManager
public class TutorialManager {
	private static string GetPlayerPrefsKey (TutorialEnum.GameTutorialType type) {
		return FrameworkUtils.StringReplace (GameVariable.HasDoneGameTutorialKey, ((int)type).ToString ());
	}

	public static bool GetHasDoneGameTutorial (TutorialEnum.GameTutorialType type) {
		return PlayerPrefs.GetInt (GetPlayerPrefsKey (type), 0) == 1;
	}

	public static void SetHasDoneGameTutorial (TutorialEnum.GameTutorialType type, bool hasDone) {
		PlayerPrefs.SetInt (GetPlayerPrefsKey (type), hasDone ? 1 : 0);
	}
}