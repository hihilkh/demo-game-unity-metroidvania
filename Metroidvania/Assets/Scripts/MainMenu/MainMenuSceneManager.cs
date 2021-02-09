using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour {
    [SerializeField] private MainMenuSceneUIManager uiManager;
    [SerializeField] private List<SelectMissionItem> selectMissionItemsInOrder;
    [SerializeField] private MissionDetailsPanel missionDetailsPanel;

    /// <summary>
    /// int : missionId
    /// </summary>
    private Dictionary<int, SelectMissionItem> selectMissionItemDict;

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectMission, OnSelectMissionClick);
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectEntry, OnSelectEntryClick);
        InitSelectMissionItems ();

        // TODO : Move to after screen transition
        CheckEntryJustUnlocked ();
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectMission, OnSelectMissionClick);
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectEntry, OnSelectEntryClick);
    }

    private void InitSelectMissionItems () {
        selectMissionItemDict = new Dictionary<int, SelectMissionItem> ();

        if (selectMissionItemsInOrder.Count != MissionManager.MissionListInOrder.Count) {
            Log.PrintWarning ("No. of selectMissionItems are not equal to no. of mission. Please check.", LogType.GameFlow);
        }

        var count = Mathf.Min (selectMissionItemsInOrder.Count, MissionManager.MissionListInOrder.Count);
        for (var i = 0; i < count; i++) {
            var mission = MissionManager.MissionListInOrder[i];

            var progress = UserManager.GetMissionProgress (mission.id);
            selectMissionItemsInOrder[i].Init (mission, progress);

            selectMissionItemDict.Add (mission.id, selectMissionItemsInOrder[i]);
        }
    }

    private void CheckEntryJustUnlocked () {
        if (UserManager.EntryJustUnlockedMissionId != null) {
            var missionId = (int)UserManager.EntryJustUnlockedMissionId;
            if (selectMissionItemDict.ContainsKey (missionId)) {
                selectMissionItemDict[missionId].ShowNewEntryUnlocked ();
            } else {
                Log.PrintWarning ("No mapping in selectMissionItemDict for mission id : " + missionId + " . Cannot show entry just unlocked effect.", LogType.GameFlow);
            }

            UserManager.ClearEntryJustUnlockedMissionId ();
        }
    }

    private void OnSelectMissionClick (object info) {
        if (!(info is Mission)) {
            Log.PrintError ("OnSelectMissionClick failed. Getting invalid info type : " + info.GetType (), LogType.UI | LogType.Input | LogType.GameFlow);
            return;
        }

        var mission = (Mission)info;
        missionDetailsPanel.Show (mission, UserManager.GetMissionProgress (mission.id));
    }

    private void OnSelectEntryClick (object info) {
        if (!(info is Mission.MapEntry)) {
            Log.PrintError ("OnSelectEntryClick failed. Getting invalid info type : " + info.GetType (), LogType.UI | LogType.Input | LogType.GameFlow);
            return;
        }

        var entry = (Mission.MapEntry)info;
        var mission = MissionManager.GetMissionByMapEntry (entry.id);
        UserManager.SelectedMissionId = mission.id;
        UserManager.SelectedMapEntryId = entry.id;

        // TODO
        SceneManager.LoadScene (GameVariable.GameSceneName);
    }
}
