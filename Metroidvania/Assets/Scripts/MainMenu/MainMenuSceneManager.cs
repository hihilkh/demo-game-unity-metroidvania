using System.Collections;
using System.Collections.Generic;
using HIHIFramework.Core;
using HIHIFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour {
    [SerializeField] private MainMenuSceneUIManager uiManager;
    [SerializeField] private List<SelectMissionItem> selectMissionItemsInOrder;

    /// <summary>
    /// int : missionId
    /// </summary>
    private Dictionary<int, SelectMissionItem> selectMissionItemDict;

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
        InitSelectMissionItems ();

        // TODO : Move to after screen transition
        CheckEntryJustUnlocked ();
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectMissionItem, OnSelectMissionItemClick);
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

    private void OnSelectMissionItemClick (object info) {
        if (!(info is Mission)) {
            Log.PrintError ("OnSelectMissionItemClick failed. Getting invalid info type : " + info.GetType (), LogType.UI | LogType.Input | LogType.GameFlow);
            return;
        }

        var details = (Mission)info;

        // TODO
        SceneManager.LoadScene (GameVariable.GameSceneName);
    }
}
