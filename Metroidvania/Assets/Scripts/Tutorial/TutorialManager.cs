using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager {
	public static bool HasDoneTutorial_Opening {
		get { return PlayerPrefs.GetInt (GameVariable.HasDoneOpeningTutorialKey, 0) == 1; }
		private set { PlayerPrefs.SetInt (GameVariable.HasDoneOpeningTutorialKey, value ? 1 : 0); }
	}
}