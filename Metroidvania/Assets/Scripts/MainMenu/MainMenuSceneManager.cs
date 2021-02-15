using System;
using System.Collections.Generic;
using HihiFramework.Core;
using HihiFramework.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSceneManager : MonoBehaviour {
    [SerializeField] private MainMenuSceneUIManager uiManager;
    [SerializeField] private List<SelectMissionItem> selectMissionItemsInOrder;
    [SerializeField] private MissionDetailsPanel missionDetailsPanel;
    [SerializeField] private NotesPanel notesPanel;

    /// <summary>
    /// int : missionId
    /// </summary>
    private readonly Dictionary<int, SelectMissionItem> selectMissionItemDict = new Dictionary<int, SelectMissionItem> ();

    private void Start () {
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectMission, SelectMissionBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_SelectEntry, SelectEntryBtnClickedHandler);
        UIEventManager.AddEventHandler (BtnOnClickType.MainMenu_OpenNotesPanel, OpenNotesPanelBtnClickedHandler);
        InitSelectMissionItems ();

        Action onFadeOutFinished = () => {
            CheckEntryJustUnlocked ();
        };

        GameUtils.ScreenFadeOut (onFadeOutFinished);
    }

    private void OnDestroy () {
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectMission, SelectMissionBtnClickedHandler);
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_SelectEntry, SelectEntryBtnClickedHandler);
        UIEventManager.RemoveEventHandler (BtnOnClickType.MainMenu_OpenNotesPanel, OpenNotesPanelBtnClickedHandler);
    }

    private void InitSelectMissionItems () {
        if (selectMissionItemsInOrder.Count != MissionManager.MissionListInOrder.Count) {
            Log.PrintWarning ("No. of selectMissionItems are not equal to no. of mission. Please check.", LogTypes.GameFlow);
        }

        var count = Mathf.Min (selectMissionItemsInOrder.Count, MissionManager.MissionListInOrder.Count);
        for (var i = 0; i < count; i++) {
            var mission = MissionManager.MissionListInOrder[i];

            var progress = UserManager.GetMissionProgress (mission.Id);
            selectMissionItemsInOrder[i].Init (mission, progress);

            selectMissionItemDict.Add (mission.Id, selectMissionItemsInOrder[i]);
        }
    }

    private void CheckEntryJustUnlocked () {
        if (UserManager.EntryJustUnlockedMissionId != null) {
            var missionId = (int)UserManager.EntryJustUnlockedMissionId;
            if (selectMissionItemDict.ContainsKey (missionId)) {
                selectMissionItemDict[missionId].ShowNewEntryUnlocked ();
            } else {
                Log.PrintWarning ("No mapping in selectMissionItemDict for mission id : " + missionId + " . Cannot show entry just unlocked effect.", LogTypes.GameFlow);
            }

            UserManager.ClearEntryJustUnlockedMissionId ();
        }
    }

    #region Events

    private void SelectMissionBtnClickedHandler (HIHIButton sender, object info) {
        if (!(info is Mission)) {
            Log.PrintError ("OnSelectMissionClick failed. Getting invalid info type : " + info.GetType (), LogTypes.UI | LogTypes.Input | LogTypes.GameFlow);
            return;
        }

        var mission = (Mission)info;
        missionDetailsPanel.Show (mission, UserManager.GetMissionProgress (mission.Id));
    }

    private void SelectEntryBtnClickedHandler (HIHIButton sender, object info) {
        if (!(info is Mission.Entry)) {
            Log.PrintError ("OnSelectEntryClick failed. Getting invalid info type : " + info.GetType (), LogTypes.UI | LogTypes.Input | LogTypes.GameFlow);
            return;
        }

        var entry = (Mission.Entry)info;
        var mission = MissionManager.GetMissionByEntry (entry.Id);
        UserManager.SelectedMissionId = mission.Id;
        UserManager.SelectedEntryId = entry.Id;

        Action onFadeInFinished = () => {
            SceneManager.LoadScene (GameVariable.GameSceneName);
        };

        GameUtils.ScreenFadeIn (onFadeInFinished);
    }

    private void OpenNotesPanelBtnClickedHandler (HIHIButton sender) {
        notesPanel.Show (UserManager.GetAllCollectedCollectable ());
    }

    #endregion
}
