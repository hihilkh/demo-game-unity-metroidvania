using System.Collections;
using System.Collections.Generic;
using HIHIFramework.UI;
using UnityEngine;
using TMPro;
using HIHIFramework.Core;

public class SelectMissionItem : MonoBehaviour {
    [SerializeField] private HIHIButton button;
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI progressText;

    /// <summary>
    /// Init the SelectMissionItem.<br />progress = null : Use new MissionProgress () as value
    /// </summary>
    public void Init (MissionDetails details, MissionProgress progress = null) {
        if (progress == null) {
            progress = new MissionProgress ();
        }
        button.SetOnClickInfo (BtnOnClickType.MainMenu_SelectMissionItem, details);

        var detailsList = new List<LocalizedTextDetails> ();
        detailsList.Add (new LocalizedTextDetails (missionNameText, details.displayNameKey));
        detailsList.Add (new LocalizedTextDetails (progressText, GameUtils.GetProgressPercentStr (progress.currentProgress), false));

        SetUnlockStatus (progress.isUnlocked);
    }

    private void SetUnlockStatus (bool isUnlocked) {
        button.interactable = isUnlocked;
    }
}
