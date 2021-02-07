using System.Collections;
using System.Collections.Generic;
using HIHIFramework.UI;
using UnityEngine;
using HIHIFramework.Core;

public class SelectMissionItem : MonoBehaviour {
    [SerializeField] private HIHIButton button;
    [SerializeField] private Animator animator;

    private const string NewEntryUnlockedStateName = "NewEntryUnlocked";

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
    }

    private void SetUnlockStatus (bool isUnlocked) {
        gameObject.SetActive (isUnlocked);
    }

    public void ShowNewEntryUnlocked () {
        transform.SetAsLastSibling ();
        animator.Play (NewEntryUnlockedStateName);
    }
}
