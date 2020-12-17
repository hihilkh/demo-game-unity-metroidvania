using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;

public class MainMenuSceneManager : MonoBehaviour {
    [SerializeField] private MainMenuSceneUIManager uiManager;

    [SerializeField] private SelectMissionItem selectMissionItemTemplate;

    private void Start () {
        GenerateSelectMissionItems ();
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
    }

    private void nDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
    }

    private void GenerateSelectMissionItems () {
        var selectMissionItemList = new List<SelectMissionItem> ();

        foreach (var missionDetails in MissionDetails.OrderedMissionList) {
            var clone = Instantiate<SelectMissionItem> (selectMissionItemTemplate);

            var progress = GameProgress.GetMissionProgress (missionDetails.id);
            clone.Init (missionDetails, progress);

            selectMissionItemList.Add (clone);
        }

        uiManager.SetSelectMissionItems (selectMissionItemList);
    }

    private void OnSelectMissionItemClick (object info) {
        if (!(info is MissionDetails)) {
            Log.PrintError ("OnSelectMissionItemClick failed. Getting invalid info type : " + info.GetType ());
            return;
        }

        var details = (MissionDetails)info;

        // TODO
        Log.Print (details.mapSceneName);
    }
}
