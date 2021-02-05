using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour {
    [SerializeField] private MainMenuSceneUIManager uiManager;

    [SerializeField] private SelectMissionItem selectMissionItemTemplate;

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
        GenerateSelectMissionItems ();
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
    }

    private void GenerateSelectMissionItems () {
        var selectMissionItemList = new List<SelectMissionItem> ();

        foreach (var missionDetails in MissionManager.MissionList) {
            var clone = Instantiate<SelectMissionItem> (selectMissionItemTemplate);

            var progress = UserManager.GetMissionProgress (missionDetails.id);
            clone.Init (missionDetails, progress);

            selectMissionItemList.Add (clone);
        }

        uiManager.SetSelectMissionItems (selectMissionItemList);
    }

    private void OnSelectMissionItemClick (object info) {
        if (!(info is MissionDetails)) {
            Log.PrintError ("OnSelectMissionItemClick failed. Getting invalid info type : " + info.GetType (), LogType.UI | LogType.Input | LogType.GameFlow);
            return;
        }

        var details = (MissionDetails)info;

        // TODO
        SceneManager.LoadScene (GameVariable.GameSceneName);
    }
}
