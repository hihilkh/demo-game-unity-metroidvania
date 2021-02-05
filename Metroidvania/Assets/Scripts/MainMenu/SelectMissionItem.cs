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

    private List<LocalizedTextDetails> detailsList;
    private bool isAddedLangChangedEvent = false;

    /// <summary>
    /// Init the SelectMissionItem.<br />
    /// progress = null : Use new MissionProgress () as value
    /// </summary>
    public void Init (Mission details, MissionProgress progress = null) {
        if (progress == null) {
            progress = new MissionProgress ();
        }

        button.SetOnClickInfo (BtnOnClickType.MainMenu_SelectMissionItem, details);

        SetUnlockStatus (progress.isUnlocked);
        SetCollectables (details, progress);

        detailsList = new List<LocalizedTextDetails> ();
        detailsList.Add (new LocalizedTextDetails (missionNameText, details.displayNameKey));
        detailsList.Add (new LocalizedTextDetails (progressText, FrameworkUtils.GetProgressPercentStr (GetMissionProgress (details, progress)), false));
        OnLangChanged ();

        LangManager.LangChangedEvent += OnLangChanged;
        isAddedLangChangedEvent = true;
    }

    private void OnDestroy () {
        if (isAddedLangChangedEvent) {
            LangManager.LangChangedEvent -= OnLangChanged;
        }
    }

    private void SetUnlockStatus (bool isUnlocked) {
        button.interactable = isUnlocked;
    }

    private float GetMissionProgress (Mission details, MissionProgress progress) {
        var totalCount = details.GetCollectableCount ();

        if (totalCount <= 0) {
            if (progress.isUnlocked && progress.isCleared) {
                return 1;
            } else {
                return 0;
            }
        }

        var collectedCount = 0;

        foreach (var collected in progress.collectedCollectables) {
            if (details.collectables.Contains (collected)) {
                collectedCount++;
            }
        }

        return (float)collectedCount / (float)totalCount;
    }

    private void SetCollectables (Mission details, MissionProgress progress) {
        // TODO
    }

    private void OnLangChanged () {
        LangManager.SetTexts (detailsList);
    }
}
