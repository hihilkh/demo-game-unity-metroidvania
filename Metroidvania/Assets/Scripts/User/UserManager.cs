using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using HIHIFramework.Core;

public static class UserManager {

    public static Dictionary<int, MissionProgress> MissionProgressDict { get; private set; }
    public static List<CharEnum.Command> EnabledCommandList { get; private set; }

    public static int SelectedMissionId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedMissionIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedMissionIdKey, value); }
    }

    public static int SelectedMapEntryId {
        get { return PlayerPrefs.GetInt (GameVariable.SelectedMapEntryIdKey, -1); }
        set { PlayerPrefs.SetInt (GameVariable.SelectedMapEntryIdKey, value); }
    }

    #region Init

    static UserManager () {
        LoadUserProgress ();
    }

    private static void LoadUserProgress () {
        Log.PrintDebug ("Load User Progress", LogType.GameFlow);
        LoadMissionProgressList ();
        LoadEnabledCommandList ();
    }

    #endregion

    #region getter

    /// <summary>
    /// Get the MissionProgress of the corresponding missionId.<br />
    /// Notes :<br />
    /// 1. The returned value is referenced in UserManager class. Do not amend it outside the UserManager class. Clone and use it if needed.<br />
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
            EnabledCommandList = new List<CharEnum.Command> ();
        } else {
            var array = str.Split (new string[] { FrameworkVariable.DefaultDelimiter }, System.StringSplitOptions.None);
            foreach (var commandIntStr in array) {
                EnabledCommandList.Add ((CharEnum.Command)int.Parse (commandIntStr));
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

    public static void ClearMission (int missionId, int? toEntryId = null) {
        var clearMissionProgress = GetMissionProgress (missionId);
        clearMissionProgress.isCleared = true;

        if (toEntryId != null) {
            var nextMissionId = MissionDetails.GetMissionIdByEntry ((int)toEntryId);
            var nextMissionProgress = GetMissionProgress (nextMissionId);
            nextMissionProgress.isUnlocked = true;

            // TODO : unlock entry
        }

        SaveMissionProgressList ();
    }

    public static void CollectedCollectable (int missionId, MapCollectable.Type collectable) {
        var missionProgress = GetMissionProgress (missionId);
        missionProgress.AddCollectedCollectable (collectable);

        SaveMissionProgressList ();
    }

    public static void EnableCommand (CharEnum.Command command) {
        if (EnabledCommandList == null) {
            EnabledCommandList = new List<CharEnum.Command> ();
        }

        if (!EnabledCommandList.Contains (command)) {
            EnabledCommandList.Add (command);
        }

        SaveEnabledCommandList ();
    }

    #endregion
}
