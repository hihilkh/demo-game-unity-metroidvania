using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using HIHIFramework.Core;

public static class GameProgress {

    public static Dictionary<int, MissionProgress> MissionProgressDict { get; private set; }
    public static List<CharacterEnum.Command> EnabledCommandList { get; private set; }

    #region Init

    static GameProgress () {
        LoadProgress ();
    }

    private static void LoadProgress () {
        Log.PrintDebug ("Load Game Progress");
        LoadMissionProgressList ();
        LoadEnabledCommandList ();
    }

    #endregion

    #region getter

    /// <summary>
    /// Get the MissionProgress of the corresponding missionId.<br />
    /// Notes :<br />
    /// 1. The returned value is referenced in GameProgress class. Do not amend it outside the GameProgress class. Clone and use it if needed.<br />
    /// 2. The returned value is ensured not be null
    /// </summary>
    public static MissionProgress GetMissionProgress (int missionId) {
        // Remarks : The static constructor ensure MissionProgressDict != null
        if (MissionProgressDict.ContainsKey (missionId)) {
            return MissionProgressDict[missionId];
        } else {
            var missionProgress = new MissionProgress ();
            MissionProgressDict.Add (missionId, missionProgress);
            return missionProgress;
        }
    }

    #endregion

    #region PlayerPrefs (Save / Load)

    private static void SaveMissionProgressList () {
        var json = "";
        if (MissionProgressDict != null && MissionProgressDict.Count > 0) {
            var allMissionProgress = new AllMissionProgress (MissionProgressDict);
            json = JsonUtility.ToJson (allMissionProgress);
        }

        PlayerPrefs.SetString (GameVariable.AllMissionProgressKey, json);
    }

    private static void LoadMissionProgressList () {
        var json = PlayerPrefs.GetString (GameVariable.AllMissionProgressKey, null);

        if (string.IsNullOrEmpty (json)) {
            MissionProgressDict = new Dictionary<int, MissionProgress> ();
        } else {
            var allMissionProgress = new AllMissionProgress ();
            allMissionProgress = JsonUtility.FromJson<AllMissionProgress> (json);
            MissionProgressDict = allMissionProgress.ConvertToDict ();
        }

        SetFirstMissionUnlocked ();
    }

    private static void SaveEnabledCommandList () {
        var str = "";
        if (EnabledCommandList != null && EnabledCommandList.Count > 0) {
            var tempList = new List<int> ();
            foreach (var command in EnabledCommandList) {
                tempList.Add ((int)command);
            }

            str = string.Join (FrameworkVariable.DefaultDelimiter, tempList);
        }

        PlayerPrefs.SetString (GameVariable.EnabledCommandListKey, str);
    }

    private static void LoadEnabledCommandList () {
        var str = PlayerPrefs.GetString (GameVariable.EnabledCommandListKey, null);

        if (string.IsNullOrEmpty (str)) {
            EnabledCommandList = new List<CharacterEnum.Command> ();
        } else {
            var array = str.Split (new string[] { FrameworkVariable.DefaultDelimiter }, System.StringSplitOptions.None);
            foreach (var commandIntStr in array) {
                EnabledCommandList.Add ((CharacterEnum.Command)int.Parse (commandIntStr));
            }
        }
    }

    #endregion

    #region Logic

    private static void SetFirstMissionUnlocked () {
        var firstMissionId = MissionDetails.OrderedMissionList[0].id;
        var progress = GetMissionProgress (firstMissionId);

        if (!progress.isUnlocked) {
            progress.isUnlocked = true;
            SaveMissionProgressList ();
        }
    }

    public static void ClearMission (int missionId) {
        var clearMissionProgress = GetMissionProgress (missionId);
        clearMissionProgress.isCleared = true;

        var clearMissionIndex = MissionDetails.OrderedMissionList.FindIndex (x => x.id == missionId);
        if (clearMissionIndex < 0) {
            Log.PrintWarning ("You has just cleared a mission which is not inside OrderedMissionList. Please check. Mission Id : " + missionId);
        } else {
            if (clearMissionIndex + 1 < MissionDetails.OrderedMissionList.Count) { // Check if there is next mission or not
                var nextMissionId = MissionDetails.OrderedMissionList[clearMissionIndex + 1].id;
                var nextMissionProgress = GetMissionProgress (nextMissionId);
                nextMissionProgress.isUnlocked = true;
            }
        }

        SaveMissionProgressList ();
    }

    public static void CollectedCollectable (int missionId, MissionCollectable.Type collectable) {
        var missionProgress = GetMissionProgress (missionId);
        missionProgress.AddCollectedCollectable (collectable);

        SaveMissionProgressList ();
    }

    public static void EnableCommand (CharacterEnum.Command command) {
        if (EnabledCommandList == null) {
            EnabledCommandList = new List<CharacterEnum.Command> ();
        }

        if (!EnabledCommandList.Contains (command)) {
            EnabledCommandList.Add (command);
        }

        SaveEnabledCommandList ();
    }

    #endregion
}
