using System;
using System.Collections;
using System.Collections.Generic;
using HIHIFramework.UI;
using UnityEngine;
using TMPro;
using HIHIFramework.Core;

public class MissionDetailsPanel : MonoBehaviour {

    [SerializeField] private Animator animator;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI selectEntryText;

    [SerializeField] private List<HIHIButton> entryBtnList;
    private List<TextMeshProUGUI> entryBtnTextParallelList = new List<TextMeshProUGUI> ();

    [SerializeField] private Transform collectableDescriptionBaseTransform;
    [SerializeField] private CollectableDescription collectableDescriptionTemplate;
 
    private bool isInitialized = false;

    private List<CollectableDescription> collectableDescriptionList = new List<CollectableDescription> ();

    private void Init () {
        if (isInitialized) {
            return;
        }

        isInitialized = true;

        entryBtnTextParallelList.Clear ();
        foreach (var btn in entryBtnList) {
            var text = btn.GetComponentInChildren<TextMeshProUGUI> ();
            if (text == null) {
                Log.PrintError ("Cannot get text component of entry button. Please check.", LogType.UI);
            } else {
                entryBtnTextParallelList.Add (text);
            }
        }

        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_CloseMissionDetailsPanel, OnCloseButtonClick);
    }

    private void OnDestroy () {
        if (isInitialized) {
            UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_CloseMissionDetailsPanel, OnCloseButtonClick);
        }
    }

    public void Show (Mission mission, MissionProgress progress) {
        Init ();

        GenerateProgress (mission, progress);
        GenerateEntryBtn (mission, progress);

        var localizedTextDetailsList = new List<LocalizedTextDetails> ();
        localizedTextDetailsList.Add (new LocalizedTextDetails (progressText, "Progress"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (selectEntryText, "SelectEntry"));
        localizedTextDetailsList.Add (new LocalizedTextDetails (titleText, mission.displayNameKey));
        LangManager.SetTexts (localizedTextDetailsList);

        gameObject.SetActive (true);
    }

    private void Hide () {
        Action onFinished = () => {
            gameObject.SetActive (false);
        };

        FrameworkUtils.Instance.StartSingleAnim (animator, GameVariable.HidePanelAnimStateName, onFinished);
    }

    private void OnCloseButtonClick () {
        Hide ();
    }

    private void GenerateProgress (Mission mission, MissionProgress progress) {
        var collectableList = new List<Collectable> ();
        foreach (var type in mission.collectableTypes) {
            var collectable = CollectableManager.GetCollectable (type);
            if (collectable != null) {
                collectableList.Add (collectable);
            }
        }

        var missingCount = collectableList.Count - collectableDescriptionList.Count;
        for (var i = 0; i < missingCount; i ++) {
            var collectableDescription = Instantiate (collectableDescriptionTemplate);
            collectableDescriptionList.Add (collectableDescription);
            FrameworkUtils.InsertChildrenToParent (collectableDescriptionBaseTransform, collectableDescription, true);
        }

        for (var i = 0; i < collectableDescriptionList.Count; i++) {
            var collectableDescription = collectableDescriptionList[i];
            if (i < collectableList.Count) {
                var collectable = collectableList[i];
                var isCollected = progress.collectedCollectables.Contains (collectable.type);
                collectableDescription.Show (collectable, isCollected);
            } else {
                collectableDescription.Hide ();
            }
        }
    }

    private void GenerateEntryBtn (Mission mission, MissionProgress progress) {
        var entryList = new List<Mission.MapEntry> ();

        foreach (var entryId in progress.unlockedMapEntryIds) {
            var entry = mission.GetMapEntry (entryId);
            if (entry != null) {
                entryList.Add (entry);
            }
        }

        if (entryList.Count > entryBtnList.Count) {
            Log.PrintError ("Current UI design do not have enough entry button.", LogType.GameFlow | LogType.UI);
            return;
        }

        for (var i = 0; i < entryBtnList.Count; i++) {
            var btn = entryBtnList[i];
            if (i < entryList.Count) {
                btn.gameObject.SetActive (true);
                btn.SetOnClickInfo (BtnOnClickType.MainMenu_SelectEntry, entryList[i]);
                LangManager.SetText (new LocalizedTextDetails (entryBtnTextParallelList[i], entryList[i].displayNameKey));
            } else {
                btn.gameObject.SetActive (false);
            }
        }
    }
}
